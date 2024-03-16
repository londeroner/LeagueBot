using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeagueBot.Infrastructure.Migrations
{
    public partial class PollIsFinished : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PollIsFinished",
                table: "DailyPolls",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PollIsFinished",
                table: "DailyPolls");
        }
    }
}
