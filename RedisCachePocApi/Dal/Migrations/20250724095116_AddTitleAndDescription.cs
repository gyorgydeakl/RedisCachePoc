using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedisCachePocApi.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Reviews",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Reviews",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Reviews",
                newName: "Text");
        }
    }
}
