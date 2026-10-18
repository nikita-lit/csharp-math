using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace csharp_math.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FunctionAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Formula = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Derivative = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Zeros = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CriticalPoints = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Extrema = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionAnalyses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FunctionAnalyses_CreatedAt",
                table: "FunctionAnalyses",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FunctionAnalyses");
        }
    }
}
