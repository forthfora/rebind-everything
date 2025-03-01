using ImprovedInput;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;

namespace RebindEverything;

public static class Rebind_Hooks_GrappleWorm
{
    public static void ApplyHooks()
    {
        On.TubeWorm.GrabbedByPlayer += TubeWorm_GrabbedByPlayer;

        try
        {
            IL.Player.Update += Player_Update_GrappleWorm;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    private static void TubeWorm_GrabbedByPlayer(On.TubeWorm.orig_GrabbedByPlayer orig, TubeWorm self)
    {
        if (self.grabbedBy.FirstOrDefault(x => x.grabber is Player)?.grabber is not Player player)
        {
            orig(self);
            return;
        }

        var wasJmpInput = player.input[0].jmp;

        if (player.IsGrappleCustomInput())
        {
            var grappleInput = player.GrapplePressed();

            player.input[0].jmp = grappleInput;
        }

        orig(self);

        player.input[0].jmp = wasJmpInput;
    }

    private static void Player_Update_GrappleWorm(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt<Room>(nameof(Room.PlaySound)),
            x => x.MatchPop(),
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<Player>("get_input")))
        {
            throw new Exception("Goto Failed");
        }

        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(1)))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);

        c.EmitDelegate<Func<bool, Player, bool>>((input, self) =>
        {
            if (!self.IsGrappleCustomInput())
            {
                return input;
            }

            return (self.JustPressed(Input_Helpers.Grapple) || Input_Helpers.MouseButtonJustPressed(ModOptions.MouseButtonGrapple.Value)) && self.canJump < 1;
        });
    }
}
