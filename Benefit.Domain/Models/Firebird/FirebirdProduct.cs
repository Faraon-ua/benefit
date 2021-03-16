namespace Benefit.Domain.Models.Firebird
{
    public class FirebirdCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class FirebirdProduct
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string CategoryId { get; set; }
        public string Barcode { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
    }
}
