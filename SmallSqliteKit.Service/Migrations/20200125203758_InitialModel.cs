using Microsoft.EntityFrameworkCore.Migrations;

namespace SmallSqliteKit.Service.Migrations
{
    public partial class InitialModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configs",
                columns: table => new
                {
                     ConfigName = table.Column<string>(nullable: false),
                     ConfigValue = table.Column<string>(nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Configs", c => c.ConfigName)
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Configs");
        }
    }
}
