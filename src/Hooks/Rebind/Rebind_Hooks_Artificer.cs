using Expedition;
using ImprovedInput;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace RebindEverything;

public static class Rebind_Hooks_Artificer
{
    public static void ApplyHooks()
    {
        On.Player.checkInput += Player_checkInput;

        try
        {
            IL.Player.ClassMechanicsArtificer += Player_ClassMechanicsArtificer_Jump;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            IL.Player.ClassMechanicsArtificer += Player_ClassMechanicsArtificer_Parry;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    // Replicate the normally required inputs to make gameplay with the rebinds more legitimate
    private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);

        if (self.SlugCatClass != MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer && (!ExpeditionGame.explosivejump || self.isSlugpup))
        {
            return;
        }

        if (!ModOptions.ArtiJumpInput.Value)
        {
            return;
        }


        var eatFlag = self.eatMeat >= 20 || self.maulTimer >= 15;
        var isParryOverride = self.IsArtiParryCustomInput() && self.input[0].y < 0;


        var artiJumpInput = self.IsArtiJumpCustomInput() && (Input_Helpers.MouseButtonPressed(ModOptions.MouseButtonArtiJump.Value) || self.IsPressed(Input_Helpers.ArtiJump)) && self.canJump <= 0 && !eatFlag && self.bodyMode == Player.BodyModeIndex.Default && self.gravity != 0.0f && !isParryOverride;

        if (artiJumpInput)
        {
            self.input[0].jmp = true;
        }


        var artiParryInput = self.IsArtiParryCustomInput() && (Input_Helpers.MouseButtonPressed(ModOptions.MouseButtonArtiParry.Value) || self.IsPressed(Input_Helpers.ArtiParry)) && !self.submerged && !eatFlag && self.gravity > 0.0f;

        if (!artiJumpInput && artiParryInput)
        {
            self.input[0].y = -1;
        }

        if (artiParryInput)
        {
            self.input[0].jmp = true;
        }
    }

    // Jump
    private static void Player_ClassMechanicsArtificer_Jump(ILContext il)
    {
        var c = new ILCursor(il);

        var afterJumpInput = c.DefineLabel();
        var afterJump = c.DefineLabel();

        // Get after jump input checks
        if (!c.TryGotoNext(MoveType.Before,
                x => x.MatchLdfld<Player>(nameof(Player.bodyMode)),
                x => x.MatchLdsfld<Player.BodyModeIndex>(nameof(Player.BodyModeIndex.ZeroG)),
                x => x.Match(OpCodes.Call),
                x => x.MatchBrtrue(out afterJumpInput)))
        {
            throw new Exception("Goto Failed");
        }

        // Get after jump block
        if (!c.TryGotoNext(MoveType.Before,
            x => x.MatchLdcR4(0.1f),
            x => x.MatchBgtUn(out afterJump)))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoPrev(MoveType.Before,
            x => x.MatchLdloc(0),
            x => x.MatchBrfalse(out _),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Player>(nameof(Player.pyroJumpped))))
        {
            throw new Exception("Goto Failed");
        }

        c.Index++;
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Ldarg_0);

        c.EmitDelegate<Func<Player, bool>>(Input_Helpers.ArtiJumpPressed);

        // Custom check branch
        c.Emit(OpCodes.Brtrue, afterJumpInput);

        // Branch after if check returns false
        c.Emit(OpCodes.Br, afterJump);
        c.Emit(OpCodes.Ldloc, 0);
    }

    // Parry
    private static void Player_ClassMechanicsArtificer_Parry(ILContext il)
    {
        var c = new ILCursor(il);

        var afterParryInput = c.DefineLabel();
        var afterParry = c.DefineLabel();

        if (!c.TryGotoNext(MoveType.Before,
            x => x.Match(OpCodes.Call),
            x => x.MatchBrfalse(out _),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Player>(nameof(Player.canJump)),
            x => x.MatchLdcI4(0),
            x => x.MatchBgt(out afterParryInput)))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoPrev(MoveType.Before,
            x => x.MatchLdloc(0),
            x => x.MatchBrfalse(out afterParry),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Player>(nameof(Player.submerged))))
        {
            throw new Exception("Goto Failed");
        }

        c.Index++;
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Ldarg_0);

        c.EmitDelegate<Func<Player, bool>>(Input_Helpers.ArtiParryPressed);

        c.Emit(OpCodes.Brtrue, afterParryInput);
        c.Emit(OpCodes.Br, afterParry);

        c.Emit(OpCodes.Ldloc, 0);
    }
}
