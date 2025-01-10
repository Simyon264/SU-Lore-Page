namespace SU_Lore.Database.Models.Accounts;

public class Profile
{
    public string Name { get; set; }
    public string Bio { get; set; }

    public string Password { get; set; }

    /// <summary>
    /// The account that created this profile.
    /// </summary>
    public Guid AccountId { get; set; }
    public Account Account { get; set; }
}