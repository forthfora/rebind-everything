﻿using ImprovedInput;
using UnityEngine;

namespace RebindEverything;

internal static class Input_Helpers
{
    public static PlayerKeybind BackSpear { get; } = PlayerKeybind.Register("rebindeverything:backspear", "Rebind Everything", "Back Spear", KeyCode.None, KeyCode.None);
    public static PlayerKeybind BackSlug { get; } = PlayerKeybind.Register("rebindeverything:backslug", "Rebind Everything", "Back Slug", KeyCode.None, KeyCode.None);

    public static PlayerKeybind Craft { get; } = PlayerKeybind.Register("rebindeverything:craft", "Rebind Everything", "Craft", KeyCode.None, KeyCode.None);

    public static PlayerKeybind ArtiJump { get; } = PlayerKeybind.Register("rebindeverything:artijump", "Rebind Everything", "Arti Jump", KeyCode.None, KeyCode.None);
    public static PlayerKeybind ArtiParry { get; } = PlayerKeybind.Register("rebindeverything:artiparry", "Rebind Everything", "Arti Parry", KeyCode.None, KeyCode.None);

    public static PlayerKeybind MakeSpear { get; } = PlayerKeybind.Register("rebindeverything:makespear", "Rebind Everything", "Make Spear", KeyCode.None, KeyCode.None);

    public static PlayerKeybind Ascend { get; } = PlayerKeybind.Register("rebindeverything:ascend", "Rebind Everything", "Ascend", KeyCode.None, KeyCode.None);
    public static PlayerKeybind AimAscend { get; } = PlayerKeybind.Register("rebindeverything:aimascend", "Rebind Everything", "Aim Ascend", KeyCode.None, KeyCode.None);

    public static PlayerKeybind Grapple { get; } = PlayerKeybind.Register("rebindeverything:grapple", "Rebind Everything", "Grapple", KeyCode.None, KeyCode.None);


    public static void InitInput()
    {
        BackSpear.Description = "The key held to make Hunter either put or retrieve a spear from their back.";
        BackSlug.Description = "The key held to put or retrieve a Slugcat from your back.";

        Craft.Description = "The key held to make Artificer or Gourmand craft the items they are holding.";

        ArtiJump.Description = "The key pressed to make Artificer double jump, only works mid-air.";
        ArtiParry.Description = "The key pressed to make Artificer parry, forces a down input.";

        MakeSpear.Description = "The key held to have Spearmaster make a new spear.";

        Ascend.Description = "The key pressed to toggle Saint's ascension mode.";
        AimAscend.Description = "The key held to move the Saint's ascension reticle around.";

        Grapple.Description = "Affects Saint's Tongue & Grapple Worms.";


        BackSpear.HideConflict = k => k == BackSlug;
        BackSlug.HideConflict = k => k == BackSpear;

        ArtiJump.HideConflict = k => k == ArtiParry;
        ArtiParry.HideConflict = k => k == ArtiJump;
    }

    public static void HideIrrelevantConfigs()
    {
        BackSlug.HideConfig = !ModManager.MSC && !ModManager.JollyCoop;

        Craft.HideConfig = !ModManager.MSC;
        ArtiJump.HideConfig = !ModManager.MSC || MachineConnector.IsThisModActive("danizk0.rebindartificer");
        ArtiParry.HideConfig = !ModManager.MSC || MachineConnector.IsThisModActive("danizk0.rebindartificer");
        MakeSpear.HideConfig = !ModManager.MSC;
        Ascend.HideConfig = !ModManager.MSC;
        AimAscend.HideConfig = !ModManager.MSC;
    }


    // Whether the custom input should be used
    public static bool IsArtiJumpCustomInput(this Player self)
    {
        return self.IsKeyBound(ArtiJump) && self.controller == null;
    }

    public static bool IsArtiParryCustomInput(this Player self)
    {
        return self.IsKeyBound(ArtiParry) && !ArtiParry.HideConfig && self.controller == null;
    }

    public static bool IsBackSpearCustomInput(this Player self)
    {
        return self.IsKeyBound(BackSpear) && !BackSpear.HideConfig && self.controller == null;
    }

    public static bool IsBackSlugCustomInput(this Player self)
    {
        return self.IsKeyBound(BackSlug) && !BackSlug.HideConfig && self.controller == null;
    }

    public static bool IsCraftCustomInput(this Player self)
    {
        return self.IsKeyBound(Craft) && !Craft.HideConfig && self.controller == null;
    }

    public static bool IsMakeSpearCustomInput(this Player self)
    {
        return self.IsKeyBound(MakeSpear) && !MakeSpear.HideConfig && self.controller == null;
    }

    public static bool IsAscendCustomInput(this Player self)
    {
        return self.IsKeyBound(Ascend) && !Ascend.HideConfig && self.controller == null;
    }

    public static bool IsAimAscendCustomInput(this Player self)
    {
        return self.IsKeyBound(AimAscend) && !AimAscend.HideConfig && self.controller == null;
    }

    public static bool IsGrappleCustomInput(this Player self)
    {
        return self.IsKeyBound(Grapple) && !Grapple.HideConfig && self.controller == null;
    }



    // Whether the custom input is pressed
    public static bool ArtiJumpPressed(this Player self)
    {
        var isCustomInput = IsArtiJumpCustomInput(self);
        var isParryOverride = ArtiJump.CurrentBinding(self.playerState.playerNumber) == ArtiParry.CurrentBinding(self.playerState.playerNumber) && self.input[0].y < 0 && self.bodyMode != Player.BodyModeIndex.ZeroG;

        var flag2 = self.eatMeat >= 20 || self.maulTimer >= 15;

        if (isCustomInput)
        {
            return self.JustPressed(ArtiJump) && self is { pyroJumpped: false, canJump: <= 0 } && !flag2 && !isParryOverride;
        }


        var flag = self.wantToJump > 0 && self.input[0].pckp;

        return flag && self is { pyroJumpped: false, canJump: <= 0 } && !flag2 && (self.input[0].y >= 0 || (self.input[0].y < 0 && (self.bodyMode == Player.BodyModeIndex.ZeroG || self.gravity <= 0.1f)));
    }

    public static bool ArtiParryPressed(this Player self)
    {
        var isCustomInput = IsArtiParryCustomInput(self);

        var flag = self.wantToJump > 0 && self.input[0].pckp;
        var flag2 = self.eatMeat >= 20 || self.maulTimer >= 15;


        if (isCustomInput)
        {
            return self.JustPressed(ArtiParry) && !self.submerged && !flag2;
        }

        return flag && !self.submerged && !flag2 && (self.input[0].y < 0 || self.bodyMode == Player.BodyModeIndex.Crawl) && (self.canJump > 0 || self.input[0].y < 0);
    }

    public static bool BackSpearPressed(this Player self)
    {
        if (self.PlayerGraspsHas(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) == 0 && self.PlayerGraspsHas(AbstractPhysicalObject.AbstractObjectType.Spear) >= 0)
        {
            return false;
        }

        var isCustomInput = IsBackSpearCustomInput(self);

        if (isCustomInput)
        {
            return self.IsPressed(BackSpear);
        }

        return self.input[0].pckp;
    }

    public static bool BackSlugPressed(this Player self)
    {
        if (self.PlayerGraspsHas(AbstractPhysicalObject.AbstractObjectType.Spear) == 0 && self.PlayerGraspsHas(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) >= 0)
        {
            return false;
        }

        var isCustomInput = IsBackSlugCustomInput(self);

        if (isCustomInput)
        {
            return self.IsPressed(BackSlug);
        }

        return self.input[0].pckp;
    }

    public static bool CraftPressed(this Player self)
    {
        var isCustomInput = IsCraftCustomInput(self);

        if (isCustomInput)
        {
            return self.IsPressed(Craft);
        }

        return self.input[0].pckp;
    }

    public static bool AimAscendPressed(this Player self)
    {
        var isCustomInput = IsAimAscendCustomInput(self);

        if (isCustomInput)
        {
            return self.IsPressed(AimAscend);
        }

        return self.input[0].thrw;
    }

    public static bool GrapplePressed(this Player self)
    {
        var isCustomInput = IsGrappleCustomInput(self);

        if (isCustomInput)
        {
            return self.IsPressed(Grapple);
        }

        return self.input[0].jmp;
    }

    public static bool MakeSpearPressed(this Player self)
    {
        var isCustomInput = IsMakeSpearCustomInput(self);

        if (isCustomInput)
        {
            return self.IsPressed(MakeSpear);
        }

        return self.input[0].pckp;
    }



    public static int PlayerGraspsHas(this Player self, AbstractPhysicalObject.AbstractObjectType type)
    {
        for (var i = 0; i < self.grasps.Length; i++)
        {
            var grasp = self.grasps[i];
            if (grasp == null)
            {
                continue;
            }

            if (grasp.grabbed.abstractPhysicalObject.type == type)
            {
                return i;
            }
        }

        return -1;
    }

    public static int PlayerGraspsHas(this Player self, CreatureTemplate.Type type)
    {
        for (var i = 0; i < self.grasps.Length; i++)
        {
            var grasp = self.grasps[i];
            if (grasp == null)
            {
                continue;
            }

            if (grasp.grabbed is not Creature creature)
            {
                continue;
            }

            if (creature.Template.type == type)
            {
                return i;
            }
        }

        return -1;
    }
}
