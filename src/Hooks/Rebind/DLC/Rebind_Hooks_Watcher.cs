using System.Reflection;
using MonoMod.RuntimeDetour;
using Watcher;

namespace RebindEverything;

public static class Rebind_Hooks_Watcher
{
    public static void ApplyHooks()
    {
        try
        {
            _ = new Hook(
                typeof(Player).GetProperty(nameof(Player.RippleAbilityActivationButtonCondition), BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(),
                typeof(Rebind_Hooks_Watcher).GetMethod(nameof(RippleAbilityActivationButtonCondition), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        try
        {
            _ = new Hook(
                typeof(Player).GetProperty(nameof(Player.watcherDynamicWarpInput), BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod(),
                typeof(Rebind_Hooks_Watcher).GetMethod(nameof(WatcherDynamicWarpInput), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        catch (Exception e)
        {
            e.LogHookException();
        }

        On.Player.WatcherUpdate += PlayerOnWatcherUpdate;
    }

    private static void PlayerOnWatcherUpdate(On.Player.orig_WatcherUpdate orig, Player self)
    {
        orig(self);

        if (!self.IsCamoCustomInput())
        {
            return;
        }

        if (!ModManager.Watcher || self.SlugCatClass != WatcherEnums.SlugcatStatsName.Watcher)
        {
            return;
        }

        if (!self.CamoPressed())
        {
            self.camoInputsNeedReset = true;
        }
    }

    private static bool RippleAbilityActivationButtonCondition(Func<Player, bool> orig, Player self)
    {
        if (!self.IsCamoCustomInput())
        {
            return orig(self);
        }

        var temp = self.input[0].spec;

        self.input[0].spec = self.CamoPressed();

        var result = orig(self);

        self.input[0].spec = temp;

        return result;
    }

    private static bool WatcherDynamicWarpInput(Func<Player, bool> orig, Player self)
    {
        if (!self.IsWarpCustomInput())
        {
            return orig(self);
        }

        var tempSpec = self.input[0].spec;
        var tempY = self.input[0].y;

        self.input[0].spec = self.WarpPressed();
        self.input[0].y = self.WarpPressed() ? 1 : 0;

        var result = orig(self);

        self.input[0].spec = tempSpec;
        self.input[0].y = tempY;

        return result;
    }
}
