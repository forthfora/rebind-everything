using Expedition;
using ImprovedInput;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using UnityEngine;

namespace RebindEverything;

internal static partial class Hooks
{
    private static readonly PlayerKeybind BackSpear = PlayerKeybind.Register("rebindeverything:backspear", "Rebind Everything", "Back Spear", KeyCode.None, KeyCode.None);

    private static readonly PlayerKeybind BackSlug = PlayerKeybind.Register("rebindeverything:backslug", "Rebind Everything", "Back Slug", KeyCode.None, KeyCode.None);

    private static readonly PlayerKeybind Craft = PlayerKeybind.Register("rebindeverything:craft", "Rebind Everything", "Craft", KeyCode.None, KeyCode.None);

    private static readonly PlayerKeybind ArtiJump = PlayerKeybind.Register("rebindeverything:artijump", "Rebind Everything", "Arti Jump", KeyCode.None, KeyCode.None);
    private static readonly PlayerKeybind ArtiParry = PlayerKeybind.Register("rebindeverything:artiparry", "Rebind Everything", "Arti Parry", KeyCode.None, KeyCode.None);

    private static readonly PlayerKeybind MakeSpear = PlayerKeybind.Register("rebindeverything:makespear", "Rebind Everything", "Make Spear", KeyCode.None, KeyCode.None);

    private static readonly PlayerKeybind Ascend = PlayerKeybind.Register("rebindeverything:ascend", "Rebind Everything", "Ascend", KeyCode.None, KeyCode.None);
    private static readonly PlayerKeybind AimAscend = PlayerKeybind.Register("rebindeverything:aimascend", "Rebind Everything", "Aim Ascend", KeyCode.None, KeyCode.None);

    private static readonly PlayerKeybind Grapple = PlayerKeybind.Register("rebindeverything:grapple", "Rebind Everything", "Grapple", KeyCode.None, KeyCode.None);


    private static bool ArtiJumpPressed(Player self)
    {
        bool isCustomInput = IsArtiJumpCustomInput(self);
        bool isParryOverride = ArtiJump.CurrentBinding(self.playerState.playerNumber) == ArtiParry.CurrentBinding(self.playerState.playerNumber) && self.input[0].y < 0 && self.bodyMode != Player.BodyModeIndex.ZeroG;

        bool flag2 = self.eatMeat >= 20 || self.maulTimer >= 15;

        if (isCustomInput)
            return self.JustPressed(ArtiJump) && !self.pyroJumpped && self.canJump <= 0 && !flag2 && !isParryOverride;


        bool flag = self.wantToJump > 0 && self.input[0].pckp;

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
}
