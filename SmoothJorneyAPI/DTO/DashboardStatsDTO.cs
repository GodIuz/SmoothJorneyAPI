namespace SmoothJorneyAPI.DTO
{
    public class DashboardStatsDTO
    {
        public int TotalUsers { get; set; }
        public int TotalBusinesses { get; set; }
        public int NewReviews { get; set; }
        public int PendingRequests { get; set; }
        public List<TrafficDataDTO> TrafficData { get; set; } = new List<TrafficDataDTO>();
        public List<RecentActivityDTO> RecentActivity { get; set; } = new List<RecentActivityDTO>();
        public List<BusinessSummaryDTO>? LatestBusinesses { get; set; }
    }
}