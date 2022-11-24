using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactPro_MVC.Data.Migrations
{
    public partial class _005_contactcategoryID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryId",
                table: "Contacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactId",
                table: "Categories",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Categories");
        }
    }
}
