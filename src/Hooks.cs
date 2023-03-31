using Expedition;
using ImprovedInput;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
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
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;

            On.Player.ctor += Player_ctor;

            On.Player.checkInput += Player_checkInput;

            On.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint; ;
            On.Player.TongueUpdate += Player_TongueUpdate;

            On.TubeWorm.GrabbedByPlayer += TubeWorm_GrabbedByPlayer;
        }


        private static bool isInit = false;

        private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            try
            {
                if (isInit) return;
                isInit = true;

                MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Options.instance);

                BackSpear.Description = "The key held to make Hunter either put or retrieve a spear from their back.";
                BackSlug.Description = "The key held to put or retrieve a Slugcat from your back.";

                Craft.Description = "The key held to make Artificer or Gourmand craft the items they are holding.";
                ArtiJump.Description = "The key pressed to make Artificer double jump, only works mid-air.";
                ArtiParry.Description = "The key pressed to make Artificer parry, forces a down input.";
                MakeSpear.Description = "The key held to have Spearmaster make a new spear.";
                Ascend.Description = "The key pressed to toggle Saint's ascension mode.";
                AimAscend.Description = "The key held to move the Saint's ascension reticle around.";

                Grapple.Description = "Affects Saint's Tongue & Grapple Worms.";


                IL.Player.GrabUpdate += Player_GrabUpdateIL;
                IL.Player.ClassMechanicsArtificer += Player_ClassMechanicsArtificerIL;

                IL.Player.SpearOnBack.Update += SpearOnBack_UpdateIL;
                IL.Player.SlugOnBack.Update += SlugOnBack_UpdateIL;

                IL.Player.Update += Player_Update;
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
            finally
            {
                orig(self);
            }
        }

        private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);

            BackSlug.HideConfig = !ModManager.MSC && !ModManager.JollyCoop;

            Craft.HideConfig = !ModManager.MSC;
            ArtiJump.HideConfig = !ModManager.MSC || MachineConnector.IsThisModActive("danizk0.rebindartificer");
            ArtiParry.HideConfig = !ModManager.MSC || MachineConnector.IsThisModActive("danizk0.rebindartificer");
            MakeSpear.HideConfig = !ModManager.MSC;
            Ascend.HideConfig = !ModManager.MSC;
            AimAscend.HideConfig = !ModManager.MSC;
        }



        private static ConditionalWeakTable<Player, PlayerModule> PlayerData = new ConditionalWeakTable<Player, PlayerModule>();

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            PlayerData.Add(self, new PlayerModule());
        }



        private static readonly PlayerKeybind BackSpear = PlayerKeybind.Register("rebindeverything:backspear", "Rebind Everything", "Back Spear", KeyCode.None, KeyCode.None);

        private static readonly PlayerKeybind BackSlug = PlayerKeybind.Register("rebindeverything:backslug", "Rebind Everything", "Back Slug", KeyCode.None, KeyCode.None);

        private static readonly PlayerKeybind Craft = PlayerKeybind.Register("rebindeverything:craft", "Rebind Everything", "Craft", KeyCode.None, KeyCode.None);

        private static readonly PlayerKeybind ArtiJump = PlayerKeybind.Register("rebindeverything:artijump", "Rebind Everything", "Arti Jump", KeyCode.None, KeyCode.None);
        private static readonly PlayerKeybind ArtiParry = PlayerKeybind.Register("rebindeverything:artiparry", "Rebind Everything", "Arti Parry", KeyCode.None, KeyCode.None);

        private static readonly PlayerKeybind MakeSpear = PlayerKeybind.Register("rebindeverything:makespear", "Rebind Everything", "Make Spear", KeyCode.None, KeyCode.None);

        private static readonly PlayerKeybind Ascend = PlayerKeybind.Register("rebindeverything:ascend", "Rebind Everything", "Ascend", KeyCode.None, KeyCode.None);
        private static readonly PlayerKeybind AimAscend = PlayerKeybind.Register("rebindeverything:aimascend", "Rebind Everything", "Aim Ascend", KeyCode.None, KeyCode.None);

        private static readonly PlayerKeybind Grapple = PlayerKeybind.Register("rebindeverything:grapple", "Rebind Everything", "Grapple", KeyCode.None, KeyCode.None);


        #region Input Checks

        private static bool ArtiJumpPressed(Player self)
        {
            bool isCustomInput = IsArtiJumpCustomInput(self);
            bool isParryOverride = IsArtiParryCustomInput(self) && self.input[0].y < 0;

            bool flag = self.wantToJump > 0 && self.input[0].pckp;
            bool flag2 = self.eatMeat >= 20 || self.maulTimer >= 15;    

            if (isCustomInput)
                return self.JustPressed(ArtiJump) && !self.pyroJumpped && self.canJump <= 0 && !flag2 && !isParryOverride;

            return flag && !self.pyroJumpped && self.canJump <= 0 && !flag2 && (self.input[0].y >= 0 || (self.input[0].y < 0 && (self.bodyMode == Player.BodyModeIndex.ZeroG || self.gravity <= 0.1f)));
        }

        private static bool IsArtiJumpCustomInput(Player self) => self.IsKeyBound(ArtiJump) && self.controller == null;



        private static bool ArtiParryPressed(Player self)
        {
            bool isCustomInput = IsArtiParryCustomInput(self);

            bool flag = self.wantToJump > 0 && self.input[0].pckp;
            bool flag2 = self.eatMeat >= 20 || self.maulTimer >= 15;

            if (isCustomInput)
                return self.JustPressed(ArtiParry) && !self.submerged && !flag2;

            return flag && !self.submerged && !flag2 && (self.input[0].y < 0 || self.bodyMode == Player.BodyModeIndex.Crawl) && (self.canJump > 0 || self.input[0].y < 0);
        }

        private static bool IsArtiParryCustomInput(Player self) => self.IsKeyBound(ArtiParry) && !ArtiParry.HideConfig && self.controller == null;



        private static bool BackSpearPressed(Player self)
        {
            if (self.PlayerGraspsHas(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) == 0 && self.PlayerGraspsHas(AbstractPhysicalObject.AbstractObjectType.Spear) >= 0) return false;
         
            bool isCustomInput = IsBackSpearCustomInput(self);

            if (isCustomInput)
                return self.IsPressed(BackSpear);

            return self.input[0].pckp;
        }

        private static bool IsBackSpearCustomInput(Player self) => self.IsKeyBound(BackSpear) && !BackSpear.HideConfig && self.controller == null;



        private static bool BackSlugPressed(Player self)
        {
            if (self.PlayerGraspsHas(AbstractPhysicalObject.AbstractObjectType.Spear) == 0 && self.PlayerGraspsHas(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) >= 0) return false;

            bool isCustomInput = IsBackSlugCustomInput(self);

            if (isCustomInput)
                return self.IsPressed(BackSlug);

            return self.input[0].pckp;
        }

        private static bool IsBackSlugCustomInput(Player self) => self.IsKeyBound(BackSlug) && !BackSlug.HideConfig && self.controller == null;



        private static bool CraftPressed(Player self)
        {
            bool isCustomInput = IsCraftCustomInput(self);

            if (isCustomInput)
                return self.IsPressed(Craft);

            return self.input[0].pckp;
        }

        private static bool IsCraftCustomInput(Player self) => self.IsKeyBound(Craft) && !Craft.HideConfig && self.controller == null;



        private static bool AscendPressed(Player self, bool isActivating)
        {
            bool isCustomInput = IsAscendCustomInput(self);

            if (isCustomInput)
                return self.IsPressed(Ascend);

            if (isActivating)
                return self.wantToJump > 0 && self.input[0].jmp;

            return self.wantToJump > 0;
        }

        private static bool IsAscendCustomInput(Player self) => self.IsKeyBound(Ascend) && !Ascend.HideConfig && self.controller == null;



        private static bool AimAscendPressed(Player self)
        {
            bool isCustomInput = IsAimAscendCustomInput(self);

            if (isCustomInput)
                return self.IsPressed(AimAscend);

            return self.input[0].thrw;
        }

        private static bool IsAimAscendCustomInput(Player self) => self.IsKeyBound(AimAscend) && !AimAscend.HideConfig && self.controller == null;



        private static bool GrapplePressed(Player self)
        {
            bool isCustomInput = IsGrappleCustomInput(self);

            if (isCustomInput)
                return self.IsPressed(Grapple);

            return self.input[0].jmp;
        }

        private static bool IsGrappleCustomInput(Player self) => self.IsKeyBound(Grapple) && !Grapple.HideConfig && self.controller == null;



        private static bool MakeSpearPressed(Player self)
        {
            bool isCustomInput = IsMakeSpearCustomInput(self);

            if (isCustomInput)
                return self.IsPressed(MakeSpear);

            return self.input[0].pckp;
        }

        private static bool IsMakeSpearCustomInput(Player self) => self.IsKeyBound(MakeSpear) && !MakeSpear.HideConfig && self.controller == null;

        #endregion


        private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self);

            if (self.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                // We can replicate the normally required inputs to make gameplay with the rebinds more legitimate
                if (IsArtiJumpCustomInput(self) && ArtiJumpPressed(self))
                    self.input[0].jmp = true;

                else if (IsArtiParryCustomInput(self) && ArtiParryPressed(self))
                    self.input[0].y = -1;



                if (IsArtiParryCustomInput(self) && ArtiParryPressed(self))
                    self.input[0].jmp = true;
            }
        }



        // Arti Jump & Parry
        // I realised later that IL Hooking wasn't necessary, oh well...
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

            c.EmitDelegate(ArtiJumpPressed);

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

            c.EmitDelegate(ArtiParryPressed);

            c.Emit(OpCodes.Brtrue, afterParryInput);
            c.Emit(OpCodes.Br, afterParry);

            c.Emit(OpCodes.Ldloc, 0);

            #endregion
        }



        // Spear Extraction, Back Spear, Back Slug, Craft
        private static void Player_GrabUpdateIL(ILContext il)
        {
            BackSpearSlugIL(il);
            MakeSpearIL(il);
            CraftIL(il);
        }



        // Back Spears & Slugs
        // Warning: Moderate pain
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

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(IsBackSpearCustomInput);
                c.Emit(OpCodes.Brtrue, afterIncrementFalseSpear);

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

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(IsBackSlugCustomInput);
                c.Emit(OpCodes.Brtrue, afterIncrementFalseSlug);
                
                c.Emit(OpCodes.Ldarg_0);
                
                c.Index++; 
            }

            #endregion

            c.Index = 0;

            #region Custom Input Checks

            c.GotoNext(MoveType.Before,
                x => x.MatchLdcI4(-1),
                x => x.MatchStloc(7));


            // Slug To Back
            c.Emit(OpCodes.Ldloc, 7);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Action<int, Player>>((grasps, self) =>
            {
                if (self.slugOnBack == null) return;
                

                if (!IsBackSlugCustomInput(self)) return;

                if (!PlayerData.TryGetValue(self, out var playerModule)) return;

                
                if (self.slugOnBack.HasASlug && !playerModule.hadASlug && BackSlugPressed(self))
                    playerModule.canTakeSlugOffBack = false;

                playerModule.hadASlug = self.slugOnBack.HasASlug;

                if (!BackSlugPressed(self))
                    playerModule.canTakeSlugOffBack = true;

                if (!playerModule.canTakeSlugOffBack && self.slugOnBack.HasASlug) return;


                bool hasFreeHand = grasps > -1;
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

                if (self.spearOnBack != null && self.spearOnBack.HasASpear) return;

                if (hasFreeHand || self.CanRetrieveSlugFromBack || holdingSlug)
                    self.slugOnBack.increment = BackSlugPressed(self);
            });



            // Spear To Back
            c.Emit(OpCodes.Ldloc, 5);
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Action<int, Player>>((grasps, self) =>
            {
                if (self.spearOnBack == null) return;


                if (!IsBackSpearCustomInput(self)) return;

                bool hasFreeHand = grasps > -1;
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

                if (hasFreeHand || self.CanRetrieveSpearFromBack || holdingSpear)
                    self.spearOnBack.increment = BackSpearPressed(self);
            });

            #endregion


            #region Disable First Increment True Checks

            ILLabel afterSlugToBack = null!;

            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Player>("get_CanRetrieveSlugFromBack"),
                x => x.MatchBrfalse(out afterSlugToBack));

            c.GotoPrev(MoveType.Before,
                x => x.MatchLdloc(7),
                x => x.MatchLdcI4(-1),
                x => x.MatchBgt(out _));

            c.Index++;
            c.Emit(OpCodes.Pop);

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(IsBackSlugCustomInput);
            c.Emit(OpCodes.Brtrue, afterSlugToBack);
            
            c.Emit(OpCodes.Ldloc, 7);


            ILLabel afterSpearToBack = null!;

            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Player>("get_CanRetrieveSpearFromBack"),
                x => x.MatchBrfalse(out afterSpearToBack));

            c.GotoPrev(MoveType.Before,
                x => x.MatchLdloc(5),
                x => x.MatchLdcI4(-1),
                x => x.MatchBgt(out _));

            c.Index++;
            c.Emit(OpCodes.Pop);

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(IsBackSpearCustomInput);
            c.Emit(OpCodes.Brtrue, afterSpearToBack);

            c.Emit(OpCodes.Ldloc, 5);

            #endregion


            #region Disable Second Increment True Checks

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

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((player) => !IsBackSlugCustomInput(player));
            c.Emit(OpCodes.And);


            // Back Spear
            c.GotoNext(MoveType.After,
                x => x.MatchLdfld<Player.InputPackage>(nameof(Player.InputPackage.pckp)));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((player) => !IsBackSpearCustomInput(player));
            c.Emit(OpCodes.And);

            #endregion
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



        // TODO: Cleanup (nah lol)
        // Make Spear
        private static void MakeSpearIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILLabel extractionDest = null!;
            ILLabel afterExtractionDest = null!;

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<Player>>((player) =>
            {
                PlayerData.TryGetValue(player, out var playerEx);
                if (playerEx == null) return;

                playerEx.wasMakeSpearInputRegistered = false;
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
            c.EmitDelegate<Func<Player, bool>>((self) =>
            {
                PlayerData.TryGetValue(self, out var playerEx);
                if (playerEx == null) return false;

                return MakeSpearPressed(self);
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
            c.EmitDelegate<Func<Player, bool>>((self) =>
            {
                PlayerData.TryGetValue(self, out var playerEx);
                if (playerEx == null) return true;

                playerEx.wasMakeSpearInputRegistered = true;

                return MakeSpearPressed(self);
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

                bool wasInputAlreadyProcessed = playerEx.wasMakeSpearInputRegistered;
                playerEx.wasMakeSpearInputRegistered = true;

                return wasInputAlreadyProcessed;
            });

            c.Emit(OpCodes.Brfalse, extractionDest);
            c.Emit(OpCodes.Ldloc_S, (byte)6);
        }


        // Craft
        private static void CraftIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.Before,
                x => x.MatchLdcI4(-1),
                x => x.MatchStloc(7));


            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<Player>>((self) =>
            {
                if (!PlayerData.TryGetValue(self, out var playerModule)) return;

                playerModule.isCrafting = false;


                if (!CraftPressed(self)) return;

                if (ModManager.MSC && (self.FreeHand() == -1 || self.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer) && CustomGraspsCanBeCrafted(self))
                {
                    self.craftingObject = true;
                    playerModule.isCrafting = true;
                    return;
                }
            });


            c.Emit(OpCodes.Ldloc_1);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, Player, bool>>((flag3, self) =>
            {
                if (!IsCraftCustomInput(self)) return flag3;

                if (!PlayerData.TryGetValue(self, out var playerModule)) return flag3;

                if (playerModule.isCrafting) return true;

                return flag3;
            });
            c.Emit(OpCodes.Stloc_1);


            c.Emit(OpCodes.Ldloc, 6);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<int, Player, int>>((num5, self) =>
            {
                if (!IsCraftCustomInput(self)) return num5;

                if (!PlayerData.TryGetValue(self, out var playerModule)) return num5;

                if (playerModule.isCrafting) return -1;

                return num5;
            });
            c.Emit(OpCodes.Stloc, 6);


            c.GotoNext(MoveType.After,
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Player>(nameof(Player.GraspsCanBeCrafted)));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<Player, bool>>((self) => !IsCraftCustomInput(self));
            c.Emit(OpCodes.And);
        }

        private static bool CustomGraspsCanBeCrafted(Player self)
        {
            if ((!(self.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer) || !(self.CraftingResults() != null)) && (!(self.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Gourmand) || !(self.CraftingResults() != null)))
            {
                if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-crafting"))
                    return self.CraftingResults() != null;
                
                return false;
            }

            return true;
        }



        // Ascend, Aim Ascend & Grapple
        private static void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player self)
        {
            if (!PlayerData.TryGetValue(self, out var playerModule))
            {
                orig(self);
                return;
            }


            int wasWantToJump = self.wantToJump;
            bool wasPckpInput = self.input[0].pckp;
            bool wasThrwInput = self.input[0].thrw;
            bool wasJmpInput = self.input[0].jmp;

            bool wasAscension = self.monkAscension;

            if (IsAscendCustomInput(self))
            {
                bool ascensionInput = self.JustPressed(Ascend);

                self.wantToJump = ascensionInput ? 1 : 0;
                self.input[0].pckp = ascensionInput && !self.monkAscension;
            }

            if (IsAimAscendCustomInput(self))
            {
                bool moveAscensionInput = AimAscendPressed(self);

                if (self.monkAscension)
                    self.input[0].thrw = moveAscensionInput;
            }

            if (IsGrappleCustomInput(self))
                self.input[0].jmp = self.JustPressed(Grapple);



            orig(self);


            if (wasAscension == self.monkAscension)
                self.wantToJump = wasWantToJump;

            self.input[0].pckp = wasPckpInput;
            self.input[0].thrw = wasThrwInput;
            self.input[0].jmp = wasJmpInput;
        }



        // Grapple
        private static void Player_TongueUpdate(On.Player.orig_TongueUpdate orig, Player self)
        {
            if (!PlayerData.TryGetValue(self, out var playerModule))
            {
                orig(self);
                return;
            }


            bool wasJmpInput = self.input[0].jmp;
            bool wasJmpInputLastFrame = self.input[1].jmp;



            if (IsGrappleCustomInput(self))
            {
                bool grappleInput = self.JustPressed(Grapple);

                self.input[0].jmp = grappleInput;
                self.input[1].jmp = false;
            }



            orig(self);



            self.input[0].jmp = wasJmpInput;
            self.input[1].jmp = wasJmpInputLastFrame;
        }

        private static void TubeWorm_GrabbedByPlayer(On.TubeWorm.orig_GrabbedByPlayer orig, TubeWorm self)
        {
            Player? player = null;
            for (int i = 0; i < self.grabbedBy.Count; i++)
            {
                if (self.grabbedBy[i].grabber is Player)
                {
                    player = self.grabbedBy[i].grabber as Player;
                    break;
                }
            }

            if (player == null)
            {
                orig(self);
                return;
            }



            bool wasJmpInput = player.input[0].jmp;



            if (IsGrappleCustomInput(player))
            {
                bool grappleInput = GrapplePressed(player);

                player.input[0].jmp = grappleInput;
            }



            orig(self);

            player.input[0].jmp = wasJmpInput;
        }


        // grapple worms i hate you
        private static void Player_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<Room>(nameof(Room.PlaySound)),
                x => x.MatchPop(),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<Player>("get_input"));

            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(1));


            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<bool, Player, bool>>((input, self) =>
            {
                if (!IsGrappleCustomInput(self))
                    return input;

                return self.JustPressed(Grapple) && self.canJump < 1;
            });
        }



        private static int PlayerGraspsHas(this Player self, AbstractPhysicalObject.AbstractObjectType type)
        {
            for (int i = 0; i < self.grasps.Length; i++)
            {
                Creature.Grasp? grasp = self.grasps[i];
                if (grasp == null) continue;

                if (grasp.grabbed.abstractPhysicalObject.type == type)
                    return i;
            }

            return -1;
        }

        private static int PlayerGraspsHas(this Player self, CreatureTemplate.Type type)
        {
            for (int i = 0; i < self.grasps.Length; i++)
            {
                Creature.Grasp? grasp = self.grasps[i];
                if (grasp == null) continue;

                if (grasp.grabbed is not Creature creature) continue;

                if (creature.Template.type == type)
                    return i;
            }

            return -1;
        }
    }
}
