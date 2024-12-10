using Domain.Elevators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.DatabaseConfiguration;

internal sealed class ElevatorConfiguration : IEntityTypeConfiguration<Elevator>
{
    public void Configure(EntityTypeBuilder<Elevator> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Number)
            .IsRequired();
        
        //HasIndex to be unique across building id and number
        builder.HasIndex(e => new { e.BuildingId, e.Number })
            .IsUnique();

        builder.Property(e => e.CurrentFloor)
            .IsRequired();

        builder.Property(e => e.ElevatorDirection)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.ElevatorStatus)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.ElevatorType)
            .HasConversion<string>()            
            .IsRequired();

        builder.Property(e => e.FloorsPerSecond)
            .IsRequired();

        builder.Property(e => e.QueueCapacity)
            .IsRequired();

        builder.HasOne(e => e.Building)
            .WithMany()
            .HasForeignKey(e => e.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
