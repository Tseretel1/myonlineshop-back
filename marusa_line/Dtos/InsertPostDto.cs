using marusa_line.Models;

namespace marusa_line.Dtos
{
    public class InsertPostDto
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int? Quantity { get; set; }
        public int? ProductTypeId { get; set; }
        public bool OrderNotAllowed{ get; set; }
        public List<InsertPhoto>? Photos { get; set; } = new List<InsertPhoto>();
    }
    public class InsertPhoto
    {
        public string PhotoUrl { get; set; }
    }
}
