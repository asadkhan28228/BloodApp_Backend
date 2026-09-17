using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodApp.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmergencySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "EmergencyResponses");

            migrationBuilder.DropColumn(
                name: "DistanceKm",
                table: "EmergencyResponses");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "EmergencyResponses");

            migrationBuilder.RenameColumn(
                name: "DonorLongitude",
                table: "EmergencyResponses",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "DonorLatitude",
                table: "EmergencyResponses",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "DonorBloodType",
                table: "EmergencyResponses",
                newName: "BloodType");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "EmergencyResponses",
                newName: "RespondedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RespondedAt",
                table: "EmergencyResponses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "EmergencyResponses",
                newName: "DonorLongitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "EmergencyResponses",
                newName: "DonorLatitude");

            migrationBuilder.RenameColumn(
                name: "BloodType",
                table: "EmergencyResponses",
                newName: "DonorBloodType");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "EmergencyResponses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DistanceKm",
                table: "EmergencyResponses",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "EmergencyResponses",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}
