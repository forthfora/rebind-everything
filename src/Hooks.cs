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
        public static void ApplyHooks()
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.Player.ctor += Player_ctor;

            On.Player.checkInput += Player_checkInput;
        }


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

                IL.Player.SpearOnBack.Update += SpearOnBack_UpdateIL;
                IL.Player.SlugOnBack.Update += SlugOnBack_UpdateIL;


                if (ModManager.MSC || ModManager.JollyCoop)
                    BackSlug = PlayerKeybind.Register("rebindeverything:backslug", "Rebind Everything", "Back Slug", KeyCode.None, KeyCode.None);

                if (ModManager.MSC)
                {

                    Craft = PlayerKeybind.Register("rebindeverything:craft", "Rebind Everything", "Craft", KeyCode.None, KeyCode.None);

                    ArtiJump = PlayerKeybind.Register("rebindeverything:artijump", "Rebind Everything", "Arti Jump", KeyCode.None, KeyCode.None);
                    ArtiParry = PlayerKeybind.Register("rebindeverything:artiparry", "Rebind Everything", "Arti Parry", KeyCode.None, KeyCode.None);

                    ExtractSpear = PlayerKeybind.Register("rebindeverything:extractspear", "Rebind Everything", "Extract Spear", KeyCode.None, KeyCode.None);

                    Ascension = PlayerKeybind.Register("rebindeverything:ascension", "Rebind Everything", "Ascension", KeyCode.None, KeyCode.None);
                }

                Grapple = PlayerKeybind.Register("rebindeverything:grapple", "Rebind Everything", "Grapple", KeyCode.None, KeyCode.None);
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }


        private static ConditionalWeakTable<Player, PlayerEx> PlayerData = new ConditionalWeakTable<Player, PlayerEx>();

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            PlayerData.Add(self, new PlayerEx());
        }



        private static readonly PlayerKeybind BackSpear = PlayerKeybind.Register("rebindeverything:backspear", "Rebind Everything", "Back Spear", KeyCode.None, KeyCode.None);
        private static PlayerKeybind? Grapple;

        private static PlayerKeybind? BackSlug;

        private static PlayerKeybind? Craft;

        private static PlayerKeybind? ArtiJump;
        private static PlayerKeybind? ArtiParry;

        private static PlayerKeybind? ExtractSpear;
        
        private static PlayerKeybind? Ascension;


        #region Input Checks

        private static bool ArtiJumpPressed(Player player)
        {
            bool isCustomInput = ArtiJump != null && ArtiJump.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;
            
            bool flag = player.wantToJump > 0 && player.input[0].pckp;
            bool flag2 = player.eatMeat >= 20 || player.maulTimer >= 15;    

            if (isCustomInput && player.controller == null)
                return player.JustPressed(ArtiJump) && !player.pyroJumpped && player.canJump <= 0 && !flag2;

            return flag && !player.pyroJumpped && player.canJump <= 0 && !flag2 && (player.input[0].y >= 0 || (player.input[0].y < 0 && (player.bodyMode == Player.BodyModeIndex.ZeroG || player.gravity <= 0.1f)));
        }

        private static bool ArtiParryPressed(Player player)
        {
            bool isCustomInput = ArtiParry != null && ArtiParry.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            bool flag = player.wantToJump > 0 && player.input[0].pckp;
            bool flag2 = player.eatMeat >= 20 || player.maulTimer >= 15;

            if (isCustomInput && player.controller == null)
                return player.JustPressed(ArtiParry) && !player.submerged && !flag2 && (player.bodyMode == Player.BodyModeIndex.Crawl || player.input[0].y < 0 || player.canJump <= 0);

            return flag && !player.submerged && !flag2 && (player.input[0].y < 0 || player.bodyMode == Player.BodyModeIndex.Crawl) && (player.canJump > 0 || player.input[0].y < 0);
        }

        private static bool BackSpearPressed(Player player)
        {
            bool isCustomInput = BackSpear != null && BackSpear.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            if (isCustomInput && player.controller == null)
                return player.IsPressed(BackSpear);

            return player.input[0].pckp;
        }

        private static bool BackSlugPressed(Player player)
        {
            bool isCustomInput = BackSlug != null && BackSlug.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            if (isCustomInput && player.controller == null)
                return player.IsPressed(BackSlug);

            return player.input[0].pckp;
        }

        private static bool CraftPressed(Player player)
        {
            bool isCustomInput = Craft != null && Craft.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            if (isCustomInput && player.controller == null)
                return player.IsPressed(Craft);

            return player.input[0].pckp;
        }

        private static bool AscensionPressed(Player player, bool isActivating)
        {
            bool isCustomInput = Ascension != null && Ascension.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            if (isCustomInput && player.controller == null)
                return player.IsPressed(Ascension);

            if (isActivating)
                return player.wantToJump > 0 && player.input[0].jmp;

            return player.wantToJump > 0;
        }

        private static bool GrapplePressed(Player player)
        {
            bool isCustomInput = Grapple != null && Grapple.CurrentBinding(player.playerState.playerNumber) != KeyCode.None;

            if (isCustomInput && player.controller == null)
                return player.IsPressed(Grapple);

            return player.input[0].jmp;
        }

        #endregion


        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);

            // We can replicate the normally required inputs to make gameplay with the rebinds more legitimate

            if (ArtiJumpPressed(self))
                self.input[0].jmp = true;
            
            if (ArtiParryPressed(self))
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
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<Player, bool>>((player) => ArtiJumpPressed(player));

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

            c.GotoPrev(MoveType.Before,
                x => x.MatchLdloc(0),
                x => x.MatchBrfalse(out afterParry),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Player>("submerged"));

            c.Index++;
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<Player, bool>>((player) => ArtiParryPressed(player));

            c.Emit(OpCodes.Brtrue, afterParryInput);
            c.Emit(OpCodes.Br, afterParry);

            c.Emit(OpCodes.Ldloc, 0);
            #endregion
        }



        // Spear Extraction, Back Spears, Slugpups
        private static void Player_GrabUpdateIL(ILContext il)
        {
            BackSpearSlugIL(il);
            //ExtractSpearIL(il);
        }


        private static void BackSpearSlugIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);


            #region Disable Setting Increment To False

            ILLabel afterIncrementFalseSpear = null!;

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
                c.Emit(OpCodes.Br, afterIncrementFalseSpear);
                c.Emit(OpCodes.Ldarg_0);
                
                c.Index++;
            }

            c.Index = 0;

            ILLabel afterIncrementFalseSlug = null!;

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
                c.Emit(OpCodes.Br, afterIncrementFalseSlug);
                c.Emit(OpCodes.Ldarg_0);
                
                c.Index++; 
            }

            #endregion

            c.Index = 0;

            #region Custom Checks

            c.GotoNext(MoveType.Before,
                x => x.MatchLdcI4(-1),
                x => x.MatchStloc(7));


            // Slug To Back
            c.Emit(OpCodes.Ldloc, 7);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Action<int, Player>>((grasps, self) =>
            {
                bool freeHand = grasps > -1;
                
                bool holdingSlug = false;

                if (self.CanPutSlugToBack)
                {
                    for (int n = 0; n < 2; n++)
                    {
                        if (self.grasps[n] != null && self.grasps[n].grabbed is Player && !((Player)self.grasps[n].grabbed).dead)
                        {
                            holdingSlug = true;
                            break;
                        }
                    }
                }

                if (self.spearOnBack.HasASpear) return;

                if (freeHand || self.CanRetrieveSlugFromBack || holdingSlug)
                    self.slugOnBack.increment = BackSlugPressed(self);
            });



            // Spear To Back
            c.Emit(OpCodes.Ldloc, 5);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Action<int, Player>>((grasps, self) =>
            {
                bool freeHand = grasps > -1;

                bool holdingSpear = false;

                if (self.CanPutSpearToBack)
                {
                    for (int m = 0; m < 2; m++)
                    {
                        if (self.grasps[m] != null && self.grasps[m].grabbed is Spear)
                        {
                            holdingSpear = true;
                            break;
                        }
                    }
                }

                if (freeHand || self.CanRetrieveSpearFromBack || holdingSpear)
                    self.spearOnBack.increment = BackSpearPressed(self);
            });

            #endregion


            #region Disable First Increment True Checks

            ILLabel afterSpearToBack = null!;

            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Player>("get_CanRetrieveSpearFromBack"),
                x => x.MatchBrfalse(out afterSpearToBack));
            
            c.GotoPrev(MoveType.Before,
                x => x.MatchLdloc(7),
                x => x.MatchLdcI4(-1),
                x => x.MatchBgt(out _));

            c.Index++;
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Br, afterSpearToBack);
            c.Emit(OpCodes.Ldloc, 7);

            #endregion


            #region Disable Secondary Increment True Checks

            // Move Closer to target
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Creature>("get_grasps"),
                x => x.MatchLdloc(28));

            c.GotoNext(MoveType.After,
                x => x.MatchBgt(out _),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Player>("get_CanRetrieveSlugFromBack"));



            // Back Slugpup
            c.GotoPrev(MoveType.After,
                x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp)));

            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);


            // Back Spear
            c.GotoNext(MoveType.After,
                x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp)));
            
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);

            #endregion

            //Plugin.Logger.LogWarning(c.Context);
        }

        private static void SlugOnBack_UpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp)));

            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player.SlugOnBack, bool>>((self) => BackSlugPressed(self.owner));
        }

        private static void SpearOnBack_UpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp)));

            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player.SpearOnBack, bool>>((self) => BackSpearPressed(self.owner));
        }



        // TODO: Cleanup
        private static void ExtractSpearIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

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
        }
    }
}
