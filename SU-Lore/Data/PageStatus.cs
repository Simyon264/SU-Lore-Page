using Microsoft.AspNetCore.Components;

namespace SU_Lore.Data;

public class PageStatus
{
    public MarkupString Content { get; set; } = new MarkupString();
    
    public int CurrentTextSpeed { get; set; } = Constants.DefaultTextSpeed;
    public bool IsInstantText { get; set; } = false;
    
    /// <summary>
    /// A stack of previous pages that were visited.
    /// </summary>
    public Stack<string> PageStack { get; set; } = new();
}