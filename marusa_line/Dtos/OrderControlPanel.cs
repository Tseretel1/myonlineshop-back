using marusa_line.Models;

namespace marusa_line.Dtos
{
    public class OrderControlPanel
    {
        public int OrderId { get; set; }
        public DateTime CreateDate { get; set; }
        public int StatusId { get; set; }
        public bool IsPaid { get; set; }
        public string? DeliveryType { get; set; }
        public int ProductQuantity { get; set; }
        public string? Comment { get; set; }
        public decimal FinalPrice { get; set; }

        public int ProductId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int ProductTypeId { get; set; }

        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
        public int OrderNumber { get; set; }

        public List<Photos> Photos { get; set; } = new List<Photos>();

        public User User { get; set; }
    }
}
