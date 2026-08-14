namespace CarDealerManager.Domain.Entities.Base
{
    public abstract class Entity
    {
        public int Id { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
        public DateTimeOffset? ArchivedAtUtc { get; set; }
    }
}
