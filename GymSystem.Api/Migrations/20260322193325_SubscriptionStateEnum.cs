using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionStateEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Subscriptions");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Subscriptions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "Subscriptions");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
