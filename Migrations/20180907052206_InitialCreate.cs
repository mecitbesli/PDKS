using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PDKS.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StandartWorkhoursSet",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StartHr = table.Column<DateTime>(nullable: false),
                    EndHr = table.Column<DateTime>(nullable: false),
                    BreakTime = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandartWorkhoursSet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSet",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    Username = table.Column<string>(nullable: false),
                    Password = table.Column<string>(nullable: false),
                    Role = table.Column<int>(nullable: false, defaultValue: 2),
                    DaysOff = table.Column<int>(nullable: false, defaultValue: 15),
                    StandartWorkHoursId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSet_StandartWorkhoursSet_StandartWorkHoursId",
                        column: x => x.StandartWorkHoursId,
                        principalTable: "StandartWorkhoursSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminAuthorizationSet",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Customize = table.Column<bool>(nullable: false, defaultValue: false),
                    Requests = table.Column<bool>(nullable: false, defaultValue: false),
                    Authority = table.Column<bool>(nullable: false, defaultValue: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAuthorizationSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminAuthorizationSet_UserSet_UserId",
                        column: x => x.UserId,
                        principalTable: "UserSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWorkedSet",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkedTime = table.Column<int>(nullable: false),
                    date = table.Column<DateTime>(nullable: false),
                    Excuse = table.Column<string>(nullable: true),
                    Request = table.Column<int>(nullable: false),
                    ReqApproved = table.Column<int>(nullable: false, defaultValue: 2),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWorkedSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWorkedSet_UserSet_UserId",
                        column: x => x.UserId,
                        principalTable: "UserSet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuthorizationSet_UserId",
                table: "AdminAuthorizationSet",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSet_StandartWorkHoursId",
                table: "UserSet",
                column: "StandartWorkHoursId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkedSet_UserId",
                table: "UserWorkedSet",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAuthorizationSet");

            migrationBuilder.DropTable(
                name: "UserWorkedSet");

            migrationBuilder.DropTable(
                name: "UserSet");

            migrationBuilder.DropTable(
                name: "StandartWorkhoursSet");
        }
    }
}
