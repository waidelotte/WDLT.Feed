using Microsoft.EntityFrameworkCore.Migrations;

namespace WDLT.Feed.Database.Migrations
{
    public partial class removeFresh : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_IsFresh",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "IsFresh",
                table: "Cards");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFresh",
                table: "Cards",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_IsFresh",
                table: "Cards",
                column: "IsFresh");
        }
    }
}
