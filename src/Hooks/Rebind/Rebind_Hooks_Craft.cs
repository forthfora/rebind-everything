using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace RebindEverything;

public static class Rebind_Hooks_Craft
{
    public static void ApplyHooks()
    {
        try
        {
            IL.Player.GrabUpdate += PlayerOnGrabUpdate_Craft;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    // Craft
    private static void PlayerOnGrabUpdate_Craft(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.Before,
            x => x.MatchLdcI4(-1),
            x => x.MatchStloc(7)))
        {
            throw new Exception("Goto Failed");
        }


        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<Player>>((self) =>
        {
            var module = self.GetModule();

            module.IsCrafting = false;

            if (!self.CraftPressed())
            {
                return;
            }

            if (!ModManager.MSC)
            {
                return;
            }

            var cachedInput = self.input[0].y;

            self.input[0].y = 1;

            var graspsCanBeCrafted = self.GraspsCanBeCrafted();

            self.input[0].y = cachedInput;

            if (!graspsCanBeCrafted)
            {
                return;
            }

            self.craftingObject = true;
            module.IsCrafting = true;
        });


        c.Emit(OpCodes.Ldloc_1);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<bool, Player, bool>>((flag3, self) =>
        {
            if (!self.IsCraftCustomInput())
            {
                return flag3;
            }

            var module = self.GetModule();

            if (module.IsCrafting)
            {
                return true;
            }

            return flag3;
        });
        c.Emit(OpCodes.Stloc_1);


        c.Emit(OpCodes.Ldloc, 6);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<int, Player, int>>((num5, self) =>
        {
            if (!self.IsCraftCustomInput())
            {
                return num5;
            }

            var module = self.GetModule();

            if (module.IsCrafting)
            {
                return -1;
            }

            return num5;
        });
        c.Emit(OpCodes.Stloc, 6);


        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchBrfalse(out _),
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<Player>(nameof(Player.GraspsCanBeCrafted))))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(Input_Helpers.IsCraftCustomInput);
        c.Emit(OpCodes.And);
    }
}
