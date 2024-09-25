using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SU_Lore.Data;
using SU_Lore.Database;
using SU_Lore.Database.Models;
using SU_Lore.Database.Models.Accounts;
using SU_Lore.Database.Models.Pages;

namespace SU_Lore.Helpers;

/// <summary>
/// Handles fetching pages from the database. It also has methods for rendering pages i.e. rich text.
/// </summary>
public class PageReader
{
    private readonly ApplicationDbContext _context;
    private readonly AuthenticationHelper _authHelper;
    private readonly NavigationManager _navigationManager;
    private readonly RandomQuoteHelper _quoteHelper;
    private readonly IMemoryCache _cache;

    public PageReader(ApplicationDbContext context, AuthenticationHelper authHelper, NavigationManager navigationManager, IMemoryCache cache, RandomQuoteHelper quoteHelper)
    {
        _context = context;
        _authHelper = authHelper;
        _navigationManager = navigationManager;
        _cache = cache;
        _quoteHelper = quoteHelper;
    }

    public bool IsPathValid(string path)
    {
        return !string.IsNullOrWhiteSpace(path) && (path.StartsWith("/system/") || GetPageFromPath(path) != null);
    }

    /// <summary>
    /// Returns a list of pages based on their GUID. They are sorted by version number.
    /// </summary>
    public bool TryGetPagesFromGuid(Guid guid, [NotNullWhen(true)] out List<Page>? page, bool collectStats = true)
    {
        var pages = _context.Pages
            .Include(p => p.Flags)
            .Where(p => p.PageGuid == guid)
            .OrderByDescending(p => p.Version)
            .ToList();

        page = pages;

        // Page stats
        var account = _authHelper.FetchAccount().Result;
        if (page.Count > 0 && collectStats)
        {
            var pageStats = _context.PageStats.FirstOrDefault(p => p.PageId == pages![0].PageGuid);
            var uniqueAccountPageKey = account?.Id.ToString() + page[0].PageGuid.ToString();
            if (pageStats != null)
            {
                // If the memory cache contains the key, the user has already viewed the page
                if (!_cache.TryGetValue(uniqueAccountPageKey, out _))
                {
                    // If the user has not viewed the page, we add the key to the cache
                    _cache.Set(uniqueAccountPageKey, true, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1)
                    });
                    pageStats.Views++;
                    _context.SaveChanges();
                }
            } else {
                // If the page stats are null, we create a new entry
                pageStats = new PageStat()
                {
                    PageId = page[0].PageGuid,
                    Views = 1
                };
                _context.PageStats.Add(pageStats);
                _context.SaveChanges();
            }
        }

        return page.Count > 0;
    }

    /// <summary>
    /// Retrieves a page from the database based on its virtual path. System pages are not stored in the DB and are handled separately.
    /// </summary>
    /// <param name="path">The virtual path of the page.</param>
    /// <param name="page">The page if found, null otherwise.</param>
    /// <returns>True if the page was found, false otherwise.</returns>
    public bool TryGetPageFromPath(string path, [NotNullWhen(true)] out Page? page, bool collectStats = true)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            page = null;
            return false;
        }

        if (path.StartsWith("/system/link/"))
        {
            // Get everything after the /system/link/ part
            path = path[(path.IndexOf("/system/link/") + "/system/link/".Length)..];
            _navigationManager.NavigateTo(path);
            page = null;
            return false;
        }
        if (path.StartsWith("/system/"))
        {
            page = GetSystemPage(path);
            return true;
        }

        page = GetPageFromPath(path, collectStats);
        return page != null;
    }

    private Page? GetPageFromPath(string path, bool collectStats = true)
    {
        var page = _context.Pages
            .Include(p => p.Flags)
            .OrderByDescending(p => p.Version)
            .FirstOrDefault(p => p.VirtualPath == path);

        var account = _authHelper.FetchAccount().Result;

        if (page != null && collectStats)
        {
            var pageStats = _context.PageStats.FirstOrDefault(p => p.PageId == page!.PageGuid);
            var uniqueAccountPageKey = account?.Id.ToString() + page.PageGuid.ToString();
            if (pageStats != null)
            {
                // If the memory cache contains the key, the user has already viewed the page
                if (!_cache.TryGetValue(uniqueAccountPageKey, out _))
                {
                    // If the user has not viewed the page, we add the key to the cache
                    _cache.Set(uniqueAccountPageKey, true, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1)
                    });
                    pageStats.Views++;
                    _context.SaveChanges();
                }
            } else {
                // If the page stats are null, we create a new entry
                pageStats = new PageStat()
                {
                    PageId = page.PageGuid,
                    Views = 1
                };
                _context.PageStats.Add(pageStats);
                _context.SaveChanges();
            }
        }


        return page;
    }

    public bool TryGetPageFromId(int pageId, [NotNullWhen(true)] out Page? o, bool collectStats = true)
    {
        var page = _context.Pages
            .Include(p => p.Flags)
            .FirstOrDefault(p => p.Id == pageId);

        o = page;

        if (!collectStats)
        {
            return o != null;
        }

        // Page stats
        var account = _authHelper.FetchAccount().Result;
        if (o != null && collectStats)
        {
            var pageStats = _context.PageStats.FirstOrDefault(p => p.PageId == page!.PageGuid);
            var uniqueAccountPageKey = account?.Id.ToString() + o.PageGuid.ToString();
            if (pageStats != null)
            {
                // If the memory cache contains the key, the user has already viewed the page
                if (!_cache.TryGetValue(uniqueAccountPageKey, out _))
                {
                    // If the user has not viewed the page, we add the key to the cache
                    _cache.Set(uniqueAccountPageKey, true, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1)
                    });
                    pageStats.Views++;
                    _context.SaveChanges();
                }
            } else {
                // If the page stats are null, we create a new entry
                pageStats = new PageStat()
                {
                    PageId = o.PageGuid,
                    Views = 1
                };
                _context.PageStats.Add(pageStats);
                _context.SaveChanges();
            }
        }

        return o != null;
    }

    private Page GetSystemPage(string path)
    {
        switch (path)
        {
            case "/system/link/account/login":
                _navigationManager.NavigateTo("/account/login");
                return null;
            case "/system/syndicate-listing":
                return new Page()
                {
                    Id = -8,
                    Content = GenerateFileListing(true).Result,
                    Flags = new HashSet<PageFlag>()
                    {
                        new PageFlag()
                        {
                            Id = -2,
                            Type = PageFlagType.HideBackButton,
                            Value = ""
                        },
                    },
                    Title = "Syndicate listing",
                    Version = 0,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    VirtualPath = "/system/syndicate-listing",
                    CreatedBy = Guid.Empty,
                    UpdatedBy = Guid.Empty,
                    PageGuid = Guid.Empty
                };
            case "/system/emag":
                return new Page()
                {
                    Id = -7,
                    Content = Emag,
                    Flags = new HashSet<PageFlag>()
                    {
                        new PageFlag()
                        {
                            Id = -3,
                            Type = PageFlagType.EnterAnimation,
                            Value = "MatrixAnimation"
                        },
                        new PageFlag()
                        {
                            Id = -4,
                            Type = PageFlagType.HideBackButton,
                            Value = ""
                        },
                        new PageFlag()
                        {
                            Id = -5,
                            Type = PageFlagType.Redirect,
                            Value = "/system/syndicate-listing"
                        }
                    },
                    Title = "ERROR #ca218c",
                    Version = 0,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    VirtualPath = "/system/emag",
                    CreatedBy = Guid.Empty,
                    UpdatedBy = Guid.Empty,
                    PageGuid = Guid.Empty
                };
            case "/system/debug/animation":
                return new Page()
                {
                    Id = -6,
                    Content = "This is a test animation page.",
                    Flags = new HashSet<PageFlag>()
                    {
                        new PageFlag()
                        {
                            Id = -3,
                            Type = PageFlagType.EnterAnimation,
                            Value = "MatrixAnimation"
                        }
                    },
                    Title = "Animation test",
                    Version = 0,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    VirtualPath = "/system/debug/animation",
                    CreatedBy = Guid.Empty,
                    UpdatedBy = Guid.Empty,
                    PageGuid = Guid.Empty
                };
            case "/system/overview":
                return new Page()
                {
                    Id = -5,
                    Content = GenerateCrewOverview(),
                    Flags = new HashSet<PageFlag>(),
                    Title = "Crew overview",
                    Version = 0,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    VirtualPath = "/system/overview",
                    CreatedBy = Guid.Empty,
                    UpdatedBy = Guid.Empty,
                    PageGuid = Guid.Empty
                };
            case "/system/listing":
                return new Page()
                {
                    Id = -4,
                    Content = GenerateFileListing(false).Result,
                    Flags = new HashSet<PageFlag>()
                    {
                        new PageFlag()
                        {
                            Id = -2,
                            Type = PageFlagType.HideBackButton,
                            Value = ""
                        },
                    },
                    Title = "File listing",
                    Version = 0,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    VirtualPath = "/system/listing",
                    CreatedBy = Guid.Empty,
                    UpdatedBy = Guid.Empty,
                    PageGuid = Guid.Empty
                };
            case "/system/boot":
                return new Page()
                {
                    Id = -3,
                    Content = Boot,
                    Flags = new HashSet<PageFlag>()
                    {
                        new PageFlag()
                        {
                            Id = -1,
                            Type = PageFlagType.HideTitle,
                            Value = ""
                        },
                        new PageFlag()
                        {
                            Id = -2,
                            Type = PageFlagType.HideBackButton,
                            Value = ""
                        },
                    },
                    Title = "Startup",
                    Version = 0,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    VirtualPath = "/system/startup",
                    CreatedBy = Guid.Empty,
                    UpdatedBy = Guid.Empty,
                    PageGuid = Guid.Empty
                };
                break;
            case "/system/startup":
                return new Page()
                {
                    Id = -2,
                    Content = Startup,
                    Flags = new HashSet<PageFlag>()
                    {
                        new PageFlag()
                        {
                            Id = -1,
                            Type = PageFlagType.HideTitle,
                            Value = ""
                        },
                        new PageFlag()
                        {
                            Id = -2,
                            Type = PageFlagType.HideBackButton,
                            Value = ""
                        },
                        new PageFlag()
                        {
                            Id = -3,
                            Type = PageFlagType.ScrollToBottom,
                            Value = ""
                        }
                    },
                    Title = "Startup",
                    Version = 0,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    VirtualPath = "/system/startup",
                    CreatedBy = Guid.Empty,
                    UpdatedBy = Guid.Empty,
                    PageGuid = Guid.Empty
                };
                break;
            case "/system/notfound":
            default:
                return new Page()
                {
                    Id = -1,
                    Content = NotFound,
                    Flags = [],
                    Title = "Not found",
                    Version = 0,
                    CreatedAt = DateTime.MinValue,
                    UpdatedAt = DateTime.MinValue,
                    VirtualPath = "/system/notfound",
                    CreatedBy = Guid.Empty,
                    UpdatedBy = Guid.Empty,
                    PageGuid = Guid.Empty
                };
                break;
        }
    }

    private string GenerateCrewOverview()
    {
        var sb = new StringBuilder();

        sb.AppendLine("[color=warning][block=warning]Warning");
        sb.AppendLine("You only have access to a limited overview of the crew. Some information may be hidden or inaccessible.");
        sb.AppendLine("[/block][/color]");
        sb.AppendLine();
        sb.AppendLine("[color=white]Crew overview:[color=sys][speed=0]");
        sb.AppendLine("--- BEGIN ---[speed=default]");
        sb.AppendLine();

        var accounts = _context.Accounts.ToList();
        // Sort the accounts by who has the highest "clearance levels", so highest role enum value (so lowest number) first
        // account has a list of roles, we get the highest role from that list
        var sortedAccounts = accounts.OrderBy(a => a.Roles.Min(r => r)).ToList();
        foreach (var account in sortedAccounts)
        {
            // roles are also sorted by enum value
            var sortedRoles = account.Roles.OrderBy(r => r).ToList();
            var roles = string.Join(", ", sortedRoles.Select(r => r.ToString()));

            sb.AppendLine($"[color=white]{account.Username}[/color]");
            sb.AppendLine($"[color=info]Roles: {roles}[/color]");
            sb.AppendLine();
        }

        sb.AppendLine("[speed=0]--- END ---[speed=default]");
        sb.AppendLine("[/color][color=warn][block=warning]Warning:");
        sb.AppendLine("This overview is classified. Misuse is punishable by law and may be prosecuted as treason.");
        sb.AppendLine("Do not attempt to access information you are not authorized to view.");
        sb.AppendLine("[/block][/color][/color]");

        return sb.ToString();
    }

    private async Task<string> GenerateFileListing(bool syndicate)
    {
        var sb = new StringBuilder();

        if (syndicate)
        {
            sb.AppendLine("[color=info][block=info]Syndicate access granted. Welcome, agent.[/block][/color]");
            sb.AppendLine("[button=/system/listing;RETURN TO NORMAL NET]");
        }

        var account = await _authHelper.FetchAccount();
        if (account == null)
        {
            // We append an error block
            sb.AppendLine("[color=red][block=error]Warning:");
            sb.AppendLine("Logged in as anonymous user. Some files may be hidden or inaccessible.");
            sb.AppendLine("[/block][/color]");
            // button to login
            sb.AppendLine("[button=/system/link/account/login;LOGIN]");
            sb.AppendLine();
        }
        else
        {
            if (account.Roles.HasRole(Role.Whitelisted))
            {
                sb.AppendLine("[color=info][block=info]ID card recognized. Welcome [username].[/block][/color]");
                sb.AppendLine("[button=/system/link/account/logout;EJECT ID]");
            } else {
                sb.AppendLine("[color=info][block=info]ID card recognized. Welcome [username].");
                sb.AppendLine("[color=red]You are not authorized to create or edit content, please contact Simyon for access.[/color]");
                sb.AppendLine("[/block][/color]");
            }

            sb.AppendLine();
        }

        sb.AppendLine("[color=white]Available files:[color=sys][speed=0]");
        sb.AppendLine("--- BEGIN ---[speed=default]");
        sb.AppendLine();

        var pages = _context.Pages
            .Select(p => p.PageGuid)
            .Distinct()
            .ToList();

        if (pages.Count > 0 && !syndicate)
        {
            // Get the 3 most recent changes
            var recentChanges = _context.Database.SqlQueryRaw<Guid>(
                    "SELECT \"PageGuid\" FROM( SELECT *, ROW_NUMBER() OVER (PARTITION BY \"PageGuid\" ORDER BY \"Id\" DESC) AS rn FROM \"Pages\") AS sub WHERE rn = 1 ORDER BY \"Id\" DESC LIMIT 3; ")
                .ToList();

            var recentChangesPages = new List<Page>();
            foreach (var recentChange in recentChanges)
            {
                if (TryGetPagesFromGuid(recentChange, out var page, false))
                {
                    recentChangesPages.Add(page.First());
                }
            }

            recentChangesPages = recentChangesPages.OrderByDescending(p => p.UpdatedAt).ToList();

            sb.AppendLine("[color=info]Recent changes:[/color]");
            foreach (var page in recentChangesPages)
            {
                if (page.Flags.HasFlag(PageFlagType.Unlisted))
                {
                    if (account == null)
                    {
                        continue;
                    }

                    if (page.CreatedBy != account.Id && !account.Roles.HasAnyRole(Role.Admin, Role.Moderator, Role.DatabaseAdmin))
                    {
                        // not the creator
                        continue;
                    }
                }

                sb.AppendLine($"[color=info]{page.Title}[/color] - {page.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"[button={page.VirtualPath};VIEW]");
            }
        }

        sb.AppendLine();
        sb.AppendLine("[color=info]All files:[/color]");

        // Get the latest version of each page
        var files = new List<Page>();
        foreach (var guid in pages)
        {
            if (TryGetPagesFromGuid(guid, out var page, false))
            {
                var item = page.First();
                if (item.Flags.HasFlag(PageFlagType.Unlisted))
                {
                    if (account == null)
                    {
                        continue;
                    }

                    if (item.CreatedBy != account.Id && !account.Roles.HasAnyRole(Role.Admin, Role.Moderator, Role.DatabaseAdmin))
                    {
                        // not the creator
                        continue;
                    }
                }

                if (item.Flags.HasFlag(PageFlagType.Syndicate) && !syndicate)
                {
                    continue; // skip syndicate files
                } else if (!item.Flags.HasFlag(PageFlagType.Syndicate) && syndicate)
                {
                    continue; // skip non-syndicate files
                }

                files.Add(item);
            }
        }

        var tree = TreeNode.BuildTree(files);
        TreeNode.GetTree(tree, ref sb);

        sb.AppendLine();
        sb.AppendLine("[speed=0]--- END ---[speed=default]");
        sb.AppendLine("[/color][color=warn][block=warning]Warning:");
        sb.AppendLine("These files are classified. Misuse is punishable by law and may be prosecuted as treason.");
        sb.AppendLine("Do not attempt to access files you are not authorized to view.");
        sb.AppendLine("[/block][/color][/color]");

        // Button controls
        sb.AppendLine("[button=/system/link/editor;NEW ENTRY]");
        sb.AppendLine("[button=/system/overview;CREW OVERVIEW]");
        sb.AppendLine("[button=/system/link/files;FILE SYSTEM]");
        sb.AppendLine("[button=/system/link/colors;COLOR PALETTES]");
        if (!syndicate)
        {
            sb.AppendLine("[button=/system/emag;INSERT EMAG]");
        }

        // Random quote
        sb.AppendLine();
        sb.AppendLine("[color=info]Space news feed:[/color]");
        sb.AppendLine(await _quoteHelper.GetRandomQuote());
        sb.AppendLine(
            "[color=info]NOTE: This is random data from local background communications. It may not be accurate, relevant or up to date.[/color]");

        return sb.ToString();
    }

    // Yes, I know this is a mess. I'm sorry. Actually, I'm not.

    private const string NotFound =
"""
[color=red]ERROR - UNABLE TO FIND REQUESTED DOCUMENT, IF YOU BELIEVE THIS IS AN ERROR, PLEASE CONTACT ENGINEERING FOR ASSISTANCE.[/color]
""";

    private const string Boot =
"""
[button=/system/startup;Start boot]
[button=/system/listing;Go to listing]
""";

    private const string Emag =
"""
#############################################
#                                           #
#            NET   SWITCHED                 #
#            ACCESS GRANTED                 #
#            WELCOME  AGENT                 #
#                                           #
#############################################
""";

    private const string Startup =
"""
Please wait...
[color=sys][speed=0]-----------------------------------------------------
| CPU Type       : NT 420 CPU at 3600 Mhz           |
| Memory test    : 3258316K OK                      |
|                                                   |
| BIOS           : NT V3.6 (c)                      |
-----------------------------------------------------[speed=default]
Initializing devices on port 1-4[speed=250]...[speed=default]
Initialized devices.

Checking for bootable media[speed=250]...[speed=default]
Bootable media found.
Booting from HDD-NT-429[speed=250]...[speed=default]

Loading kernel[speed=250]...[speed=default]
Mounting root filesystem[speed=250]...[speed=default]
Starting services[speed=250]...[speed=default]
Starting graphical interface[speed=250]...[speed=default]
Starting audio interface[speed=250]...[speed=default]
[speed=0]
  _   _                _______                       
 | \ | |              |__   __|                      (c)
 |  \| | __ _ _ __   ___ | |_ __ __ _ ___  ___ _ __  
 | . ` |/ _` | '_ \ / _ \| | '__/ _` / __|/ _ \ '_ \ 
 | |\  | (_| | | | | (_) | | | | (_| \__ \  __/ | | |
 |_| \_|\__,_|_| |_|\___/|_|_|  \__,_|___/\___|_| |_|
_____________________________________________________
    Copyright [date]. All rights Reserved.
[speed=default]

Reading ID card[speed=250]...[speed=default]
Loading user profile[speed=250]...[speed=default]
Welcome, [username]
[button=/system/listing;Open file listing]
[/color]
""";
}