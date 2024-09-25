using Microsoft.AspNetCore.Components;
using SU_Lore.Database.Models.Pages;

namespace SU_Lore.Helpers.Animation;

public abstract class Animation
{
    /// <summary>
    /// Starts the animation.
    /// </summary>
    /// <param name="page">The page the animation is on.</param>
    /// <param name="setContent">The content to animate. Set this to the new content when changes are made.</param>
    public abstract void Start(Page page, Action<string> setContent, TaskCompletionSource<bool> tcs);
}