using Menu.Remix.MixedUI;

namespace RebindEverything;

public class ModOptions : OptionsTemplate
{
    public static ModOptions Instance { get; } = new();

    public static void RegisterOI()
    {
        if (MachineConnector.GetRegisteredOI(Plugin.ModId) != Instance)
        {
            MachineConnector.SetRegisteredOI(Plugin.ModId, Instance);
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

        AddTab(ref tabIndex, "General");

        AddNewLine(2);
        
        AddTextLabel("To configure the input bindings with this mod, look at the normal Input Settings menu under Options!", FLabelAlignment.Center, true);
        DrawTextLabels(ref Tabs[tabIndex]);

        AddNewLine();

        AddCheckBox(ArtiJumpInput, (string)ArtiJumpInput.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tabIndex]);

        AddNewLinesUntilEnd();
        DrawBox(ref Tabs[tabIndex]);
    }
}
