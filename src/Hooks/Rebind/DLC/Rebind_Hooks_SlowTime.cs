using On.Expedition;

namespace RebindEverything;

public static class Rebind_Hooks_SlowTime
{
    public static void ApplyHooks()
    {
        ExpeditionGame.SlowTimeTracker.Update += SlowTimeTrackerOnUpdate;
    }

    private static void SlowTimeTrackerOnUpdate(ExpeditionGame.SlowTimeTracker.orig_Update orig, Expedition.ExpeditionGame.SlowTimeTracker self)
    {
        if (self.game?.Players[self.playerNumber]?.realizedCreature is not Player player)
        {
            orig(self);
            return;
        }

        if (!player.IsSlowTimeCustomInput())
        {
            orig(self);
            return;
        }

        var input0 = player.input[0];
        var input1 = player.input[1];

        player.input[0].mp = player.SlowTimePressed();
        player.input[1].pckp = player.SlowTimePressed();

        // Prevent alternative conditions from triggering
        player.input[0].pckp = false;
        player.input[1].mp = false;

        orig(self);

        player.input[0] = input0;
        player.input[1] = input1;
    }
}
