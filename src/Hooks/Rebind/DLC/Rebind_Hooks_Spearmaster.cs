using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace RebindEverything;

public static class Rebind_Hooks_Spearmaster
{
    public static void ApplyHooks()
    {
        try
        {
            IL.Player.GrabUpdate += PlayerOnGrabUpdate_Spearmaster;
        }
        catch (Exception e)
        {
            e.LogHookException();
        }
    }

    private static void PlayerOnGrabUpdate_Spearmaster(ILContext il)
    {
        var c = new ILCursor(il);

        var extractionDest = c.DefineLabel();
        var afterExtractionDest = c.DefineLabel();

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Action<Player>>(player =>
        {
            var module = player.GetModule();

            module.WasMakeSpearInputRegistered = false;
        });

        // Retraction
        if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Player>("get_input"),
                x => x.MatchLdcI4(0),
                x => x.MatchLdelema<Player.InputPackage>(),
                x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp))))
        {
            throw new Exception("Goto Failed");
        }

        // Ignore the original input check
        c.Emit(OpCodes.Pop);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(self => self.MakeSpearPressed());

        // Move closer to target
        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt<Player>(nameof(Player.PickupPressed))))
        {
            throw new Exception("Goto Failed");
        }

        // Get Destination
        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(3),
            x => x.MatchLdcI4(-1),
            x => x.MatchBle(out extractionDest)))
        {
            throw new Exception("Goto Failed");
        }


        // Extraction
        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<Player>("get_input"),
            x => x.MatchLdcI4(0),
            x => x.MatchLdelema<Player.InputPackage>(),
            x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.y)),
            x => x.MatchBrtrue(out afterExtractionDest)))
        {
            throw new Exception("Goto Failed");
        }

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(self =>
        {
            var module = self.GetModule();

            module.WasMakeSpearInputRegistered = true;

            return self.MakeSpearPressed();
        });

        c.Emit(OpCodes.Brfalse, afterExtractionDest);



        // Move just before PickupPressed checks
        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchStfld<Player>(nameof(Player.wantToThrow))))
        {
            throw new Exception("Goto Failed");
        }

        c.Index++;
        c.Emit(OpCodes.Pop);

        // Branch back to check extraction
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate<Func<Player, bool>>(player =>
        {
            var module = player.GetModule();

            var wasInputAlreadyProcessed = module.WasMakeSpearInputRegistered;

            module.WasMakeSpearInputRegistered = true;

            return wasInputAlreadyProcessed;
        });

        c.Emit(OpCodes.Brfalse, extractionDest);
        c.Emit(OpCodes.Ldloc_S, (byte)6);
    }
}
