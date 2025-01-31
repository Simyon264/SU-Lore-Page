namespace SU_Lore.Database.Models.Pages;

/// <summary>
/// A comment for a page.
/// </summary>
public class PageComment
{
    public int Id { get; set; }

    /// <summary>
    /// What page is this comment for?
    /// </summary>
    public Guid PageId { get; set; }

    /// <summary>
    /// The content of the comment.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// The account that made this comment.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// The profile that made this comment.
    /// </summary>
    public string ProfileName { get; set; } = null!;

    /// <summary>
    /// When was this comment created?
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}