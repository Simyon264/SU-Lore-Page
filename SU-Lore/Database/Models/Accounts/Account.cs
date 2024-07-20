using Microsoft.EntityFrameworkCore;

namespace SU_Lore.Database.Models.Accounts;

/// <summary>
/// Represents a user account.
/// </summary>
[PrimaryKey("Id")]
public class Account
{
    /// <summary>
    /// The unique identifier of the account.
    /// </summary>
    public required Guid Id { get; set; }
    
    /// <summary>
    /// The username of the account.
    /// </summary>
    public required string Username { get; set; }
    
    /// <summary>
    /// Roles that the account has. This is a set because an account can only have one of each role. Would be weird if you were a db admin twice.
    /// </summary>
    public List<Role> Roles { get; set; } = new();
}