﻿using Menu.Remix.MixedUI;

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

    public static int NumberOfTabs => 1;

    public override void Initialize()
    {
        base.Initialize();

        Tabs = new OpTab[NumberOfTabs];
        var tabIndex = -1;

        AddGeneralTab(ref tabIndex);
    }

    private void AddGeneralTab(ref int tabIndex)
    {
        AddTab(ref tabIndex, "General");

        AddNewLine(2);

        AddTextLabel("To configure the input bindings with this mod, look at the normal Input Settings menu under Options!", FLabelAlignment.Center, true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine();

        if (ModManager.MSC)
        {
            AddCheckBox(ArtiJumpInput);
            DrawCheckBoxes(ref Tabs[tabIndex], offsetX: 150.0f);

            if (GetConfigurable(ArtiJumpInput, out OpCheckBox checkBox))
            {
                checkBox.colorEdge = PlayerGraphics.DefaultSlugcatColor(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Artificer);
            }
        }

        AddNewLinesUntilEnd();
        DrawBox(ref Tabs[tabIndex]);
    }
}
