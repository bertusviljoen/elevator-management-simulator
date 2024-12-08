using Domain.Users;
using Domain.Buildings;
using Domain.Elevators;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

/// <summary> Application database context. </summary>
public interface IApplicationDbContext
{
    /// <summary> Get the DbSet of User entities. </summary>
    DbSet<User> Users { get; }
    /// <summary> Get the DbSet of Building entities. </summary>
    DbSet<Building> Buildings { get; }
    /// <summary> Get the DbSet of Elevator entities. </summary>
    DbSet<Elevator> Elevators { get; }
    /// <summary> Save the changes to the database. </summary>
    /// <param name="cancellationToken"> The cancellation token. </param>
    /// <returns> The number of state entries written to the database. </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
