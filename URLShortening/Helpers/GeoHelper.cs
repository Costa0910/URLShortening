using System.Text.Json;
using URLShortening.Models;

namespace URLShortening.Helpers;

public class GeoHelper() : IGeoHelper
{
    public async Task<LocationModel> GetCountryAsync(string ipAddress)
    {
        try
        {
            var httpClient = new HttpClient();

            var url = $"https://ipwho.is/{ipAddress}";
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var location = JsonSerializer.Deserialize<LocationModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return location;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching country: {ex.Message}");
        }

        return null;
    }
}
