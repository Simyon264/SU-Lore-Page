using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SU_Lore.Database.Models.Accounts;

namespace SU_Lore.Database.Models.Pages;

/// <summary>
/// Represents a page that can be viewed and edited.
/// </summary>
[PrimaryKey("Id")]
public class Page
{
    /// <summary>
    /// A unique identifier for the page. This is auto-incremented.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The version of this page. After an edit, this number is incremented by one and the edits will be saved onto a copy that is now the "working page".
    /// </summary>
    public required int Version { get; set; }

    /// <summary>
    /// The title of this page. This is what will be displayed in the title bar of the browser and on the terminal top bar.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The page content. This is the actual text that will be displayed on the page.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// A hashset of flags that can be set on the page to change its behavior.
    /// </summary>
    public required HashSet<PageFlag> Flags { get; set; }

    /// <summary>
    /// When was this page created?
    /// </summary>
    public required DateTime CreatedAt { get; set; }

    /// <summary>
    /// When was this page last updated?
    /// </summary>
    public required DateTime UpdatedAt { get; set; }

    /// <summary>
    /// The account that created this page.
    /// </summary>
    public required Guid CreatedBy { get; set; }

    /// <summary>
    /// The account that last updated this page.
    /// </summary>
    public required Guid UpdatedBy { get; set; }

    /// <summary>
    /// The virtual path of the page. This is the path that the page will be accessible from and will dictate how it gets displayed in file listings.
    /// </summary>
    public required string VirtualPath { get; set; } = "/";

    /// <summary>
    /// The GUID of this page "family". This is used to group multiple versions of the same page together.
    /// </summary>
    public required Guid PageGuid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The profile that created this page. May be null if the person who created the page is not logged into a profile.
    /// </summary>
    public string? ProfileCreated { get; set; }

    /// <summary>
    /// The profile that last updated this page. May be null if the person who updated the page is not logged into a profile.
    /// </summary>
    public string? ProfileUpdated { get; set; }
}