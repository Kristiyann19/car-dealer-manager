namespace CarDealerManager.Domain.Entities
{
    public class Expense
    {
        public int Id { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public string Type { get; set; } 
        public string Description { get; set; }

        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
