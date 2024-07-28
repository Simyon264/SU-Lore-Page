using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SU_Lore.Data;
using SU_Lore.Database;

namespace SU_Lore.Controllers;

[Controller]
public class CustomCss : Controller
{
    private ApplicationDbContext _context;
    
    public CustomCss(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("/custom.css")]
    public async Task<IActionResult> GetCustomCss()
    {
        var colors = await _context.Colors.ToListAsync();
        
        colors.RemoveAll(c => c.Hex.Length != 7);
        for (var i = 0; i < colors.Count; i++)
        {
            var color = colors[i];
            if (color.Name.All(c => Constants.AllowedColorNameCharacters.Contains(c))) continue;
            
            Log.Warning("Invalid color name: {ColorName}", color.Name);
            colors.RemoveAt(i);
            i--;
        }
        
        var css = new StringBuilder();
        
        css.AppendLine("/* This file is generated on the fly. ");
        css.AppendLine(" * Do not edit it directly. ");
        // get a custom message from the database
        var message = Constants.CustomCssMessages[new Random().Next(Constants.CustomCssMessages.Length)];
        css.Append($" * {message}");
        css.AppendLine(" */");
        
        css.AppendLine(":root {");
        foreach (var color in colors)
        {
            css.AppendLine($"--fg-{color.Name}: {color.Hex};");
            css.AppendLine($"--glow-{color.Name}: {color.Hex}cc;");
        }
        
        css.AppendLine("}");
        
        foreach (var color in colors)
        {
            css.AppendLine($".color-{color.Name} {{");
            css.AppendLine($"    color: var(--fg-{color.Name});");
            css.AppendLine($"    text-shadow: 0 0 3px var(--glow-{color.Name});");
            css.AppendLine("}");
        }
        
        // Make sure the browser doesn't cache the CSS file
        Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        
        return Content(css.ToString(), "text/css");
    }
}