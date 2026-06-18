namespace CarDealerManager.Application.Dtos
{
    public class ExpenseDto
    {
        public int Id { get; set; }

        public int CarId { get; set; }
        public CarDto Car { get; set; } = null!;

        public string Type { get; set; }
        public string Description { get; set; }

        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
