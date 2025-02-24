
using System.Runtime.CompilerServices;

namespace RebindEverything;

public static class ModuleManager
{
    private static ConditionalWeakTable<AbstractCreature, PlayerModule> PlayerData { get; } = new();

    public static PlayerModule GetModule(this Player player)
    {
        return PlayerData.GetValue(player.abstractCreature, _ => new PlayerModule());
    }
}

