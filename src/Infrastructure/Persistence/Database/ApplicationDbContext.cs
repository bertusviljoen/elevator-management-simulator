using Application.Abstractions.Data;
using Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database;

/// <inheritdoc cref="IApplicationDbContext" />
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):
    DbContext(options), IApplicationDbContext
{
    /// <inheritdoc cref="IApplicationDbContext" />
    public DbSet<User> Users { get; init; }

    /// <inheritdoc cref="DbContext" />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

    /// <inheritdoc cref="DbContext" />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

}
