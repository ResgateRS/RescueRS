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
        builder.Property(e => e.AdultsNumber);
        builder.Property(e => e.ChildrenNumber);
        builder.Property(e => e.ElderlyNumber);
        builder.Property(e => e.DisabledNumber);
        builder.Property(e => e.AnimalsNumber);
        builder.Property(e => e.Latitude);
        builder.Property(e => e.Longitude);
        builder.Property(e => e.Status);
        builder.Property(e => e.RescueDateTime);
    }
}