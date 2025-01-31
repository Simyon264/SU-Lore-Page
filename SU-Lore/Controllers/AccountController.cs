using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SU_Lore.Data;
using SU_Lore.Database;
using SU_Lore.Database.Models.Accounts;
using SU_Lore.Helpers;

namespace SU_Lore.Controllers;

[Controller]
[Route("/account/")]
public class AccountController : Controller
{
    /// <summary>
    /// Contains tickets for login into profiles.
    /// Key: Ticket
    /// Value: Profile name
    /// </summary>
    public static ConcurrentDictionary<Guid, string> ProfileTickets = new();

    public const string ProfileClaim = "Profile";

    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Route("login")]
    [HttpGet]
    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties()
        {
            RedirectUri = "/account/redirect",
        });
    }

    [Route("logout")]
    [HttpGet]
    public async Task<IActionResult> Logout(
        [FromQuery] string redirect = "/system/listing"
        )
    {
        await HttpContext.SignOutAsync("Cookies");
        return Redirect($"/?page={redirect}");
    }

    [Route("redirect")]
    [HttpGet]
    public async Task<IActionResult> RedirectFromLogin()
    {
        if (!User.Identity?.IsAuthenticated ?? false)
        {
            return Unauthorized();
        }
        var guid = Guid.Parse(User.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value);

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == guid);
        var username = await Ss14ApiHelper.FetchAccountFromGuid(guid);
        if (username == null)
        {
            return StatusCode(500, "Failed to fetch account information from SS14 API.");
        }

        if (account == null)
        {
            // New person, create an account for them.
            account = new Account()
            {
                Id = guid,
                Username = username.userName,
                Roles = new List<Role>() {Role.Employee}, // This is to prevent errors. stupiddd
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
        }

        // Is the username in the db and the api the same?
        if (account.Username != username.userName)
        {
            account.Username = username.userName;
            await _context.SaveChangesAsync();
        }

        User.AddIdentity(new ClaimsIdentity(new List<Claim>()
        {
            new Claim(ProfileClaim, account.Username),
        }, "Cookies"));

        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(User));

        Log.Information("User {Username} logged in. Profile: {Profile}", account.Username, GetProfile(User));

        return Redirect("/?page=/system/listing");
    }

    [HttpGet]
    [Route("profile/login")]
    public async Task<IActionResult> ProfileLogin(
        [FromQuery] Guid ticket
        )
    {
        if (!ProfileTickets.TryRemove(ticket, out var profile))
        {
            return NotFound("Invalid ticket.");
        }

        var guid = Guid.Parse(User.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value);
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == guid);
        if (account == null)
        {
            return BadRequest("You need to be logged in to an account to log into a profile.");
        }

        var user = User as ClaimsPrincipal;
        var identity = user.Identity as ClaimsIdentity;
        var claims = identity.Claims.Where(c => c.Type != ProfileClaim).ToList();

        claims.Add(new Claim(ProfileClaim, profile));

        var newIdentity = new ClaimsIdentity(claims, "Cookies");
        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(newIdentity));

        Log.Information("User {Username} logged in. Profile: {Profile}", account.Username, GetProfile(User));
        return Redirect("/?page=/system/listing");
    }

    /// <summary>
    /// Logs you out of your profile but not the account.
    /// </summary>
    [HttpGet]
    [Route("profile/logout")]
    public async Task<IActionResult> ProfileLogout()
    {
        var profile = GetProfile(User);
        if (profile == "Unknown")
        {
            return Unauthorized("You are not logged into a profile.");
        }

        var guid = Guid.Parse(User.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value);
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == guid);
        if (account == null)
        {
            return Unauthorized("You are not logged into an account.");
        }

        var user = User as ClaimsPrincipal;
        var identity = user.Identity as ClaimsIdentity;
        var claims = identity.Claims.Where(c => c.Type != ProfileClaim).ToList();

        claims.Add(new Claim(ProfileClaim, account.Username));

        var newIdentity = new ClaimsIdentity(claims, "Cookies");
        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(newIdentity));

        Log.Information("User {Username} logged out of profile {Profile}.", account.Username, profile);
        return Redirect("/?page=/system/listing");
    }

    public static string GetProfile(ClaimsPrincipal user)
    {
        return user.FindFirst(ProfileClaim)?.Value ?? "Unknown";
    }
}