namespace SmoothJorneyAPI.Entities
{
    public class BusinessPhoto
    {
        public int Id { get; set; }
        public string Url { get; set; } 
        public int BusinessId { get; set; }
        public Business Business { get; set; }
    }
}