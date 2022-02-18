using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CGZBot3.Data.Database.StateMigrations
{
    public partial class Indexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GlobalState_ServerId",
                table: "GlobalState",
                column: "ServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GlobalState_ServerId",
                table: "GlobalState");
        }
    }
}
