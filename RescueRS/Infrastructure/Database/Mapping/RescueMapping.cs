using ResgateRS.Domain.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ResgateRS.Infrastructure.Database.Mapping;
public class RescueMapping : IEntityTypeConfiguration<RescueEntity>
{
    public void Configure(EntityTypeBuilder<RescueEntity> builder)
    {
        builder.HasKey(e => e.RescueId);

        builder.Property(e => e.RescueId);
        builder.Property(e => e.RequestDateTime);
        builder.Property(e => e.TotalPeopleNumber);
        builder.Property(e => e.ChildrenNumber);
        builder.Property(e => e.ElderlyNumber);
        builder.Property(e => e.DisabledNumber);
        builder.Property(e => e.AnimalsNumber);
        builder.Property(e => e.Latitude);
        builder.Property(e => e.Longitude);
        builder.Property(e => e.Rescued)
            .HasConversion(
                v => v ? 1 : 0, // Convert TRUE to 1, FALSE to 0
                v => v == 1); // Convert 1 back to TRUE, anything else to FALSE
        builder.Property(e => e.RescueDateTime);
    }
}