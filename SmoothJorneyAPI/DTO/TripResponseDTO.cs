namespace SmoothJorneyAPI.DTO
{
    public class TripResponseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal CurrentCost { get; set; }
        public decimal RemainingBudget { get; set; }
        public List<TripItemResponseDTO> Activities { get; set; } = new List<TripItemResponseDTO>();
    }
}
