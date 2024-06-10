
using Jamidar.Data;
using System.ComponentModel.DataAnnotations;



namespace Jamidar.Models
{
    public enum ProductType { Mug, Jug, Cup }

    public enum Size { Small, Medium, Large }
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public ProductType Type { get; set; }
        public List<Variant> Variants { get; set; } = new List<Variant>();
    }

    public class Variant
    {
        [Key]
        public int VariantId { get; set; }
        public string Color { get; set; }
        public string Specification { get; set; }
        public Size Size { get; set; }
    }


}