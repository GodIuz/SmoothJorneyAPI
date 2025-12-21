using Microsoft.Extensions.Options;
using SmoothJorneyAPI.DTO;
using SmoothJorneyAPI.Interfaces;
using SmoothJorneyAPI.Settings;
using System.Text.Json;

namespace SmoothJorneyAPI.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService(HttpClient httpClient, IOptions<AiOptions> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.WeatherApiKey;
        }

        public async Task<WeatherDataDTO> GetCurrentWeatherAsync(string city)
        {
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_WEATHER_KEY")
            {
                return new WeatherDataDTO { Description = "Sunny (Default)", Temperature = 22 };
            }

            try
            {
                var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return new WeatherDataDTO { Description = "Clear Sky", Temperature = 20 };
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                var description = root.GetProperty("weather")[0].GetProperty("description").GetString();
                var temp = root.GetProperty("main").GetProperty("temp").GetDouble();

                return new WeatherDataDTO
                {
                    Description = description ?? "Sunny",
                    Temperature = temp
                };
            }
            catch
            {
                return new WeatherDataDTO { Description = "Sunny", Temperature = 25 };
            }
        }

    }
}
