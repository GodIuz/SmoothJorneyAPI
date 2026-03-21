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
            var systemPrompt = $@"
                        Είσαι ένας επαγγελματίας ταξιδιωτικός πράκτορας. 
                        Ο χρήστης θέλει να ταξιδέψει στην πόλη {req.City} με διάθεση '{req.Mood}'.
                        Το ταξίδι θα διαρκέσει ΑΚΡΙΒΩΣ {req.Days} ΗΜΕΡΕΣ.

                        Έχεις στη διάθεσή σου τις εξής επιχειρήσεις από τη βάση δεδομένων:
                        {businessContext}

                        ΟΔΗΓΙΕΣ:
                        1. Πρέπει ΥΠΟΧΡΕΩΤΙΚΑ να επιστρέψεις ένα πλάνο που να καλύπτει ΚΑΙ ΤΙΣ {req.Days} ΗΜΕΡΕΣ. ΜΗΝ σταματήσεις στην 1η ημέρα.
                        2. Ο πίνακας 'days' στο JSON ΠΡΕΠΕΙ να έχει ακριβώς {req.Days} αντικείμενα.
                        3. Κάθε ημέρα πρέπει να έχει τουλάχιστον 2-3 δραστηριότητες.
                        4. Αν δεν φτάνουν οι επιχειρήσεις που σου έδωσα για όλες τις μέρες, συμπλήρωσε το πρόγραμμα με γενικές δραστηριότητες (π.χ. 'Βόλτα στο κέντρο', 'Χαλάρωση στο πάρκο').

                        Επίστρεψε ΜΟΝΟ το JSON σε αυτή τη μορφή (χωρίς markdown, χωρίς έξτρα κείμενο):
                        {{
                          ""days"": [
                            {{
                              ""day"": 1,
                              ""activities"": [
                                {{ ""title"": ""Όνομα επιχείρησης ή δραστηριότητας"", ""time"": ""10:00"", ""description"": ""Περιγραφή..."" }}
                              ]
                            }},
                            // ... ΠΡΕΠΕΙ να συνεχίσεις για τη μέρα 2, 3 κ.ο.κ μέχρι τη μέρα {req.Days}
                          ]
                        }}
                        ";

            var userPrompt = $@"Σχεδιάστε ένα ταξίδι {req.Mood} στην {req.City}. 
                                Προϋπολογισμός: {req.TotalBudget}€ συνολικά για {req.NumberOfPeople} άτομα.
                                Ημερομηνίες: {req.StartDate:dd/MM/yyyy} έως {req.EndDate:dd/MM/yyyy}.
                                Δημιουργήστε ακριβώς {totalDays} ημέρες δραστηριοτήτων.";

            var response = await CallLlmAsync(systemPrompt, userPrompt);
            return CleanJson(response);
        }

        public async Task<string> GenerateTextAsync(string prompt)
        {
            var requestBody = new
            {
                model = "mixtral-8x7b-32768",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.5
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Δεν ήταν δυνατή η ανάλυση.";
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

        private string CleanJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "{}";

            int startIndex = raw.IndexOf('{');
            int endIndex = raw.LastIndexOf('}');

            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                return raw.Substring(startIndex, (endIndex - startIndex) + 1);
            }
            return raw.Trim();
        }
    }
}