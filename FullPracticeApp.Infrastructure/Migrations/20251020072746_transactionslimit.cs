using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FullPracticeApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class transactionslimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AmountDeposited",
                table: "Users",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AmountTransferred",
                table: "Users",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AmountWithdrawn",
                table: "Users",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountDeposited",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AmountTransferred",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AmountWithdrawn",
                table: "Users");
        }
    }
}
