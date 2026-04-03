using Azure.Core;
using Microsoft.Extensions.Options;
using SmoothJorneyAPI.DTO;
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


        public async Task<string> GetDetailedTripPlanAsync(MoodTripRequestDTO req, string businessContext)
        {
            int totalDays = (req.EndDate - req.StartDate).Days + 1;
            if (totalDays <= 0) totalDays = 1;

            var systemPrompt = $@"
        Είσαι ένας κορυφαίος ταξιδιωτικός πράκτορας. 
        Πρέπει να φτιάξεις ένα ταξιδιωτικό πλάνο για την πόλη {req.City} με διάθεση '{req.Mood}'.
        Έχεις στη διάθεσή σου τις εξής επιχειρήσεις από τη βάση: {businessContext}

        ΟΔΗΓΙΕΣ:
        1. Το ταξίδι διαρκεί {totalDays} ημέρες. Πρέπει να δημιουργήσεις ΑΚΡΙΒΩΣ {totalDays} ημέρες στο πρόγραμμα.
        2. Κάθε ημέρα να έχει 2-3 δραστηριότητες.
        3. ΛΟΓΙΣΤΙΚΑ ΞΕΝΟΔΟΧΕΙΟΥ: Στην Ημέρα 1, η πρώτη δραστηριότητα ΠΡΕΠΕΙ να είναι το 'Check-in στο Κατάλυμα'. Στην Ημέρα {totalDays} (την τελευταία), η τελευταία δραστηριότητα ΠΡΕΠΕΙ να είναι το 'Check-out & Αναχώρηση'.
        4. Επίστρεψε ΜΟΝΟ ΕΓΚΥΡΟ JSON.
        
        ΔΟΜΗ JSON ΠΟΥ ΠΡΕΠΕΙ ΝΑ ΑΚΟΛΟΥΘΗΣΕΙΣ:
        {{
            ""days"": [
            {{
                ""day"": 1,
                ""activities"": [
                {{ 
                    ""title"": ""Όνομα δραστηριότητας"", 
                    ""time"": ""10:00"", 
                    ""duration"": ""2 ώρες"", 
                    ""description"": ""Περιγραφή"" 
                }}
                ]
            }}
            ]
        }}";

            var userPrompt = $@"Σχεδιάστε ένα ταξίδι {req.Mood} στην {req.City}. 
                        Προϋπολογισμός: {req.TotalBudget}€ για {req.NumberOfPeople} άτομα.
                        Ημερομηνίες: {req.StartDate:dd/MM/yyyy} έως {req.EndDate:dd/MM/yyyy}.
                        Θέλω ακριβώς {totalDays} ημέρες. Απάντησε αποκλειστικά σε μορφή JSON.";
            return await GenerateTextAsync(systemPrompt, userPrompt);
        }

        public async Task<string> GenerateTextAsync(string systemPrompt, string userPrompt)
        {
            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = userPrompt }
        },
                temperature = 0.3,
                response_format = new { type = "json_object" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq API Error [{response.StatusCode}]: {errorBody}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";
        }

        public async Task<string> SummarizeReviewsAsync(IEnumerable<string> reviews)
        {
            if (!reviews.Any()) return "No reviews available.";

            var reviewsText = string.Join("\n- ", reviews.Take(15));

            var systemPrompt = "Είστε ένας χρήσιμος βοηθός που συνοψίζει τις κριτικές πελατών.";
            var userPrompt = $@"
            Ακολουθούν κριτικές χρηστών για μια επιχείρηση:
            {reviewsText}
            
            Παρακαλώ γράψτε μια σύντομη περίληψη (μέγιστο 3 γραμμές) που να αποτυπώνει το γενικό συναίσθημα, τα πλεονεκτήματα και τα μειονεκτήματα.";

            return await CallLlmAsync(systemPrompt, userPrompt);
        }

        private async Task<string> CallLlmAsync(string system, string user)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "Error: Λείπει το API Key";

            var requestBody = new
            {
                model = "llama-3.3-70b-versatile",
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

    }
}