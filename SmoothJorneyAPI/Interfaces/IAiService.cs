namespace SmoothJorneyAPI.Interfaces
{
    public interface IAiService
    {
        Task<string> GetTripPlanAsync(string city, int days, decimal budget, string mood, string weather, DateTime startDate);
        
        Task<string> SummarizeReviewsAsync(IEnumerable<string> reviews);
    }
}
