using csharp_math.Services;
using Microsoft.EntityFrameworkCore;

namespace csharp_math.Data
{
    public static class SeedData
    {
        private static readonly string[] SampleFormulas =
        [
            "x^3 - 3*x",
            "x^2 - 4*x + 3",
            "x^3 - 12*x",
            "(x^2 - 1)/(x - 2)",
            "sqrt(x)*(x - 3)",
            "x^4 - 2x^2",
            "x*e^(-x)",
            "sqrt(4 - x^2)",
        ];

        /// <summary>
        /// Fills an empty database with sample functions. The analysis data is computed by the
        /// analyzer, so it always matches the formulas.
        /// </summary>
        public static async Task SeedAsync(AppDbContext context, FunctionAnalyzer analyzer)
        {
            if (await context.FunctionAnalyses.AnyAsync())
            {
                // Upgrade analyses saved by the previous engine without losing IDs or creation dates.
                var legacy = await context.FunctionAnalyses.Where(f => f.AnalysisVersion < 2).ToListAsync();
                foreach (var analysis in legacy)
                {
                    // Keep saved records from earlier versions even if their formula
                    // is outside the current worksheet calculator.
                    if (analyzer.TryValidate(analysis.Formula, out _))
                    {
                        analyzer.Fill(analysis);
                    }
                }

                await context.SaveChangesAsync();
                return;
            }

            foreach (var formula in SampleFormulas)
            {
                await context.FunctionAnalyses.AddAsync(analyzer.Analyze(formula));
            }

            await context.SaveChangesAsync();
        }
    }
}
