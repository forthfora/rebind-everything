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
        var afterPoleGrabCheck = c.DefineLabel();
        var afterInputCheck = c.DefineLabel();

	if (!c.TryGotoNext(MoveType.Before,
		x => x.MatchStfld<Player>("wantToGrab")))
	{
            throw new Exception("Goto Failed");
	}

        // Get input skip destination AND check skip destination
        if (!c.TryGotoNext(MoveType.Before,
		x => x.MatchLdarg(0),
		x => x.MatchCall<Player>("get_input"),
		x => x.MatchLdcI4(0),
		x => x.MatchLdelema<Player.InputPackage>(),
		x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.y)),
		x => x.MatchLdcI4(0),
		x => x.MatchBle(out afterInputCheck)))
        {
            throw new Exception("Goto Failed");
        }
        c.Emit(OpCodes.Br, afterPoleGrabCheck);
        c.Index += 8;
        c.MarkLabel(afterPoleGrabCheck);
        c.EmitDelegate<Func<Player, bool>>(Input_Helpers.PoleGrabPressed);
        c.Emit(OpCodes.Brfalse, afterInputCheck);
    }
}
