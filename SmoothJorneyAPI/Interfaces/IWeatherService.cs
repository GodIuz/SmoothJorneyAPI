using SmoothJorneyAPI.DTO;

namespace SmoothJorneyAPI.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherDataDTO> GetCurrentWeatherAsync(string city);
    }
}
