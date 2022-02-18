using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CGZBot3.Data.Database.SettingsMigrations
{
    public partial class Indexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Settings_ServerId",
                table: "Settings",
                column: "ServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Settings_ServerId",
                table: "Settings");
        }
    }
}
