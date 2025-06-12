
using System.Text.Json;
using NotesAPI.DTO;

namespace NotesAPI.Helpers
{

    public static class GeoLocationHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<LocationInfo> GetLocationFromIpAsync(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip) || ip == "::1" || ip.StartsWith("127.") || ip.StartsWith("192.168.") || ip.StartsWith("10."))
            {
                return new LocationInfo
                {
                    City = "Localhost",
                    Country = "Local Network",
                    RegionName = "N/A",
                    Query = ip ?? "Unknown",
                };
            }

            try
            {
                var url = $"http://ip-api.com/json/{ip}";
                var response = await _httpClient.GetFromJsonAsync<LocationInfo>(url);
                return response ?? new LocationInfo { City = "Unknown", Country = "Unknown", Query = ip };
            }
            catch
            {
                return new LocationInfo { City = "Error", Country = "Error", Query = ip };
            }
        }

        public static async Task<string> GetLocationFromLatLngNominatimAsync(double latitude, double longitude)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=10&addressdetails=1";

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("YourAppName/1.0");

        try
        {
            var response = await httpClient.GetStringAsync(url);
            using var jsonDoc = JsonDocument.Parse(response);

            if (jsonDoc.RootElement.TryGetProperty("display_name", out var displayName))
            {
                return displayName.GetString() ?? "Unknown Location";
            }
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"HTTP error: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return "Unknown Location";
    }
    }
}