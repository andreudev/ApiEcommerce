using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiEcommerce.Migrations
{
    /// <inheritdoc />
    public partial class ColumnUrlLocalToProduct2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrlLocal",
                table: "Products",
                newName: "ImgUrlLocal");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImgUrlLocal",
                table: "Products",
                newName: "ImageUrlLocal");
        }
    }
}
