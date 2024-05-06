using ResgateRS.Domain.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ResgateRS.Infrastructure.Database.Mapping;
public class UserMapping : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(e => e.UserId);

        builder.Property(e => e.UserId);
        builder.Property(e => e.Rescuer)
            .HasConversion(
                v => v ? 1 : 0, // Convert TRUE to 1, FALSE to 0
                v => v == 1); // Convert 1 back to TRUE, anything else to FALSE
        builder.Property(e => e.Cellphone);
    }
}