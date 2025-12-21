using Microsoft.Extensions.Options;
using SmoothJorneyAPI.Interfaces;
using SmoothJorneyAPI.Settings;
using System.Text;
using System.Text.Json;

namespace SmoothJorneyAPI.Services
{
    public class GroqAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GroqAiService(HttpClient httpClient, IOptions<AiOptions> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.GroqApiKey;
        }

        // --- 1. TRIP PLANNER ---
        public async Task<string> GetTripPlanAsync(string city, int days, decimal budget, string mood, string weather, DateTime startDate)
        {
            // Υπολογισμός Εποχής
            var season = startDate.Month switch
            {
                12 or 1 or 2 => "Winter",
                3 or 4 or 5 => "Spring",
                6 or 7 or 8 => "Summer",
                _ => "Autumn"
            };

            // Οδηγία Συστήματος (Strict JSON)
            var systemPrompt = @"
            You are an expert travel planner. 
            Create a detailed itinerary strictly in JSON format.
            DO NOT include markdown formatting (like ```json). Just return the raw JSON string.
            Structure: { ""days"": [ { ""day"": 1, ""activities"": [ { ""time"": ""10:00"", ""title"": ""Activity Name"", ""description"": ""Short detail"", ""estimatedCost"": 20.0 } ] } ] }";

            // Το αίτημα του χρήστη
            var userPrompt = $@"
            Plan a {days}-day trip to {city}.
            CONTEXT:
            - Date: {startDate:yyyy-MM-dd} ({season}).
            - Weather forecast: {weather}. (If raining, prefer indoor activities like museums).
            - User Mood: {mood}.
            - Budget: {budget} EUR total.
            
            REQUIREMENTS:
            - Provide estimated cost for each activity.
            - Ensure total cost stays under {budget}.
            - Output ONLY valid JSON.";

            var response = await CallLlmAsync(systemPrompt, userPrompt);
            return CleanJson(response);
        }

        public async Task<string> SummarizeReviewsAsync(IEnumerable<string> reviews)
        {
            if (!reviews.Any()) return "No reviews available.";

            var reviewsText = string.Join("\n- ", reviews.Take(15));

            var systemPrompt = "You are a helpful assistant that summarizes customer reviews.";
            var userPrompt = $@"
            Here are user reviews for a business:
            {reviewsText}
            
            Please write a short summary (max 3 lines) capturing the general sentiment, pros, and cons.";

            return await CallLlmAsync(systemPrompt, userPrompt);
        }
        private async Task<string> CallLlmAsync(string system, string user)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "Error: API Key missing.";

            var requestBody = new
            {
                model = "llama3-8b-8192",
                messages = new[]
                {
                    new { role = "system", content = system },
                    new { role = "user", content = user }
                },
                temperature = 0.5,
                max_tokens = 2500
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {_apiKey}");

            try
            {
                var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"Error from AI: {response.StatusCode} - {error}";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
            }
            catch (Exception ex)
            {
                return $"Exception calling AI: {ex.Message}";
            }
        }

        private string CleanJson(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "{}";
            return raw.Replace("```json", "").Replace("```", "").Trim();
        }
    }
}
