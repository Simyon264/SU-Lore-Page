using System.Collections.Immutable;
using Serilog;
using SU_Lore.Data.RichText;

namespace SU_Lore.Helpers;

public class RandomQuoteHelper
{
    private const string ApiUrl = "https://eclipse.is-going-to.cyou/static/stories/bubbles-and-sensors/random-quote.php";

    private ImmutableArray<string> _fallbackQuotes = [
        ..new string[]
        {
            "Engineering has loosed another Tesla, please stand by.",
            "David has knocked driven his shittle into the news satellite, please wait.",
            "The earth simulator has failed, we are working on a fix.",
            "Lucy, I know that the coffee machine is broken! Just wait- Oh shit is this on?",
            "A loose singularity is currently eating engineering, please use the backup generators."
        }
    ];

    private HttpClient _httpClient;
    private Random _random;
    private RichTextParser _parser;

    public RandomQuoteHelper(RichTextParser richTextParser)
    {
        _httpClient = new HttpClient();

        _random = new Random();
        _parser = richTextParser;
    }

    public async Task<string> GetRandomQuote()
    {
        try
        {
            var response = _httpClient.GetAsync(ApiUrl).Result;
            response.EnsureSuccessStatusCode();
            var content = response.Content.ReadAsStringAsync().Result;

            // ensure the thing we got back is valid rich text
            try
            {
                _ = await _parser.Parse(content);
            }
            catch (Exception e)
            {
                Log.Error(e, "Recieved invalid rich text from qoute API.");
                return GetFallBack();
            }

            return content;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get random quote.");
            return GetFallBack();
        }
    }

    private string GetFallBack()
    {
        return _fallbackQuotes[_random.Next(0, _fallbackQuotes.Length)];
    }
}