namespace Benefit.Domain.Models.Excel
{
    public class ExcelProduct
    {
        public ExcelProduct()
        {
            Product = new Product();
        }
        public Product Product { get; set; }
        public Category Category { get; set; }
        public string ImagesList { get; set; }
        public string CurrencyName { get; set; }
        public int Visible { get; set; }
    }
}
