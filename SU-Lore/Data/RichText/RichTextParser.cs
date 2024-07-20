using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using SU_Lore.Database.Models.Accounts;
using SU_Lore.Helpers;

namespace SU_Lore.Data.RichText;

/// <summary>
/// This parses a string of text into html ready elements.
/// TODO: This should use a library or something, this is NOT a good way to do this.
/// </summary>
public class RichTextParser
{
    private readonly AuthenticationHelper _authenticationHelper;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    
    public RichTextParser(AuthenticationHelper authenticationHelper, AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationHelper = authenticationHelper;
        _authenticationStateProvider = authenticationStateProvider;
    }
    
    public static string GetCurrentDate()
    {
        return DateTime.Now.AddYears(200).ToString("yyyy-MM-dd");
    }
    
    public async Task<MarkupString> Parse(string input)
    {
        var user = await _authenticationHelper.FetchAccount();
        ReplaceStatics(ref input, user);
        ReplaceHeaders(ref input);
        ParseButtonTags(ref input);
        ParseColorTags(ref input);
        ParseBlockTags(ref input);
        ParseMediaTags(ref input);
        
        return new MarkupString(input);
    }

    /// <summary>
    /// Replaces header tags, so [head=1]text[/head] becomes <h1>text</h1>
    /// </summary>
    private void ReplaceHeaders(ref string input)
    {
        int startIndex;
        while ((startIndex = input.IndexOf("[head=", StringComparison.Ordinal)) != -1)
        {
            var endIndex = input.IndexOf("]", startIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                break;

            var level = input[startIndex + 6];
            var closingTagIndex = input.IndexOf("[/head]", endIndex, StringComparison.Ordinal);
            if (closingTagIndex == -1)
                break;
            
            if (!int.TryParse(level.ToString(), out var num))
            {
                throw new Exception("Header level is invalid. It should be between 1 and 6. But it was: " + level);
            }

            // max level is 6
            if (num > 6)
                throw new Exception("Header level is invalid. It should be between 1 and 6. But it was: " + level);
            
            if (num < 1)
                throw new Exception("Header level is invalid. It should be between 1 and 6. But it was: " + level);

            var replacement = $"<h{level}>{input.Substring(endIndex + 1, closingTagIndex - endIndex - 1)}</h{level}>";
            input = input.Remove(startIndex, closingTagIndex - startIndex + 7).Insert(startIndex, replacement);
        }
    }

    /// <summary>
    /// Parses button tags, so [button=link;text] becomes an anchor tag with <a href=\"#{buttonLink}\">{buttonText}</a>
    /// </summary>
    private void ParseButtonTags(ref string input)
    {
        int startIndex;
        while ((startIndex = input.IndexOf("[button=", StringComparison.Ordinal)) != -1)
        {
            var endIndex = input.IndexOf("]", startIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                break;

            var path = input.Substring(startIndex + 8, endIndex - startIndex - 8);
            var split = path.Split(";");
            if (split.Length != 2)
            {
                throw new Exception("Button tag is malformed. It should be in the format [button=link;text]");
            }

            var replacement = $"<a href=\"#{split[0]}\">{split[1]}</a>";
            input = input.Remove(startIndex, endIndex - startIndex + 1).Insert(startIndex, replacement);
        }
    }

    /// <summary>
    /// Parses the [image], [audio], and [video] tags.
    /// </summary>
    private void ParseMediaTags(ref string input)
    {
        int startIndex;
        
        // First the image tag, image tag works like this: [image=filename;alt text;width;height]
        while ((startIndex = input.IndexOf("[image=", StringComparison.Ordinal)) != -1)
        {
            var endIndex = input.IndexOf("]", startIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                break;

            var path = input.Substring(startIndex + 7, endIndex - startIndex - 7);
            var split = path.Split(";");
            if (split.Length != 4)
            {
                throw new Exception("Image tag is malformed. It should be in the format [image=filename;alt text;width;height]");
            }

            var replacement = $"<img src=\"/resources/{split[0]}\" alt=\"{split[1]}\" width=\"{split[2]}\" height=\"{split[3]}\">";
            input = input.Remove(startIndex, endIndex - startIndex + 1).Insert(startIndex, replacement);
        }
        
        // Then the audio tag, audio does not have a closing tag
        while ((startIndex = input.IndexOf("[audio=", StringComparison.Ordinal)) != -1)
        {
            var endIndex = input.IndexOf("]", startIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                break;

            var path = input.Substring(startIndex + 7, endIndex - startIndex - 7);
            var replacement = $"<audio controls><source src=\"/resources/{path}\" type=\"audio/mpeg\"></audio>";
            input = input.Remove(startIndex, endIndex - startIndex + 1).Insert(startIndex, replacement);
        }
        
        // Then the video tag, video does not have a closing tag
        while ((startIndex = input.IndexOf("[video=", StringComparison.Ordinal)) != -1)
        {
            var endIndex = input.IndexOf("]", startIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                break;

            var path = input.Substring(startIndex + 7, endIndex - startIndex - 7);
            var replacement = $"<video controls><source src=\"/resources/{path}\" type=\"video/mp4\"></video>";
            input = input.Remove(startIndex, endIndex - startIndex + 1).Insert(startIndex, replacement);
        }
    }

    private void ReplaceStatics(ref string input, Account? account)
    {
        input = input.Replace("[date]", GetCurrentDate());
        input = input.Replace("[username]", account?.Username ?? "Anonymous");
        
        // Prevent XSS
        input = input.Replace("<", "&lt;");
        input = input.Replace(">", "&gt;");
    }
    
    private void ParseColorTags(ref string input)
    {
        int startIndex;
        while ((startIndex = input.IndexOf("[color=", StringComparison.Ordinal)) != -1)
        {
            var endIndex = input.IndexOf("]", startIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                break;

            var colorName = input.Substring(startIndex + 7, endIndex - startIndex - 7);
            // Make sure the color name is valid (only asci letters, no " or ' or anything)
            if (!Regex.IsMatch(colorName, "^[a-zA-Z]+$"))
            {
                throw new Exception("Color name contains invalid characters. Only ascii letters are allowed. Name was: " + colorName);
            }
            
            var closingTagIndex = input.IndexOf("[/color]", endIndex, StringComparison.Ordinal);
            if (closingTagIndex == -1)
                break;

            var replacement = $"<span class=\"color-{colorName}\">{input.Substring(endIndex + 1, closingTagIndex - endIndex - 1)}</span>";
            input = input.Remove(startIndex, closingTagIndex - startIndex + 8).Insert(startIndex, replacement);
        }
    }
    
    private void ParseBlockTags(ref string input)
    {
        int startIndex;
        while ((startIndex = input.IndexOf("[block=", StringComparison.Ordinal)) != -1)
        {
            var endIndex = input.IndexOf("]", startIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                break;

            var colorName = input.Substring(startIndex + 7, endIndex - startIndex - 7);
            var closingTagIndex = input.IndexOf("[/block]", endIndex, StringComparison.Ordinal);
            if (closingTagIndex == -1)
                break;

            var replacement = $"<p class=\"block block-{colorName}\">{input.Substring(endIndex + 1, closingTagIndex - endIndex - 1)}</p>";
            input = input.Remove(startIndex, closingTagIndex - startIndex + 8).Insert(startIndex, replacement);
        }
    }
}