using SmoothJorneyAPI.Entities;

namespace SmoooothJourneyApi.Entities
{
    public class BusinessImage
    {
        public int Id { get; set; }

        public byte[]? ImageContent { get; set; }

        public string? ContentType { get; set; }

        public int BusinessId { get; set; }

        public virtual Business? Business { get; set; }
    }
}