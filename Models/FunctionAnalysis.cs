using System.ComponentModel.DataAnnotations;

namespace csharp_math.Models
{
    /// <summary>
    /// A stored analysis of a single-variable function f(x).
    /// </summary>
    public class FunctionAnalysis
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Formula { get; set; } = string.Empty; // e.g. "x^3 - 3*x"

        [MaxLength(1000)]
        public string Domain { get; set; } = string.Empty; // e.g. "(-inf, inf)"

        [MaxLength(1000)]
        public string Derivative { get; set; } = string.Empty; // e.g. "-3 + 3*x^2"

        [MaxLength(1000)]
        public string Zeros { get; set; } = string.Empty; // e.g. "x1 = -1.7321, x2 = 0, x3 = 1.7321"

        [MaxLength(1000)]
        public string CriticalPoints { get; set; } = string.Empty; // e.g. "x1 = -1, x2 = 1"

        [MaxLength(1000)]
        public string Extrema { get; set; } = string.Empty; // e.g. "max f(-1) = 2; min f(1) = -2"

        public string FormulaLatex { get; set; } = string.Empty;

        public string DerivativeLatex { get; set; } = string.Empty;

        public string SecondDerivativeLatex { get; set; } = string.Empty;

        public string ValueRange { get; set; } = string.Empty;

        public string Parity { get; set; } = string.Empty;

        public bool RestrictDomain { get; set; }

        public int AnalysisVersion { get; set; }

        public string Positivity { get; set; } = string.Empty;

        public string Monotonicity { get; set; } = string.Empty;

        public string SecondDerivative { get; set; } = string.Empty;

        public string InflectionPoints { get; set; } = string.Empty;

        public string Concavity { get; set; } = string.Empty;

        public double RangeFrom { get; set; } = -5;

        public double RangeTo { get; set; } = 5;

        public bool IsNumerical { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
