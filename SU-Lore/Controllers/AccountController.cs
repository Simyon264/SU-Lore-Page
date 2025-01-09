using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SU_Lore.Data;
using SU_Lore.Database;
using SU_Lore.Database.Models.Accounts;
using SU_Lore.Helpers;

namespace SU_Lore.Controllers;

[Controller]
[Route("/account/")]
public class AccountController : Controller
{
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

        return Redirect("/?page=/system/listing");
    }
}