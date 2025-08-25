namespace RebindEverything;

public sealed class PlayerModule
{
    public bool WasMakeSpearInputRegistered { get; set; }
    public bool HadASlug { get; set; }
    public bool CanTakeSlugOffBack { get; set; }
    public bool IsCrafting { get; set; }

    public bool IsBackSpearIncrementing { get; set; }
    public bool IsBackSlugIncrementing { get; set; }
}
