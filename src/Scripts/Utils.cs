using System.Runtime.CompilerServices;

namespace RebindEverything;

public static class Utils
{
    public static void LogHookException(this Exception e, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
    {
        Plugin.Logger.LogError($"Caught exception applying a hook! May not be fatal, but likely to cause issues." +
                               $"\nRelated to ({Path.GetFileNameWithoutExtension(filePath)}.{memberName}). Details:" +
                               $"\n{e}");
    }
}
