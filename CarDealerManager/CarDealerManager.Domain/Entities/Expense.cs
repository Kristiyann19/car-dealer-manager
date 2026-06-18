using CarDealerManager.Domain.Entities.Base;

namespace CarDealerManager.Domain.Entities
{
    public class Expense : Entity
    {
        public int CarId { get; set; }
        public Car Car { get; set; } = null!;

        public string Type { get; set; } 
        public string Description { get; set; }

        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
