using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NaverWebtoonDownloader.CoreLib.Migrations
{
    public partial class Init2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Webtoons",
                columns: table => new
                {
                    ID = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Writer = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Webtoons", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Episodes",
                columns: table => new
                {
                    WebtoonID = table.Column<long>(type: "INTEGER", nullable: false),
                    No = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Episodes", x => new { x.WebtoonID, x.No });
                    table.ForeignKey(
                        name: "FK_Episodes_Webtoons_WebtoonID",
                        column: x => x.WebtoonID,
                        principalTable: "Webtoons",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    WebtoonID = table.Column<long>(type: "INTEGER", nullable: false),
                    EpisodeNo = table.Column<long>(type: "INTEGER", nullable: false),
                    No = table.Column<long>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    IsDownloaded = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => new { x.WebtoonID, x.EpisodeNo, x.No });
                    table.ForeignKey(
                        name: "FK_Images_Episodes_WebtoonID_EpisodeNo",
                        columns: x => new { x.WebtoonID, x.EpisodeNo },
                        principalTable: "Episodes",
                        principalColumns: new[] { "WebtoonID", "No" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Images_Webtoons_WebtoonID",
                        column: x => x.WebtoonID,
                        principalTable: "Webtoons",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_WebtoonID",
                table: "Episodes",
                column: "WebtoonID");

            migrationBuilder.CreateIndex(
                name: "IX_Images_EpisodeNo",
                table: "Images",
                column: "EpisodeNo");

            migrationBuilder.CreateIndex(
                name: "IX_Images_WebtoonID",
                table: "Images",
                column: "WebtoonID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "Episodes");

            migrationBuilder.DropTable(
                name: "Webtoons");
        }
    }
}
