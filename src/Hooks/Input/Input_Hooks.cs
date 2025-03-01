using UnityEngine;

namespace RebindEverything;

public static class Input_Hooks
{
    public static void ApplyHooks()
    {
        On.RainWorldGame.Update += RainWorldGameOnUpdate;
    }

    private static void RainWorldGameOnUpdate(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        for (var i = 0; i < Input_Helpers.MaxMouseButtonIndex + 1; i++)
        {
            Input_Helpers.MouseButtonIndexToWasInput[i] = Input.GetMouseButton(i);
        }
    }
}
