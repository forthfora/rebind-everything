using ImprovedInput;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using static MonoMod.InlineRT.MonoModRule;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using Random = UnityEngine.Random;

namespace RebindEverything
{
    internal static class Hooks
    {
        private static bool isInit = false;

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);

            if (isInit) return;
            isInit = true;

            try
            {
                IL.Player.GrabUpdate += Player_GrabUpdateIL;
                IL.Player.ClassMechanicsArtificer += Player_ClassMechanicsArtificerIL;

                if (ModManager.MSC)
                {
                    Craft = PlayerKeybind.Register("rebindeverything:craft", "Rebind Everything", "Craft", KeyCode.None, KeyCode.None);

                    ArtiJump = PlayerKeybind.Register("rebindeverything:artijump", "Rebind Everything", "Arti Jump", KeyCode.None, KeyCode.None);
                    ArtiParry = PlayerKeybind.Register("rebindeverything:artiparry", "Rebind Everything", "Arti Parry", KeyCode.None, KeyCode.None);

                    ExtractSpear = PlayerKeybind.Register("rebindeverything:extractspear", "Rebind Everything", "Extract Spear", KeyCode.None, KeyCode.None);
                    Ascension = PlayerKeybind.Register("rebindeverything:ascension", "Rebind Everything", "Ascension", KeyCode.None, KeyCode.None);
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }


        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.Player.ctor += Player_ctor;

            On.Player.checkInput += Player_checkInput;
        }


        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            PlayerData.Add(self, new PlayerEx());
        }

        private static ConditionalWeakTable<Player, PlayerEx> PlayerData = new ConditionalWeakTable<Player, PlayerEx>();

        private static readonly PlayerKeybind BackSpear = PlayerKeybind.Register("rebindeverything:backspear", "Rebind Everything", "Back Spear", KeyCode.None, KeyCode.None);
        private static PlayerKeybind Craft = null!;

        private static PlayerKeybind ArtiJump = null!;
        private static PlayerKeybind ArtiParry = null!;

        private static PlayerKeybind ExtractSpear = null!;
        private static PlayerKeybind Ascension = null!;


        private static bool CanArtiJump(Player player)
        {
            bool isCustomInput = ArtiJump != null && ArtiJump.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;
            
            bool flag = player.wantToJump > 0 && player.input[0].pckp;
            bool flag2 = player.eatMeat >= 20 || player.maulTimer >= 15;    

            if (isCustomInput && player.controller == null)
                return player.JustPressed(ArtiJump) && !player.pyroJumpped && player.canJump <= 0 && !flag2;

            return flag && !player.pyroJumpped && player.canJump <= 0 && !flag2 && (player.input[0].y >= 0 || (player.input[0].y < 0 && (player.bodyMode == Player.BodyModeIndex.ZeroG || player.gravity <= 0.1f)));
        }

        private static bool CanArtiParry(Player player)
        {
            bool isCustomInput = ArtiParry != null && ArtiParry.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            bool flag = player.wantToJump > 0 && player.input[0].pckp;
            bool flag2 = player.eatMeat >= 20 || player.maulTimer >= 15;

            if (isCustomInput && player.controller == null)
            {
                return player.JustPressed(ArtiParry) && !player.submerged && !flag2 && (player.bodyMode == Player.BodyModeIndex.Crawl || player.input[0].y < 0 || player.canJump <= 0);
            }

            return flag && !player.submerged && !flag2 && (player.input[0].y < 0 || player.bodyMode == Player.BodyModeIndex.Crawl) && (player.canJump > 0 || player.input[0].y < 0);
        }

        private static bool CanBackSpear(Player player)
        {
            bool isCustomInput = BackSpear != null && BackSpear.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            if (isCustomInput && player.controller == null)
                return player.IsPressed(BackSpear);

            return player.input[0].pckp;
        }

        private static bool CanCraft(Player player)
        {
            bool isCustomInput = Craft != null && Craft.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            if (isCustomInput && player.controller == null)
                return player.IsPressed(Craft);

            return player.input[0].pckp;
        }

        private static bool CanAscension(Player player, bool isActivating)
        {
            bool isCustomInput = Ascension != null && Ascension.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            if (isCustomInput && player.controller == null)
                return player.IsPressed(Ascension);

            if (isActivating)
                return player.wantToJump > 0 && player.input[0].jmp;

            return player.wantToJump > 0;
        }


        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);

            // We can replicate the normally required inputs to make gameplay with the rebinds more legitimate

            if (CanArtiJump(self))
                self.input[0].jmp = true;
            
            if (CanArtiParry(self))
                self.input[0].jmp = true;
        }

        // Arti Jump & Parry
        private static void Player_ClassMechanicsArtificerIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            #region Arti Jump
            ILLabel afterJumpInput = null!;
            ILLabel afterJump = null!;

            // Get after jump input checks
            c.GotoNext(MoveType.Before,
                x => x.MatchLdfld<Player>("bodyMode"),
                x => x.MatchLdsfld<Player.BodyModeIndex>("ZeroG"),
                x => x.Match(OpCodes.Call),
                x => x.MatchBrtrue(out afterJumpInput));

            // Get after jump block
            c.GotoNext(MoveType.Before,
                x => x.MatchLdcR4(0.1f),
                x => x.MatchBgtUn(out afterJump));

            c.GotoPrev(MoveType.Before,
                x => x.MatchLdloc(0),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Player>("pyroJumpped"));


            c.Index++;
            c.Emit(OpCodes.Pop);
            Plugin.Logger.LogWarning(c.Index);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<Player, bool>>((player) => CanArtiJump(player));

            // Custom check branch
            c.Emit(OpCodes.Brtrue, afterJumpInput);

            // Branch after if check returns false
            c.Emit(OpCodes.Br, afterJump);
            c.Emit(OpCodes.Ldloc, 0);
            #endregion

            #region Arti Parry
            ILLabel afterParryInput = null!;
            ILLabel afterParry = null!;

            c.GotoNext(MoveType.Before,
                x => x.Match(OpCodes.Call),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Player>("canJump"),
                x => x.MatchLdcI4(0),
                x => x.MatchBgt(out afterParryInput));

            Plugin.Logger.LogWarning(c.Index);

            c.GotoPrev(MoveType.Before,
                x => x.MatchLdloc(0),
                x => x.MatchBrfalse(out afterParry),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Player>("submerged"));

            c.Index++;
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<Player, bool>>((player) => CanArtiParry(player));

            c.Emit(OpCodes.Brtrue, afterParryInput);
            c.Emit(OpCodes.Br, afterParry);

            c.Emit(OpCodes.Ldloc, 0);
            #endregion
        }


        private static void Player_GrabUpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // TODO: Rewrite to better standard
            #region Extract Spear

            ILLabel extractionDest = null!;
            ILLabel afterExtractionDest = null!;

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<Player>>((player) =>
            {
                PlayerData.TryGetValue(player, out var playerEx);
                if (playerEx == null) return;

                playerEx.wasExtractSpearInputRegistered = false;
            });



            // Retraction
            c.GotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Player>("get_input"),
                x => x.MatchLdcI4(0),
                x => x.MatchLdelema<Player.InputPackage>(),
                x => x.MatchLdfld<Player.InputPackage>("pckp"));

            c.RemoveRange(5);

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((player) =>
            {
                PlayerData.TryGetValue(player, out var playerEx);
                if (playerEx == null) return false;

                if (ExtractSpear == null || ExtractSpear.CurrentBinding(player.playerState.playerNumber) == KeyCode.None || player.controller != null) return player.input[0].pckp;

                return player.IsPressed(ExtractSpear);
            });



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
                PlayerData.TryGetValue(player, out var playerEx);
                if (playerEx == null) return true;

                playerEx.wasExtractSpearInputRegistered = true;

                if (ExtractSpear == null || ExtractSpear.CurrentBinding(player.playerState.playerNumber) == KeyCode.None || player.controller != null) return player.input[0].pckp;

                return player.IsPressed(ExtractSpear);
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
                PlayerData.TryGetValue(player, out var playerEx);
                if (playerEx == null) return false;

                bool wasInputAlreadyProcessed = playerEx.wasExtractSpearInputRegistered;
                playerEx.wasExtractSpearInputRegistered = true;

                return wasInputAlreadyProcessed;
            });

            c.Emit(OpCodes.Brfalse, extractionDest);
            c.Emit(OpCodes.Ldloc_S, (byte)6);
            #endregion
        }
    }
}
