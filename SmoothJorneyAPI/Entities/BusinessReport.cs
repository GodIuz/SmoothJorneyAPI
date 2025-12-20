namespace SmoothJorneyAPI.Entities
{
    public class BusinessReport
    {
        public int Id { get; set; }
        public string? Reason { get; set; }
        public bool IsResolved { get; set; }

        public int ShopId { get; set; }
        public virtual Business? Business { get; set; }

        public string? UserId { get; set; }
    }
}