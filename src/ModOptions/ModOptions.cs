using Menu.Remix.MixedUI;
using UnityEngine;
using Custom = RWCustom.Custom;

namespace RebindEverything;

public class ModOptions : OptionsTemplate
{
    public static ModOptions Instance { get; } = new();

    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
        {
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
        }
    }

    public static Configurable<bool> ArtiJumpInput { get; } = Instance.config.Bind("artiJumpInput", true, new ConfigurableInfo(
        "When checked, pressing either Arti Jump or Arti Parry will cause a jump input, mimicking the original binding's behavior.",
        null, "", "Arti Jump Input?"));


    public static Configurable<int> MouseButtonBackSpear { get; } = Instance.config.Bind(nameof(MouseButtonBackSpear), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Back Spear Mouse Button"));

    public static Configurable<int> MouseButtonBackSlug { get; } = Instance.config.Bind(nameof(MouseButtonBackSlug), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Back Slug Mouse Button"));

    public static Configurable<int> MouseButtonCraft { get; } = Instance.config.Bind(nameof(MouseButtonCraft), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Craft Mouse Button"));

    public static Configurable<int> MouseButtonArtiJump { get; } = Instance.config.Bind(nameof(MouseButtonArtiJump), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Arti Jump Mouse Button"));

    public static Configurable<int> MouseButtonArtiParry { get; } = Instance.config.Bind(nameof(MouseButtonArtiParry), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Arti Parry Mouse Button"));

    public static Configurable<int> MouseButtonMakeSpear { get; } = Instance.config.Bind(nameof(MouseButtonMakeSpear), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Make Spear Mouse Button"));

    public static Configurable<int> MouseButtonAscend { get; } = Instance.config.Bind(nameof(MouseButtonAscend), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Ascend Mouse Button"));

    public static Configurable<int> MouseButtonAimAscend { get; } = Instance.config.Bind(nameof(MouseButtonAimAscend), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Aim Ascend Mouse Button"));

    public static Configurable<int> MouseButtonGrapple { get; } = Instance.config.Bind(nameof(MouseButtonGrapple), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Grapple Mouse Button"));

    public static Configurable<int> MouseButtonCamo { get; } = Instance.config.Bind(nameof(MouseButtonCamo), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Camo Mouse Button"));

    public static Configurable<int> MouseButtonWarp { get; } = Instance.config.Bind(nameof(MouseButtonWarp), 0, new ConfigurableInfo(
        "Mouse button index to trigger the action. 0 to disable. Hold and drag up or down to change.",
        new ConfigAcceptableRange<int>(0, Input_Helpers.MaxMouseButtonIndex + 1), "",
        "Warp Mouse Button"));

    public static int NumberOfTabs => 3;

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[NumberOfTabs];
        var tabIndex = -1;

        AddGeneralTab(ref tabIndex);

        if (ModManager.MSC)
        {
            AddMSCTab(ref tabIndex);
        }

        if (ModManager.Watcher)
        {
            AddWatcherTab(ref tabIndex);
        }
    }

    private void AddGeneralTab(ref int tabIndex)
    {
        AddTab(ref tabIndex, "General");

        AddNewLine();

        AddTextLabel("To configure the input bindings with this mod, look at the normal Input Settings menu under Options!", FLabelAlignment.Center, true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine();

        AddDragger(MouseButtonBackSpear);
        DrawDraggers(ref Tabs[tabIndex], 150.0f);

        AddDragger(MouseButtonGrapple);
        DrawDraggers(ref Tabs[tabIndex], 150.0f);

        if (ModManager.MSC || ModManager.JollyCoop || MachineConnector.IsThisModActive("henpemaz_rainmeadow"))
        {
            AddDragger(MouseButtonBackSlug);
            DrawDraggers(ref Tabs[tabIndex], 150.0f);
        }

        AddNewLinesUntilEnd();
        DrawBox(ref Tabs[tabIndex]);

        if (GetConfigurable(MouseButtonBackSpear, out OpDragger dragger))
        {
            dragger.colorText = dragger.colorEdge = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.Red);
        }
    }

    private void AddMSCTab(ref int tabIndex)
    {
        AddTab(ref tabIndex, "MSC");
        Tabs[tabIndex].colorButton = Custom.hexToColor("ceffff");

        AddCheckBox(ArtiJumpInput);
        DrawCheckBoxes(ref Tabs[tabIndex], offsetX: 150.0f);

        if (GetConfigurable(ArtiJumpInput, out OpCheckBox checkBox))
        {
            checkBox.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer);
        }

        AddNewLine();

        AddDragger(MouseButtonCraft);
        AddDragger(MouseButtonMakeSpear);
        DrawDraggers(ref Tabs[tabIndex]);

        AddDragger(MouseButtonArtiJump);
        AddDragger(MouseButtonArtiParry);
        DrawDraggers(ref Tabs[tabIndex]);

        AddDragger(MouseButtonAscend);
        AddDragger(MouseButtonAimAscend);
        DrawDraggers(ref Tabs[tabIndex]);

        AddNewLinesUntilEnd();
        DrawBox(ref Tabs[tabIndex]);

        if (GetConfigurable(MouseButtonCraft, out OpDragger dragger))
        {
            dragger.colorText = dragger.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
        }

        if (GetConfigurable(MouseButtonMakeSpear, out dragger))
        {
            dragger.colorText = dragger.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Spear);
        }

        if (GetConfigurable(MouseButtonArtiJump, out dragger))
        {
            dragger.colorText = dragger.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer);
        }

        if (GetConfigurable(MouseButtonArtiParry, out dragger))
        {
            dragger.colorText = dragger.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer);
        }

        if (GetConfigurable(MouseButtonAscend, out dragger))
        {
            dragger.colorText = dragger.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint);
        }

        if (GetConfigurable(MouseButtonAimAscend, out dragger))
        {
            dragger.colorText = dragger.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint);
        }
    }

    private void AddWatcherTab(ref int tabIndex)
    {
        var watcherColor = Color.Lerp(PlayerGraphics.DefaultSlugcatColor(Watcher.WatcherEnums.SlugcatStatsName.Watcher), Color.white, 0.1f);

        AddTab(ref tabIndex, "Watcher");
        Tabs[tabIndex].colorButton = watcherColor;

        AddDragger(MouseButtonCamo);
        AddDragger(MouseButtonWarp);
        DrawDraggers(ref Tabs[tabIndex]);

        AddNewLinesUntilEnd();
        DrawBox(ref Tabs[tabIndex]);

        if (GetConfigurable(MouseButtonCamo, out OpDragger dragger))
        {
            dragger.colorText = dragger.colorEdge = watcherColor;
        }

        if (GetConfigurable(MouseButtonWarp, out dragger))
        {
            dragger.colorText = dragger.colorEdge = watcherColor;
        }
    }
}
