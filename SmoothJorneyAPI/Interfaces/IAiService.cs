using SmoothJorneyAPI.DTO;

namespace SmoothJorneyAPI.Interfaces
{
    public interface IAiService
    {
        Task<string> SummarizeReviewsAsync(IEnumerable<string> reviews);

        Task<string> GetDetailedTripPlanAsync(MoodTripRequestDTO req, string businessContext);

        Task<string> GenerateTextAsync(string systemPrompt, string userPrompt);
    }
}