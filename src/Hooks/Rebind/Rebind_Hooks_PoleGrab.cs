using ImprovedInput;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace RebindEverything;

public static class Rebind_Hooks_PoleGrab
{
    public static void ApplyHooks()
    {
        try
        {
            IL.Player.MovementUpdate += PlayerOnMovementUpdate_PoleGrab;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    private static void PlayerOnMovementUpdate_PoleGrab(ILContext il)
    {
        var c = new ILCursor(il);

        var failDest = c.DefineLabel();

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchStfld<Player>(nameof(Player.wantToGrab)),
            x => x.MatchLdarg(0),
		    x => x.MatchLdfld<Player>(nameof(Player.wantToGrab)),
            x => x.MatchLdcI4(0),
            x => x.MatchBle(out failDest)))
        {
            throw new Exception("Goto Failed");
        }

        var skipDest = c.MarkLabel();

        if (!c.TryGotoPrev(MoveType.Before,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Player>(nameof(Player.wantToGrab))))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(x => !x.IsPoleGrabCustomInput() || x.PoleGrabPressed());
        c.Emit(OpCodes.Brfalse, failDest);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(x => x.IsPoleGrabCustomInput());
        c.Emit(OpCodes.Brtrue, skipDest);
    }
}
