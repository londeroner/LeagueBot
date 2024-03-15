using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LeagueBot.Infrastructure.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyPolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PollIsStarted = table.Column<bool>(type: "boolean", nullable: false),
                    ChatId = table.Column<string>(type: "text", nullable: true),
                    TimeToStartVote = table.Column<TimeSpan>(type: "interval", nullable: false),
                    TimeToStartGame = table.Column<TimeSpan>(type: "interval", nullable: false),
                    PollDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPolls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PollUserResult",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    DailyPollId = table.Column<int>(type: "integer", nullable: false),
                    PollResult = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollUserResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollUserResult_DailyPolls_DailyPollId",
                        column: x => x.DailyPollId,
                        principalTable: "DailyPolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PollUserResult_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollUserResult_DailyPollId",
                table: "PollUserResult",
                column: "DailyPollId");

            migrationBuilder.CreateIndex(
                name: "IX_PollUserResult_UserId",
                table: "PollUserResult",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PollUserResult");

            migrationBuilder.DropTable(
                name: "DailyPolls");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
