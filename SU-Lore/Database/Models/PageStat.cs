namespace SU_Lore.Database.Models;

public class PageStat
{
    public int Id { get; set; }
    
    /// <summary>
    /// The page that this stat is for.
    /// </summary>
    public Guid PageId { get; set; }
    
    /// <summary>
    /// The view count for this page.
    /// </summary>
    public int Views { get; set; }
}