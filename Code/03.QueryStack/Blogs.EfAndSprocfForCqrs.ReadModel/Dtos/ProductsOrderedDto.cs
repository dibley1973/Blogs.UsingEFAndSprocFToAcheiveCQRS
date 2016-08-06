
namespace Blogs.EfAndSprocfForCqrs.ReadModel.Dtos
{
    public class ProductsOrderedDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PurchasePrice { get; set; }
    }
}