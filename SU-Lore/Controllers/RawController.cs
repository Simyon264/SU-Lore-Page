using Microsoft.AspNetCore.Mvc;
using SU_Lore.Helpers;

namespace SU_Lore.Controllers;

/// <summary>
/// Returns the raw content of a page without any formatting.
/// </summary>
[Route("raw")]
public class RawController : Controller
{
    private readonly PageReader _pageService;

    public RawController(PageReader pageService)
    {
        _pageService = pageService;
    }

    /// <summary>
    /// Returns the raw content of a page without any formatting.
    /// </summary>
    /// <returns>The raw content of the page.</returns>
    [HttpGet]
    public async Task<IActionResult> GetRawContent(
        [FromQuery] int page
        )
    {
        if (!_pageService.TryGetPageFromId(page, out var pageFetched))
        {
            return NotFound();
        }

        return Content(pageFetched.Content, "text/plain");
    }
}