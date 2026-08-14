using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeCheck.DataAcces.Migrations
{
    /// <inheritdoc />
    public partial class AddRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VenueAdminAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueAdminAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VenueAdminAssignments_Users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VenueAdminAssignments_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenueBans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BannedByAdminUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VenueBans_Users_BannedByAdminUserId",
                        column: x => x.BannedByAdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VenueBans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VenueBans_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VenueAdminAssignments_AdminUserId_VenueId",
                table: "VenueAdminAssignments",
                columns: new[] { "AdminUserId", "VenueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VenueAdminAssignments_VenueId",
                table: "VenueAdminAssignments",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueBans_BannedByAdminUserId",
                table: "VenueBans",
                column: "BannedByAdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueBans_UserId",
                table: "VenueBans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueBans_VenueId_UserId",
                table: "VenueBans",
                columns: new[] { "VenueId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VenueAdminAssignments");

            migrationBuilder.DropTable(
                name: "VenueBans");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
