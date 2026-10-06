using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Mish.Api.Data;

#nullable disable

namespace Mish.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261006154000_CategorySlugUnique")]
public sealed class CategorySlugUnique : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Categories_Slug",
            table: "Categories",
            column: "Slug",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Categories_Slug",
            table: "Categories");
    }
}
