using CarBuilder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarBuilder.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Car> Cars { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
