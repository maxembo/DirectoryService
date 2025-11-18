using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Postgres.Departments;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id)
            .HasName("pk_departments");

        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => DepartmentId.Create(value))
            .HasColumnName("id");

        builder.ComplexProperty(
            d => d.Name, db =>
            {
                db.Property(d => d.Value)
                    .HasMaxLength(Constants.MAX_DEPARTMENT_NAME_LENGTH)
                    .IsRequired()
                    .HasColumnName("name");
            });

        builder.ComplexProperty(
            d => d.Identifier, db =>
            {
                db.Property(d => d.Value)
                    .HasMaxLength(Constants.MAX_DEPARTMENT_IDENTIFIER_LENGTH)
                    .IsRequired()
                    .HasColumnName("identifier");
            });

        builder.ComplexProperty(
            d => d.Path, db =>
            {
                db.Property(d => d.Value)
                    .HasMaxLength(Constants.MAX_DEPARTMENT_PATH_LENGTH)
                    .IsRequired()
                    .HasColumnName("path");

                db.Property(d => d.Depth)
                    .IsRequired()
                    .HasColumnName("depth");
            });

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(d => d.ParentId)
            .IsRequired(false)
            .HasColumnName("parent_id");

        builder.HasMany(d => d.Childrens)
            .WithOne()
            .HasForeignKey(d => d.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false)
            .HasConstraintName("fk_departments_children");
    }
}