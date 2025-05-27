namespace ShoesFactory.Models
{
    public class Shoe
    {
        public string Article { get; set; }
        public string ModelName { get; set; }
        public int Size { get; set; }
        public int QuantityInStock { get; set; }
        public decimal Price { get; set; }
        public string PhotoPath { get; set; }
    }
}
