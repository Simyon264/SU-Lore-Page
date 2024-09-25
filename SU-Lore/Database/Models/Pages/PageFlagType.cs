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

    /// <summary>
    /// Dictates that the page will use custom CSS. The value should be the CSS to use.
    /// </summary>
    [PageFlag("If set, the page will use custom CSS. The value should be the CSS to use.", hasValue: true)]
    CustomCss = 5,

    /// <summary>
    /// Once the page is rendered fully, it will redirect to the value. If instant is set, it will redirect instantly.
    /// </summary>
    [PageFlag("On end of rendering, the page will redirect to the value. If instant is set, it will redirect instantly.", hasValue: true)]
    Redirect = 6,

    /// <summary>
    /// What animation should be used when the page is entered. If set, the page text will always be hidden until the animation is done.
    /// </summary>
    [PageFlag("What animation should be used when the page is entered. If set, the page text will always be hidden until the animation is done.", hasValue: true)]
    EnterAnimation = 7,

    /// <summary>
    /// Should this page show up in the syndicate listing?
    /// </summary>
    [PageFlag("Should this page show up in the syndicate listing?", hasValue: false)]
    Syndicate = 8,
}