using System.Text;
using Microsoft.AspNetCore.Components;
using SU_Lore.Database.Models.Pages;

namespace SU_Lore.Helpers.Animation;

/// <summary>
/// Animation that fades in the content by gradually revealing more letters in a random order.
/// </summary>
public class FadeInAnimation : Animation
{
    private const int CharactersPerFrame = 1;

    /// <inheritdoc />
    public override void Start(Page page, Action<string> setContent, TaskCompletionSource<bool> tcs)
    {
        var contentString = page.Content;
        var content = contentString.ToCharArray();
        var visibleCharacters = new bool[content.Length];

        var random = new Random();
        var charactersToReveal = content.Length;
        var charactersRevealed = 0;

        while (charactersRevealed < charactersToReveal)
        {
            var charactersThisFrame = Math.Min(CharactersPerFrame, charactersToReveal - charactersRevealed);
            for (var i = 0; i < charactersThisFrame; i++)
            {
                int index;
                do
                {
                    index = random.Next(content.Length);
                } while (visibleCharacters[index]);

                visibleCharacters[index] = true;
                charactersRevealed++;
            }

            var newContent = new StringBuilder();
            for (var i = 0; i < content.Length; i++)
            {
                newContent.Append(visibleCharacters[i] ? content[i] : ' ');
            }

            setContent(newContent.ToString());
        }

        tcs.SetResult(true);
    }
}