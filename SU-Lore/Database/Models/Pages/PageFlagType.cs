namespace SU_Lore.Database.Models.Pages;

/// <summary>
/// The type of flag that can be set on a page.
/// </summary>
public enum PageFlagType
{
    /// <summary>
    /// The page should hide the title
    /// </summary>
    [PageFlag("If set, the title of the page will not be displayed on the top.", hasValue: false)]
    HideTitle = 0,
    
    /// <summary>
    /// The page should hide the back button.
    /// </summary>
    [PageFlag("If set, the back button will not be displayed on the top.", hasValue: false, hidden:true)]
    HideBackButton = 1,
    
    /// <summary>
    /// Dictates that the page will automatically scroll to the bottom during rendering.
    /// </summary>
    [PageFlag("If set, the page will automatically scroll to the bottom when rendered.", hasValue: false)]
    ScrollToBottom = 2,
    
    /// <summary>
    /// The page should be password protected. The password being the value.
    /// </summary>
    [PageFlag("If set, the page will be password protected. The password being the value.", hasValue:true)]
    PasswordProtected = 3,
    
    [PageFlag("If set, the page will not be listed in the navigation.", hasValue: false)]
    Unlisted = 4,
}