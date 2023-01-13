using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmallSqliteKit.Service.Migrations
{
    /// <inheritdoc />
    public partial class AddDbOptimizeAndVacuum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastOptimizeTime",
                table: "DatabaseBackups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastVacuumTime",
                table: "DatabaseBackups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Optimize",
                table: "DatabaseBackups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OptimizeFrequency",
                table: "DatabaseBackups",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Vacuum",
                table: "DatabaseBackups",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VacuumFrequency",
                table: "DatabaseBackups",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastOptimizeTime",
                table: "DatabaseBackups");

            migrationBuilder.DropColumn(
                name: "LastVacuumTime",
                table: "DatabaseBackups");

            migrationBuilder.DropColumn(
                name: "Optimize",
                table: "DatabaseBackups");

            migrationBuilder.DropColumn(
                name: "OptimizeFrequency",
                table: "DatabaseBackups");

            migrationBuilder.DropColumn(
                name: "Vacuum",
                table: "DatabaseBackups");

            migrationBuilder.DropColumn(
                name: "VacuumFrequency",
                table: "DatabaseBackups");
        }
    }
}
