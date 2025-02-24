using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace RebindEverything;

public static class Rebind_Hooks_Back
{
    public static void ApplyHooks()
    {
        try
        {
            IL.Player.GrabUpdate += PlayerOnGrabUpdate_Back;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            IL.Player.SpearOnBack.Update += SpearOnBack_UpdateIL;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            IL.Player.SlugOnBack.Update += SlugOnBack_UpdateIL;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    // Back Spears & Slugs
    private static void PlayerOnGrabUpdate_Back(ILContext il)
    {
        var c = new ILCursor(il);

        // Disable Setting Increment to False
        var afterIncrementFalseSpear = c.DefineLabel();

        while (c.TryGotoNext(MoveType.Before,
                   x => x.MatchLdarg(0),
                   x => x.MatchLdfld<Player>(nameof(Player.spearOnBack)),
                   x => x.MatchBrfalse(out afterIncrementFalseSpear),
                   x => x.MatchLdarg(0),
                   x => x.MatchLdfld<Player>(nameof(Player.spearOnBack)),
                   x => x.MatchLdcI4(0),
                   x => x.MatchStfld<Player.SpearOnBack>(nameof(Player.SpearOnBack.increment))))
        {
            c.Index++;
            c.Emit(OpCodes.Pop);

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(Input_Helpers.IsBackSpearCustomInput);
            c.Emit(OpCodes.Brtrue, afterIncrementFalseSpear);

            c.Emit(OpCodes.Ldarg_0);

            c.Index++;
        }

        c.Index = 0;

        var afterIncrementFalseSlug = c.DefineLabel();

        while (c.TryGotoNext(MoveType.Before,
                   x => x.MatchLdarg(0),
                   x => x.MatchLdfld<Player>(nameof(Player.slugOnBack)),
                   x => x.MatchBrfalse(out afterIncrementFalseSlug),
                   x => x.MatchLdarg(0),
                   x => x.MatchLdfld<Player>(nameof(Player.slugOnBack)),
                   x => x.MatchLdcI4(0),
                   x => x.MatchStfld<Player.SlugOnBack>(nameof(Player.SlugOnBack.increment))))
        {
            c.Index++;
            c.Emit(OpCodes.Pop);

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(Input_Helpers.IsBackSlugCustomInput);
            c.Emit(OpCodes.Brtrue, afterIncrementFalseSlug);

            c.Emit(OpCodes.Ldarg_0);

            c.Index++;
        }

        c.Index = 0;


        // Custom Input Checks
        if (!c.TryGotoNext(MoveType.Before,
            x => x.MatchLdcI4(-1),
            x => x.MatchStloc(7)))
        {
            throw new Exception("Goto Failed");
        }

        // Slug To Back
        c.Emit(OpCodes.Ldloc, 7);
        c.Emit(OpCodes.Ldarg_0);

        c.EmitDelegate<Action<int, Player>>((grasps, self) =>
        {
            if (self.slugOnBack is null)
            {
                return;
            }

            if (!self.IsBackSlugCustomInput())
            {
                return;
            }

            var module = self.GetModule();


            if (self.slugOnBack.HasASlug && !module.HadASlug && self.BackSlugPressed())
            {
                module.CanTakeSlugOffBack = false;
            }

            module.HadASlug = self.slugOnBack.HasASlug;

            if (!self.BackSlugPressed())
            {
                module.CanTakeSlugOffBack = true;
            }

            if (!module.CanTakeSlugOffBack && self.slugOnBack.HasASlug)
            {
                return;
            }


            var hasFreeHand = grasps > -1;
            var holdingSlug = false;

            if (self.CanPutSlugToBack)
            {
                for (var i = 0; i < 2; i++)
                {
                    if (self.grasps[i] is null)
                    {
                        continue;
                    }

                    if (self.grasps[i].grabbed is not Player player)
                    {
                        continue;
                    }

                    if (player.dead)
                    {
                        continue;
                    }

                    holdingSlug = true;
                    break;
                }
            }

            if (self.spearOnBack is { HasASpear: true })
            {
                return;
            }

            if (hasFreeHand || self.CanRetrieveSlugFromBack || holdingSlug)
            {
                self.slugOnBack.increment = self.BackSlugPressed();
            }
        });

        // Spear To Back
        c.Emit(OpCodes.Ldloc, 5);
        c.Emit(OpCodes.Ldarg_0);

        c.EmitDelegate<Action<int, Player>>((grasps, self) =>
        {
            if (self.spearOnBack is null)
            {
                return;
            }

            if (!self.IsBackSpearCustomInput())
            {
                return;
            }

            var hasFreeHand = grasps > -1;
            var holdingSpear = false;

            if (self.CanPutSpearToBack)
            {
                for (var m = 0; m < 2; m++)
                {
                    if (self.grasps[m] is null || self.grasps[m].grabbed is not Spear)
                    {
                        continue;
                    }

                    holdingSpear = true;
                    break;
                }
            }

            if (hasFreeHand || self.CanRetrieveSpearFromBack || holdingSpear)
            {
                self.spearOnBack.increment = self.BackSpearPressed();
            }
        });



        // Disable First Increment True Checks
        var afterSlugToBack = c.DefineLabel();

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt<Player>("get_CanRetrieveSlugFromBack"),
            x => x.MatchBrfalse(out afterSlugToBack)))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoPrev(MoveType.Before,
            x => x.MatchLdloc(7),
            x => x.MatchLdcI4(-1),
            x => x.MatchBgt(out _)))
        {
            throw new Exception("Goto Failed");
        }

        c.Index++;
        c.Emit(OpCodes.Pop);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate(Input_Helpers.IsBackSlugCustomInput);
        c.Emit(OpCodes.Brtrue, afterSlugToBack);

        c.Emit(OpCodes.Ldloc, 7);


        var afterSpearToBack = c.DefineLabel();

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt<Player>("get_CanRetrieveSpearFromBack"),
            x => x.MatchBrfalse(out afterSpearToBack)))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoPrev(MoveType.Before,
            x => x.MatchLdloc(5),
            x => x.MatchLdcI4(-1),
            x => x.MatchBgt(out _)))
        {
            throw new Exception("Goto Failed");
        }

        c.Index++;
        c.Emit(OpCodes.Pop);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate(Input_Helpers.IsBackSpearCustomInput);
        c.Emit(OpCodes.Brtrue, afterSpearToBack);

        c.Emit(OpCodes.Ldloc, 5);



        // Disable 2nd Increment True Checks

        // Move Closer to target
        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<Creature>("get_grasps"),
            x => x.MatchLdloc(28)))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchBgt(out _),
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<Player>("get_CanRetrieveSlugFromBack")))
        {
            throw new Exception("Goto Failed");
        }


        // Back Slugpup
        if (!c.TryGotoPrev(MoveType.After,
            x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp))))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(Input_Helpers.IsBackSlugCustomInput);
        c.Emit(OpCodes.And);


        // Back Spear
        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp))))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(Input_Helpers.IsBackSpearCustomInput);
        c.Emit(OpCodes.And);
    }

    private static void SlugOnBack_UpdateIL(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp))))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player.SlugOnBack, bool>>(self => self.owner.BackSlugPressed());
    }

    private static void SpearOnBack_UpdateIL(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp))))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player.SpearOnBack, bool>>(self => self.owner.BackSpearPressed());
    }
}
