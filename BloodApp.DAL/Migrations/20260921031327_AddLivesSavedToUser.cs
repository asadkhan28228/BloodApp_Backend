using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodApp.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddLivesSavedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LivesSaved",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LivesSaved",
                table: "Users");
        }
    }
}
