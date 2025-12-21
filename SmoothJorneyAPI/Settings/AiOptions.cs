namespace SmoothJorneyAPI.Settings
{
    public class AiOptions
    {
        public const string SectionName = "AiOptions";
        public string GroqApiKey { get; set; } = string.Empty;
        public string WeatherApiKey { get; set; } = string.Empty;
    }
}
