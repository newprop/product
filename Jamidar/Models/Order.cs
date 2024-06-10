using System.ComponentModel.DataAnnotations;

namespace Jamidar.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserAddress { get; set; }
    }

    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
