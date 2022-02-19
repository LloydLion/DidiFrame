using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CGZBot3.Data.Database.SettingsMigrations
{
    public partial class TestSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestSystemId",
                table: "Settings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SomeString = table.Column<string>(type: "TEXT", nullable: false),
                    TestChannel = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Settings_TestSystemId",
                table: "Settings",
                column: "TestSystemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Settings_TestSettings_TestSystemId",
                table: "Settings",
                column: "TestSystemId",
                principalTable: "TestSettings",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Settings_TestSettings_TestSystemId",
                table: "Settings");

            migrationBuilder.DropTable(
                name: "TestSettings");

            migrationBuilder.DropIndex(
                name: "IX_Settings_TestSystemId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "TestSystemId",
                table: "Settings");
        }
    }
}
