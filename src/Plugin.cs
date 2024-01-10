using BepInEx;
using BepInEx.Logging;
using System;
using System.Security.Permissions;
using System.Security;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete


namespace RebindEverything;

[BepInPlugin(MOD_ID, MOD_ID, "1.0.0")]
internal class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "rebindeverything";


    public static string MOD_NAME = "";
    public static string VERSION = "";
    public static string AUTHORS = "";

    public static new ManualLogSource Logger { get; private set; } = null!;

    public void OnEnable()
    {
        Logger = base.Logger;
        Hooks.ApplyInit();
    }
}
