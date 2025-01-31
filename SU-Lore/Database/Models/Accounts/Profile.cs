namespace SU_Lore.Database.Models.Accounts;

public class Profile
{
    public int Id { get; set; }

    /// <summary>
    /// The unique identifier of the profile.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Short description of the user.
    /// </summary>
    public required string Bio { get; set; }

    /// <summary>
    /// The password of the profile. Can be null if a password is not required.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The account that created this profile.
    /// </summary>
    public Guid AccountId { get; set; }
    public Account Account { get; set; }
}