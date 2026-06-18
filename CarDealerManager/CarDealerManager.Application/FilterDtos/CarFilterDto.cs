using CarDealerManager.Application.FilterDtos.Base;
using CarDealerManager.Domain.Entities;

namespace CarDealerManager.Application.FilterDtos
{
    public class CarFilterDto : FilterDto<Car>
    {
        public string? Status { get; set; }
        public string? Brand { get; set; }

        public override IQueryable<Car> WhereBuilder(IQueryable<Car> query)
        {
            query = base.WhereBuilder(query);

            if (!string.IsNullOrWhiteSpace(TextFilter))
            {
                var text = TextFilter.ToLower();

                query = query.Where(c =>
                    c.Brand.ToLower().Contains(text) ||
                    c.Model.ToLower().Contains(text) ||
                    c.Vin.ToLower().Contains(text));
            }

            if (!string.IsNullOrWhiteSpace(Status))
            {
                query = query.Where(c => c.Status == Status);
            }

            if (!string.IsNullOrWhiteSpace(Brand))
            {
                query = query.Where(c => c.Brand == Brand);
            }

            return query;
        }
    }
}
