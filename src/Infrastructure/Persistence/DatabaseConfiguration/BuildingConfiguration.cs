using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.HasKey(b => b.Id);

        builder.HasIndex(b => b.Name).IsUnique();
        //
        // builder.HasMany(b => b.Users)
        //     .WithOne(u => u.Building)
        //     .HasForeignKey(u => u.BuildingId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
