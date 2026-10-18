using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace csharp_math.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Concavity",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DerivativeLatex",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FormulaLatex",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InflectionPoints",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsNumerical",
                table: "FunctionAnalyses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Monotonicity",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Positivity",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "RangeFrom",
                table: "FunctionAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: -5.0);

            migrationBuilder.AddColumn<double>(
                name: "RangeTo",
                table: "FunctionAnalyses",
                type: "REAL",
                nullable: false,
                defaultValue: 5.0);

            migrationBuilder.AddColumn<string>(
                name: "SecondDerivative",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondDerivativeLatex",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "FunctionAnalyses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Concavity",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "DerivativeLatex",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "FormulaLatex",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "InflectionPoints",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "IsNumerical",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "Monotonicity",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "Positivity",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "RangeFrom",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "RangeTo",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "SecondDerivative",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "SecondDerivativeLatex",
                table: "FunctionAnalyses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "FunctionAnalyses");
        }
    }
}
