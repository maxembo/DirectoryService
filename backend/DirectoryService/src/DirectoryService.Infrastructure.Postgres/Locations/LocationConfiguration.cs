using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Locations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id)
            .HasName("pk_locations");

        builder.Property(l => l.Id)
            .HasConversion(
                id => id.Value,
                value => LocationId.Create(value))
            .HasColumnName("id");

        builder.OwnsOne(
            l => l.Name, lb =>
            {
                lb.Property(l => l.Value)
                    .HasMaxLength(Constants.MAX_LOCATION_NAME_LENGTH)
                    .IsRequired()
                    .HasColumnName("name");

                lb.HasIndex(l => l.Value)
                    .IsUnique();
            });

        builder.ComplexProperty(
            l => l.Timezone, lb =>
            {
                lb.Property(l => l.Value)
                    .HasMaxLength(Constants.MAX_LOCATION_TIMEZONE_LENGTH)
                    .IsRequired()
                    .HasColumnName("timezone");
            });

        builder.OwnsOne(
            l => l.Address, lb =>
            {
                lb.ToJson("address");

                lb.Property(l => l.City)
                    .HasMaxLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
                    .IsRequired()
                    .HasColumnName("city");

                lb.Property(l => l.Country)
                    .HasMaxLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
                    .IsRequired()
                    .HasColumnName("country");

                lb.Property(l => l.Street)
                    .HasMaxLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
                    .IsRequired()
                    .HasColumnName("street");

                lb.Property(l => l.House)
                    .HasMaxLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
                    .IsRequired()
                    .HasColumnName("house");
            });

        builder.Property(l => l.IsActive)
            .HasColumnName("is_active");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(l => l.DeletedAt)
            .HasColumnName("deleted_at");
    }
}