using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Positions;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id)
            .HasName("pk_positions");

        builder.Property(p => p.Id).HasConversion(
                id => id.Value,
                value => PositionId.Create(value))
            .HasColumnName("id");

        builder.OwnsOne(
            p => p.Name, pb =>
            {
                pb.Property(p => p.Value)
                    .HasMaxLength(Constants.MAX_POSITION_NAME_LENGTH)
                    .IsRequired()
                    .HasColumnName("name");

                pb.HasIndex(p => p.Value)
                    .IsUnique();
            });

        builder.OwnsOne(
            p => p.Description, pb =>
            {
                pb.Property(p => p.Value)
                    .IsRequired(false)
                    .HasMaxLength(Constants.MAX_POSITION_DESCRIPTION_LENGTH)
                    .HasColumnName("description");
            });

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active");

        builder.Property(p => p.DeletedAt)
            .HasColumnName("deleted_at");
    }
}