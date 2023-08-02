using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleObjectStore.Migrations
{
    /// <inheritdoc />
    public partial class AllowedHosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedHosts",
                columns: table => new
                {
                    Hostname = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedHosts", x => x.Hostname);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedHosts");
        }
    }
}
