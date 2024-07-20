namespace SU_Lore.Database.Models.Accounts;

/// <summary>
/// Represents a role that an account can have.
/// </summary>
public enum Role
{
    #region Admin Roles
    // Roles that have permissions assigned.
    
    /// <summary>
    /// This person has direct database access and therefore can do anything.
    /// </summary>
    DatabaseAdmin = 0,
    
    /// <summary>
    /// Full permissions to the website, able to do anything but not directly access the database.
    /// </summary>
    Admin = 1,
    
    /// <summary>
    /// Able to access any content generated but no administrative permissions. I.e. viewing logs, etc.
    /// </summary>
    Moderator = 2,
    
    /// <summary>
    /// Allowed to edit and create pages as well as uploading files.
    /// </summary>
    Whitelisted = 3,
    
    #endregion
    
    
    #region Flavor Roles

    // Roles that are purely for flavor and have no special permissions. Specific to sector umbra.
    
    ProjectManager = 100,
    GameMaster = 101,
    Maintainer = 102,
    Employee = 103,
    //TsjipTsjip — Today at 12:35
    //add bee role
    //Do it now
    Bee = 104,
    Chicken = 105,

    #endregion
}

public static class RoleExtensions
{
    public static bool HasRole(this List<Role> roles, Role role)
    {
        return roles.Contains(role);
    }
    
    /// <summary>
    /// Checks if the list of roles contains any of the roles provided.
    /// </summary>
    /// <param name="roles"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public static bool HasAnyRole(this List<Role> roles, params Role[] role)
    {
        return role.Any(roles.Contains);
    }
}