namespace Blogs.EfAndSprocfForCqrs.Services.Models
{
    public class ProductModel
    {
        public string Description { get; set; }
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public int ProductId { get; set; }
        public decimal PurchasePrice { get; set; }
    }
}