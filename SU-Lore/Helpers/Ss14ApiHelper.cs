using System.Text.Json;
using SU_Lore.Data;

namespace SU_Lore.Helpers;

public static class Ss14ApiHelper
{
    public static async Task<Ss14ApiResponse?> FetchAccountFromGuid(Guid guid)
    {
        var client = new HttpClient();
        var response = await client.GetAsync($"https://central.spacestation14.io/auth/api/query/userid?userid={guid}");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Ss14ApiResponse>(content);
    }
}