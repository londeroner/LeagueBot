using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeagueBot.Infrastructure.Migrations
{
    public partial class PollId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollUserResult_User_UserId",
                table: "PollUserResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.AddColumn<string>(
                name: "PollId",
                table: "DailyPolls",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PollUserResult_Users_UserId",
                table: "PollUserResult",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollUserResult_Users_UserId",
                table: "PollUserResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PollId",
                table: "DailyPolls");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PollUserResult_User_UserId",
                table: "PollUserResult",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
