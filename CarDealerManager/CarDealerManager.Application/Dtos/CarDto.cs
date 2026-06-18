namespace CarDealerManager.Application.Dtos
{
    public class CarDto
    {
        public int Id { get; set; }

        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }

        public string Vin { get; set; }

        public decimal PurchasePrice { get; set; }
        public DateTime PurchaseDate { get; set; }

        public decimal? SalePrice { get; set; }
        public DateTime? SaleDate { get; set; }

        public string Status { get; set; } = "Purchased"; //make enum

        public ICollection<ExpenseDto> Expenses { get; set; } = new List<ExpenseDto>();
    }
}
