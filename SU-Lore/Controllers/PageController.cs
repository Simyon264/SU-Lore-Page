using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SU_Lore.Data;
using SU_Lore.Data.RichText;
using SU_Lore.Database;
using SU_Lore.Database.Models.Accounts;
using SU_Lore.Database.Models.Pages;
using SU_Lore.Helpers;

namespace SU_Lore.Controllers;

[Controller]
[Route("/api/page/")]
public class PageController : Controller
{
    private ApplicationDbContext _context;
    private PageReader _pageReader;
    private AuthenticationHelper _authHelper;
    private AuthenticationStateProvider _authStateProvider;

    public PageController(ApplicationDbContext context, PageReader pageReader, AuthenticationHelper authHelper, AuthenticationStateProvider authStateProvider)
    {
        _context = context;
        _pageReader = pageReader;
        _authHelper = authHelper;
        _authStateProvider = authStateProvider;
    }

    [HttpGet("/page")]
    [ProducesResponseType<Page>(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)] // wrong password (if password protected)
    public async Task<IActionResult> GetPage([FromQuery] string virtualPath, [FromQuery] string password = "")
    {
        var account = GetAccount();

        Page? page = null;
        if (_pageReader.TryGetPageFromPath(virtualPath, out var pagePath, true, account))
        {
            page = pagePath;
        }
        else if (Guid.TryParse(virtualPath, out var guid))
        {
            if (!_pageReader.TryGetPageFromGuid(guid, out page))
            {
                return NotFound();
            }
        }
        else if (int.TryParse(virtualPath, out var id))
        {
            if (!_pageReader.TryGetPageFromId(id, out page, true, account))
            {
                return NotFound();
            }
        }
        else
        {
            return NotFound();
        }

        if (page.Flags.HasFlag(PageFlagType.PasswordProtected)
            && page.CreatedBy != account?.Id
            ) // Allow creator to view page without password
        {
            if (account != null && account.Roles.HasAnyRole(Role.Admin, Role.Moderator, Role.DatabaseAdmin) && password == "SKIP")
                return Ok(page);

            if (password != page.Flags.GetFlagValue(PageFlagType.PasswordProtected))
                return Unauthorized();
        }

        return Ok(page);
    }

    /// <summary>
    /// Translates a given string rich text to HTML.
    /// Why is this a patch even tho it changes NOTHING on the server?
    /// GET requests CANNOT HAVE A BODY :)))))
    /// </summary>
    [HttpPatch("/page/rich_text")]
    public async Task<IActionResult> RichTextToHtml(
        [FromBody] string? text
        )
    {
        if (string.IsNullOrWhiteSpace(text))
            return BadRequest("Text cannot be empty.");

        var account = GetAccount();

        var parser = new RichTextParser(_authHelper, _authStateProvider);
        var html = await parser.Parse(text, account);
        return Ok(html.ToString());
    }

    private Account? GetAccount()
    {
        Account? account = null;

        if (User.Identity != null)
        {
            var claim = User.Claims
                .FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (claim != null)
            {
                var guid = Guid.Parse(claim);

                account = _context.Accounts
                    .FirstOrDefault(a => a.Id == guid);
            }
        }

        return account;
    }


    /// <summary>
    /// Lists all pages
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> ListPages()
    {
        var pages = await _context.Pages
            .Include(p => p.Flags)
            .Select(page => new
            {
                page.PageGuid,
                page.Title,
                page.Flags,
                page.VirtualPath,
                page.Version
            })
            .ToListAsync();

        // Get latest version of each page
        var latestPages = pages.GroupBy(p => p.VirtualPath)
            .Select(g => g.OrderByDescending(p => p.Version).First())
            .ToList();

        var response = latestPages.Select(page => new PageResponse()
        {
            PageGuid = page.PageGuid,
            Title = page.Title,
            Flags = page.Flags,
            VirtualPath = page.VirtualPath
        });

        // Remove any flags that are not meant to be exposed to the API
        foreach (var page in response)
        {
            foreach (var pageFlag in page.Flags)
            {
                var flagAttribute = pageFlag.Type.GetAttribute<PageFlagAttribute>();

                if (flagAttribute.HideInApi)
                    pageFlag.Value = "";
            }
        }

        return Ok(response);
    }

    public class PageResponse
    {
        public Guid PageGuid { get; set; }
        public string Title { get; set; }
        public HashSet<PageFlag> Flags { get; set; }
        public string VirtualPath { get; set; }
    }
}