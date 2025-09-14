using N1coLoyalty.Application.Common.Interfaces;

namespace N1coLoyalty.Infrastructure.Services;

internal class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
}