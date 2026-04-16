using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class FixLocationNameAndAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .Annotation("Npgsql:PostgresExtension:ltree", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "locations",
                type: "citext",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS ix_locations_address;
                """);

            migrationBuilder.Sql(
                """
                 CREATE UNIQUE INDEX ix_locations_address
                    ON locations (
                                  (lower(address ->> 'Country')),
                                  (lower(address ->> 'City')),
                                  (lower(address ->> 'Street')),
                                  (lower(address ->> 'House'))
                        );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "locations",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 120);

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS ix_locations_address;
                """);

            migrationBuilder.Sql(@" CREATE UNIQUE INDEX ix_locations_address ON locations (address)");
        }
    }
}

