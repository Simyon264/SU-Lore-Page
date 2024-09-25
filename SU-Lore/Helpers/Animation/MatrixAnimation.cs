using System.Text;
using SU_Lore.Database.Models.Pages;

namespace SU_Lore.Helpers.Animation;

/// <summary>
/// Animation that first displays a full random set of characters, then gradually reveals the content by removing characters.
/// </summary>
public class MatrixAnimation : Animation
{
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}~";
    private const int Delay = 5; // how long to wait between each character reveal

    /// <inheritdoc />
    public override void Start(Page page, Action<string> setContent, TaskCompletionSource<bool> tcs)
    {
        var pageContent = page.Content;

        var random = new Random();
        var sb = new StringBuilder();
        for (var i = 0; i < pageContent.Length; i++)
        {
            sb.Append(Characters[random.Next(Characters.Length)]);
        }

        var randomContent = sb.ToString();
        setContent(randomContent);

        // Gradually reveal the content in random order
        var characters = randomContent.ToCharArray();
        var revealed = new bool[characters.Length];
        var revealedCount = 0;
        Task.Run(async () =>
        {
            while (revealedCount < characters.Length)
            {
                var index = random.Next(characters.Length);
                if (revealed[index])
                {
                    continue;
                }

                revealed[index] = true;
                characters[index] = pageContent[index];
                setContent(new string(characters));
                revealedCount++;
                await Task.Delay(Delay);
            }

            tcs.SetResult(true);
        });
    }
}