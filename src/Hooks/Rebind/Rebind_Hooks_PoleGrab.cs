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

    // Pole Grabbing
    private static void PlayerOnMovementUpdate_PoleGrab(ILContext il)
    {
        var c = new ILCursor(il);

        var afterInputCheck = c.DefineLabel();

        // Get input skip destination
        if (!c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
		    x => x.MatchLdarg(0),
		    x => x.MatchCallOrCallvirt<Player>("get_input"),
		    x => x.MatchLdcI4(0),
		    x => x.MatchLdelema<Player.InputPackage>(),
		    x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.y)),
		    x => x.MatchLdcI4(0),
		    x => x.MatchBle(out afterInputCheck)))
        {
            throw new Exception("Goto Failed");
        }

        var origPath = c.MarkLabel();

        if (!c.TryGotoPrev(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Player>("get_input")))
        {
            throw new Exception("Goto Failed");
        }

        // Follow the original path if we're not using the custom bind
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(player => player.IsPoleGrabCustomInput());
        c.Emit(OpCodes.Brfalse, origPath);

        // Push 1 in-case we jump
        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldc_I4_1);
        c.Emit<Player>(OpCodes.Stfld, nameof(Player.wantToGrab));

        // Jump if the custom input is pressed
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(player => player.PoleGrabPressed());
        c.Emit(OpCodes.Brtrue, afterInputCheck);

        // Pop 1, return to orig path
        c.Emit(OpCodes.Pop);
        c.Emit(OpCodes.Br, origPath);
    }
}
