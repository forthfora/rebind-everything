using On.MoreSlugcats;

namespace RebindEverything;

public static class Rebind_Hooks_RivCell
{
    public static void ApplyHooks()
    {
        EnergyCell.Update += EnergyCellOnUpdate;
    }

    private static void EnergyCellOnUpdate(EnergyCell.orig_Update orig, MoreSlugcats.EnergyCell self, bool eu)
    {
        if (self.grabbedBy.Count == 0 || self.grabbedBy[0].grabber is not Player player || !player.IsRivCellCustomInput())
        {
            orig(self, eu);
            return;
        }

        var input = player.input[0];

        player.input[0].pckp = player.RivCellPressed();

        orig(self, eu);

        player.input[0] = input;
    }
}
