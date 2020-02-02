using System;
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
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configs", x => x.ConfigName);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseBackups",
                columns: table => new
                {
                    DatabaseBackupId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DatabasePath = table.Column<string>(nullable: false),
                    BackupFrequency = table.Column<int>(nullable: false),
                    LastBackupTime = table.Column<DateTime>(nullable: true),
                    UploadToDropbox = table.Column<bool>(nullable: false),
                    UploadToDropboxFrequency = table.Column<int>(nullable: true),
                    LastUploadToDropboxTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseBackups", x => x.DatabaseBackupId);
                });

            migrationBuilder.CreateTable(
                name: "BackupAudits",
                columns: table => new
                {
                    BackupAuditId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DatabaseBackupId = table.Column<int>(nullable: false),
                    AuditLog = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupAudits", x => x.BackupAuditId);
                    table.ForeignKey(
                        name: "FK_BackupAudits_DatabaseBackups_DatabaseBackupId",
                        column: x => x.DatabaseBackupId,
                        principalTable: "DatabaseBackups",
                        principalColumn: "DatabaseBackupId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackupAudits_DatabaseBackupId",
                table: "BackupAudits",
                column: "DatabaseBackupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackupAudits");

            migrationBuilder.DropTable(
                name: "Configs");

            migrationBuilder.DropTable(
                name: "DatabaseBackups");
        }
    }
}
