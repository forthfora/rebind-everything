namespace RebindEverything;

public static class Hooks
{
    public static bool IsInit { get; private set; }

    public static void ApplyHooks()
    {
        Rebind_Hooks_Back.ApplyHooks();
        Rebind_Hooks_GrappleWorm.ApplyHooks();
        Rebind_Hooks_PoleGrab.ApplyHooks();

        Rebind_Hooks_Craft.ApplyHooks();

        Rebind_Hooks_Artificer.ApplyHooks();
        Rebind_Hooks_Saint.ApplyHooks();
        Rebind_Hooks_Spearmaster.ApplyHooks();

        Rebind_Hooks_Watcher.ApplyHooks();
    }

    public static void ApplyInit()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
    }

    private static void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        try
        {
            ModOptions.RegisterOI();
            
            if (IsInit)
            {
                return;
            }

            IsInit = true;

            var mod = ModManager.ActiveMods.FirstOrDefault(mod => mod.id == Plugin.MOD_ID);

            if (mod is null)
            {
                throw new Exception("Could not find mod ID!");
            }

            Plugin.ModName = mod.name;
            Plugin.Version = mod.version;
            Plugin.Authors = mod.authors;

            ApplyHooks();
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

        Input_Helpers.HideIrrelevantConfigs();
    }
}
