using BloodDonationAPI.DAL.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodApp.DAL.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261012120000_AddDonatedMlToUser")]
    public partial class AddDonatedMlToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DonatedMl",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DonatedMl", table: "Users");
        }
    }
}
