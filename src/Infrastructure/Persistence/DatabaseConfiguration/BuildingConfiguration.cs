using Domain.Buildings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.DatabaseConfiguration;

internal sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.HasKey(b => b.Id);

        builder.HasIndex(b => b.Name).IsUnique();

        builder.Property(b => b.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        //the below relationship reads as follows:
        //a building has relationship with CreatedByUser with a foreign key of CreatedByUserId
        //the relationship is one to many meaning that a building can only have one user that created it
        builder.HasOne(b => b.CreatedByUser)
            .WithMany()
            .HasForeignKey(b => b.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(b => b.UpdatedByUser)
            .WithMany()
            .HasForeignKey(b => b.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
