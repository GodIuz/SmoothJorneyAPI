namespace SmoothJorneyAPI.DTO
{
    public class DashboardStatsDTO
    {
        public int TotalUsers { get; set; }
        public int TotalBusinesses { get; set; }
        public int NewReviews { get; set; }
        public List<RecentActivityDTO> RecentActivity { get; set; } = new List<RecentActivityDTO>();
        public List<BusinessSummaryDTO>? LatestBusinesses { get; set; }
    }
}