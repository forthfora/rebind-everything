using ImprovedInput;
using UnityEngine;

namespace RebindEverything;

public static class Input_Helpers
{
    public static PlayerKeybind BackSpear { get; } = PlayerKeybind.Register("rebindeverything:backspear", "Rebind Everything", "Back Spear", KeyCode.None, KeyCode.None);
    public static PlayerKeybind BackSlug { get; } = PlayerKeybind.Register("rebindeverything:backslug", "Rebind Everything", "Back Slug", KeyCode.None, KeyCode.None);
    public static PlayerKeybind PoleGrab { get; } = PlayerKeybind.Register("rebindeverything:polegrab", "Rebind Everything", "Pole Grab", KeyCode.None, KeyCode.None);

    public static PlayerKeybind Craft { get; } = PlayerKeybind.Register("rebindeverything:craft", "Rebind Everything", "Craft", KeyCode.None, KeyCode.None);

    public static PlayerKeybind ArtiJump { get; } = PlayerKeybind.Register("rebindeverything:artijump", "Rebind Everything", "Arti Jump", KeyCode.None, KeyCode.None);
    public static PlayerKeybind ArtiParry { get; } = PlayerKeybind.Register("rebindeverything:artiparry", "Rebind Everything", "Arti Parry", KeyCode.None, KeyCode.None);

    public static PlayerKeybind MakeSpear { get; } = PlayerKeybind.Register("rebindeverything:makespear", "Rebind Everything", "Make Spear", KeyCode.None, KeyCode.None);

    public static PlayerKeybind Ascend { get; } = PlayerKeybind.Register("rebindeverything:ascend", "Rebind Everything", "Ascend", KeyCode.None, KeyCode.None);
    public static PlayerKeybind AimAscend { get; } = PlayerKeybind.Register("rebindeverything:aimascend", "Rebind Everything", "Aim Ascend", KeyCode.None, KeyCode.None);

    public static PlayerKeybind Grapple { get; } = PlayerKeybind.Register("rebindeverything:grapple", "Rebind Everything", "Grapple", KeyCode.None, KeyCode.None);

    public static PlayerKeybind Camo { get; } = PlayerKeybind.Register("rebindeverything:camo", "Rebind Everything", "Camo", KeyCode.None, KeyCode.None);
    public static PlayerKeybind Warp { get; } = PlayerKeybind.Register("rebindeverything:warp", "Rebind Everything", "Warp", KeyCode.None, KeyCode.None);


    public static void InitInput()
    {
        BackSpear.Description = "The key held to make Hunter either put or retrieve a spear from their back.";
        BackSlug.Description = "The key held to put or retrieve a Slugcat from your back.";
        PoleGrab.Description = "Key pressed to grab poles (default is to hold up).";

        Craft.Description = "The key held to make Artificer or Gourmand craft the items they are holding.";

        ArtiJump.Description = "The key pressed to make Artificer double jump, only works mid-air.";
        ArtiParry.Description = "The key pressed to make Artificer parry, forces a down input.";

        MakeSpear.Description = "The key held to have Spearmaster make a new spear.";

        Ascend.Description = "The key pressed to toggle Saint's ascension mode.";
        AimAscend.Description = "The key held to move the Saint's ascension reticule around.";

        Grapple.Description = "Affects Saint's Tongue & Grapple Worms.";

        Camo.Description = "Key pressed to trigger Watcher's camouflage ability.";
        Warp.Description = "Key pressed to trigger Watcher's warp ability.";

        BackSpear.HideConflict = k => k == BackSlug;
        BackSlug.HideConflict = k => k == BackSpear;

        ArtiJump.HideConflict = k => k == ArtiParry;
        ArtiParry.HideConflict = k => k == ArtiJump;
    }

    public static void HideIrrelevantConfigs()
    {
        BackSlug.HideConfig = !ModManager.MSC && !ModManager.JollyCoop && !MachineConnector.IsThisModActive("henpemaz_rainmeadow");

        Craft.HideConfig = !ModManager.MSC;
        ArtiJump.HideConfig = !ModManager.MSC || MachineConnector.IsThisModActive("danizk0.rebindartificer");
        ArtiParry.HideConfig = !ModManager.MSC || MachineConnector.IsThisModActive("danizk0.rebindartificer");
        MakeSpear.HideConfig = !ModManager.MSC;
        Ascend.HideConfig = !ModManager.MSC;
        AimAscend.HideConfig = !ModManager.MSC;

        Camo.HideConfig = !ModManager.Watcher;
        Warp.HideConfig = !ModManager.Watcher;
    }


    // Whether the custom input should be used
    public static bool IsArtiJumpCustomInput(this Player self)
    {
        return self.IsKeyBound(ArtiJump) && self.controller is null;
    }

    public static bool IsArtiParryCustomInput(this Player self)
    {
        return self.IsKeyBound(ArtiParry) && !ArtiParry.HideConfig && self.controller is null;
    }

    public static bool IsBackSpearCustomInput(this Player self)
    {
        return self.IsKeyBound(BackSpear) && !BackSpear.HideConfig && self.controller is null;
    }

    public static bool IsBackSlugCustomInput(this Player self)
    {
        return self.IsKeyBound(BackSlug) && !BackSlug.HideConfig && self.controller is null;
    }

    public static bool IsCraftCustomInput(this Player self)
    {
        return self.IsKeyBound(Craft) && !Craft.HideConfig && self.controller is null;
    }

    public static bool IsMakeSpearCustomInput(this Player self)
    {
        return self.IsKeyBound(MakeSpear) && !MakeSpear.HideConfig && self.controller is null;
    }

    public static bool IsAscendCustomInput(this Player self)
    {
        return self.IsKeyBound(Ascend) && !Ascend.HideConfig && self.controller is null;
    }

    public static bool IsAimAscendCustomInput(this Player self)
    {
        return self.IsKeyBound(AimAscend) && !AimAscend.HideConfig && self.controller is null;
    }

    public static bool IsGrappleCustomInput(this Player self)
    {
        return self.IsKeyBound(Grapple) && !Grapple.HideConfig && self.controller is null;
    }

    public static bool IsCamoCustomInput(this Player self)
    {
        return self.IsKeyBound(Camo) && !Camo.HideConfig && self.controller is null;
    }

    public static bool IsWarpCustomInput(this Player self)
    {
        return self.IsKeyBound(Warp) && !Warp.HideConfig && self.controller is null;
    }

    public static bool IsPoleGrabCustomInput(this Player self)
    {
        return self.IsKeyBound(PoleGrab) && !PoleGrab.HideConfig && self.controller is null;
    }


    // Whether the custom input is pressed
    public static bool ArtiJumpPressed(this Player self)
    {
        var isCustomInput = IsArtiJumpCustomInput(self);
        var isParryOverride = ArtiJump.CurrentBinding(self.playerState.playerNumber) == ArtiParry.CurrentBinding(self.playerState.playerNumber) && self.input[0].y < 0 && self.bodyMode != Player.BodyModeIndex.ZeroG;

        var eatFlag = self.eatMeat >= 20 || self.maulTimer >= 15;

        if (isCustomInput)
        {
            return self.JustPressed(ArtiJump) && self is { pyroJumpped: false, canJump: <= 0 } && !eatFlag && !isParryOverride;
        }

        var inputFlag = self.wantToJump > 0 && self.input[0].pckp;

        return inputFlag && self is { pyroJumpped: false, canJump: <= 0 } && !eatFlag && (self.input[0].y >= 0 || (self.input[0].y < 0 && (self.bodyMode == Player.BodyModeIndex.ZeroG || self.gravity <= 0.1f)));
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

        return IsBackSpearCustomInput(self) ? self.IsPressed(BackSpear) : self.input[0].pckp;
    }

    public static bool BackSlugPressed(this Player self)
    {
        if (self.PlayerGraspsHas(AbstractPhysicalObject.AbstractObjectType.Spear) == 0 && self.PlayerGraspsHas(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) >= 0)
        {
            return false;
        }

        return IsBackSlugCustomInput(self) ? self.IsPressed(BackSlug) : self.input[0].pckp;
    }

    public static bool CraftPressed(this Player self)
    {
        return IsCraftCustomInput(self) ? self.IsPressed(Craft) : self.input[0].pckp;
    }

    public static bool AimAscendPressed(this Player self)
    {
        return IsAimAscendCustomInput(self) ? self.IsPressed(AimAscend) : self.input[0].thrw;
    }

    public static bool GrapplePressed(this Player self)
    {
        return IsGrappleCustomInput(self) ? self.IsPressed(Grapple) : self.input[0].jmp;
    }

    public static bool MakeSpearPressed(this Player self)
    {
        return IsMakeSpearCustomInput(self) ? self.IsPressed(MakeSpear) : self.input[0].pckp;
    }

    public static bool CamoPressed(this Player self)
    {
        return IsCamoCustomInput(self) ? self.IsPressed(Camo) : self.input[0].spec;
    }

    public static bool WarpPressed(this Player self)
    {
        return IsWarpCustomInput(self) ? self.IsPressed(Warp) : self.input[0].spec;
    }

    public static bool PoleGrabPressed(this Player self)
    {
        if ((ModManager.MSC && self.monkAscension) || self.Submersion > 0.9f)
        {
            return false;
        }

        return IsPoleGrabCustomInput(self) ? self.IsPressed(PoleGrab) : self.input[0].y > 0;
    }

    // Back Spear & Slug Helpers
    public static int PlayerGraspsHas(this Player self, AbstractPhysicalObject.AbstractObjectType type)
    {
        for (var i = 0; i < self.grasps.Length; i++)
        {
            var grasp = self.grasps[i];
            if (grasp is null)
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

            if (grasp?.grabbed is not Creature creature)
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
