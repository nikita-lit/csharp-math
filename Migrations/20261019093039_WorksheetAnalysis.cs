using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace csharp_math.Migrations
{
    /// <inheritdoc />
    public partial class WorksheetAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnalysisVersion",
                table: "FunctionAnalyses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Parity",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "RestrictDomain",
                table: "FunctionAnalyses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ValueRange",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisVersion",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "Parity",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "RestrictDomain",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "ValueRange",
                table: "FunctionAnalyses");
        }
    }
}
