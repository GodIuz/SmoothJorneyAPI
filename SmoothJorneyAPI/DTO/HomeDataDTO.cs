
namespace SmoothJorneyAPI.DTO
{
    public class HomeDataDTO
    {
        public BusinessSummaryDTO? TopShop { get; set; }
        public List<BusinessSummaryDTO> TopAttractions { get; set; } = new();
    }
}
