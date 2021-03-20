using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WDLT.Feed.Database.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceId = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: true),
                    IsProtected = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastTimestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false, defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blacklist",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Word = table.Column<string>(type: "TEXT", maxLength: 35, nullable: false),
                    SubscriptionId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blacklist_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CardId = table.Column<string>(type: "TEXT", nullable: true),
                    Header = table.Column<string>(type: "TEXT", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    HasImage = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasVideo = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasRepost = table.Column<bool>(type: "INTEGER", nullable: false),
                    OriginalUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsFresh = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsViewed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBookmark = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    SubscriptionId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blacklist_SubscriptionId_Word",
                table: "Blacklist",
                columns: new[] { "SubscriptionId", "Word" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardId_SubscriptionId",
                table: "Cards",
                columns: new[] { "CardId", "SubscriptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_IsBookmark",
                table: "Cards",
                column: "IsBookmark");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_IsFresh",
                table: "Cards",
                column: "IsFresh");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_IsViewed",
                table: "Cards",
                column: "IsViewed");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_SubscriptionId",
                table: "Cards",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Timestamp",
                table: "Cards",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Source",
                table: "Subscriptions",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SourceId_Source",
                table: "Subscriptions",
                columns: new[] { "SourceId", "Source" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blacklist");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Subscriptions");
        }
    }
}
