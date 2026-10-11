
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodApp.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==========================================
            // ADD EMAIL VERIFICATION COLUMNS
            // ==========================================

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerifiedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // ==========================================
            // PRESERVE EXISTING USERS
            // ==========================================
            // Users registered before this migration
            // are treated as verified to preserve login.
            // New users will still require verification.

            migrationBuilder.Sql(@"
                UPDATE [Users]
                SET [IsEmailVerified] = 1,
                    [EmailVerifiedAt] = SYSUTCDATETIME()
                WHERE [IsEmailVerified] = 0;
            ");

            // ==========================================
            // CREATE EMAIL VERIFICATION TOKENS TABLE
            // ==========================================

            migrationBuilder.CreateTable(
                name: "EmailVerificationTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    UserId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    TokenHash = table.Column<string>(
                        type: "nvarchar(64)",
                        maxLength: 64,
                        nullable: false),

                    ExpiresAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    IsUsed = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_EmailVerificationTokens",
                        x => x.Id);

                    table.ForeignKey(
                        name: "FK_EmailVerificationTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ==========================================
            // CREATE INDEXES
            // ==========================================

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_TokenHash",
                table: "EmailVerificationTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_UserId",
                table: "EmailVerificationTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ==========================================
            // ROLLBACK EMAIL VERIFICATION
            // ==========================================

            migrationBuilder.DropTable(
                name: "EmailVerificationTokens");

            migrationBuilder.DropColumn(
                name: "EmailVerifiedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Users");
        }
    }
}
