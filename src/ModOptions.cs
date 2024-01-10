using System.Collections.Generic;
using System.Xml.Linq;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace RebindEverything;

public class ModOptions : OptionsTemplate
{
    public static ModOptions Instance { get; } = new();
    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
            MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
    }

    public static Configurable<bool> artiJumpInput = Instance.config.Bind("artiJumpInput", true, new ConfigurableInfo(
        "When checked, pressing either Arti Jump or Arti Parry will cause a jump input, mimicking the original binding's behavior.",
        null, "", "Arti Jump Input?"));


    private const int NUMBER_OF_TABS = 1;

    public override void Initialize()
    {
        base.Initialize();
        Tabs = new OpTab[NUMBER_OF_TABS];
        int tabIndex = -1;

        AddTab(ref tabIndex, "General");

        AddNewLine(2);
        
        AddTextLabel("To configure the input bindings with this mod, look at the normal Input Settings menu under Options!", FLabelAlignment.Center, true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine(1);

        AddCheckBox(artiJumpInput, (string)artiJumpInput.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLine(13);
        DrawBox(ref Tabs[tabIndex]);
    }
}