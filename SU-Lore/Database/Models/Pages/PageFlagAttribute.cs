namespace SU_Lore.Database.Models.Pages;

[AttributeUsage(AttributeTargets.Field)]
public class PageFlagAttribute : Attribute
{
    public string Description { get; }
    public bool HasValue { get; }
    public bool Hidden { get; }
    public bool HideInApi { get; }

    public PageFlagAttribute(string description, bool hasValue = true, bool hidden = false, bool hideInApi = false)
    {
        Description = description;
        HasValue = hasValue;
        Hidden = hidden;
        HideInApi = hideInApi;
    }
}