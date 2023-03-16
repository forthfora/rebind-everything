using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RebindSpearmaster
{
    internal static class Hooks
    {
        public static void ApplyHooks() => On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        private static Dictionary<Player, bool> wasInputProcessed = new Dictionary<Player, bool>();

        private static bool isInit = false;

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            if (isInit) return;
            isInit = true;

            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Options.instance);

            try
            {
                IL.Player.GrabUpdate += Player_GrabUpdateIL;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }

        private static void Player_GrabUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel extractionDest = null!;
            ILLabel afterExtractionDest = null!;

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<Player>>((player) => wasInputProcessed[player] = false);



            // Retraction
            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Player>("get_input"),
                x => x.MatchLdcI4(0),
                x => x.MatchLdelema<Player.InputPackage>(),
                x => x.MatchLdfld<Player.InputPackage>("pckp"));

            c.RemoveRange(5);

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((player) => IsCustomKeybindPressed(player));



            // Move closer to target
            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Player>("PickupPressed"));


            // Get Destination
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(3),
                x => x.MatchLdcI4(-1),
                x => x.MatchBle(out extractionDest));


            // Extraction
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Player>("get_input"),
                x => x.MatchLdcI4(0),
                x => x.MatchLdelema<Player.InputPackage>(),
                x => x.MatchLdfld<Player.InputPackage>("y"),
                x => x.MatchBrtrue(out afterExtractionDest));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((player) =>
            {
                wasInputProcessed[player] = true;
                return IsCustomKeybindPressed(player);
            });

            c.Emit(OpCodes.Brfalse, afterExtractionDest);



            // Move just before PickupPressed checks
            c.GotoNext(MoveType.After,
                x => x.MatchStfld<Player>("wantToThrow"));

            c.Index++;
            c.Emit(OpCodes.Pop);

            // Branch back to check extraction
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((player) =>
            {
                // lol
                bool wasInputAlreadyProcessed = wasInputProcessed[player];
                wasInputProcessed[player] = true;
                return wasInputAlreadyProcessed;
            });

            c.Emit(OpCodes.Brfalse, extractionDest);
            c.Emit(OpCodes.Ldloc_S, (byte)6);
        }
    }
}
