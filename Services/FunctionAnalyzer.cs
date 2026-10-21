using csharp_math.Models;
using csharp_math.Services.Mathematics;

namespace csharp_math.Services
{
    public class FormulaException(string message) : Exception(message);

    /// <summary>
    /// Adapts the imported mathematics engine to the existing MVC application and database.
    /// </summary>
    public class FunctionAnalyzer
    {
        private readonly FunctionAnalysisEngine _engine = new();

        public bool TryValidate(string? formula, out string? error)
        {
            error = string.IsNullOrWhiteSpace(formula)
                ? "Sisesta funktsiooni valem."
                : formula.Length > 200
                    ? "Valem võib olla kuni 200 märki pikk."
                    : _engine.Validate(formula);

            return error == null;
        }

        public AnalysisResult Compute(string formula, double from = -5, double to = 5, bool restrictDomain = false)
        {
            if (!TryValidate(formula, out var error))
            {
                throw new FormulaException(error!);
            }

            var rangeError = ValidateRange(from, to);
            if (rangeError != null)
            {
                throw new FormulaException(rangeError);
            }

            try
            {
                return _engine.Analyze(formula.Trim(), from, to, restrictDomain);
            }
            catch (FormulaParseException ex)
            {
                throw new FormulaException(ex.Message);
            }
            catch (ArithmeticException)
            {
                throw new FormulaException("Valemi arvutamine ei õnnestunud. Kontrolli valemit.");
            }
        }

        public FunctionAnalysis Analyze(string formula, double from = -5, double to = 5, bool restrictDomain = false)
        {
            var analysis = new FunctionAnalysis
            {
                Formula = formula.Trim(),
                RangeFrom = from,
                RangeTo = to,
                RestrictDomain = restrictDomain
            };

            Fill(analysis);
            return analysis;
        }

        public void Fill(FunctionAnalysis analysis)
        {
            var t = Compute(analysis.Formula, analysis.RangeFrom, analysis.RangeTo, analysis.RestrictDomain);
            analysis.FormulaLatex = t.FormulaLatex;
            analysis.Domain = t.Domain;
            analysis.ValueRange = t.ValueRange;
            analysis.Parity = t.Parity;
            analysis.AnalysisVersion = 2;
            analysis.Zeros = t.Zeros;
            analysis.Positivity = t.Positivity;
            analysis.Derivative = t.Derivative;
            analysis.DerivativeLatex = t.DerivativeLatex;
            analysis.CriticalPoints = t.CriticalPoints;
            analysis.Extrema = t.Extrema;
            analysis.Monotonicity = t.Monotonicity;
            analysis.SecondDerivative = t.SecondDerivative;
            analysis.SecondDerivativeLatex = t.SecondDerivativeLatex;
            analysis.InflectionPoints = t.InflectionPoints;
            analysis.Concavity = t.Concavity;
            analysis.IsNumerical = t.IsNumerical;
        }

        public object GetPlotData(string formula, double from, double to, double step, bool restrictDomain = false)
        {
            if (!double.IsFinite(step) || step < 0.0001)
            {
                throw new FormulaException("Samm peab olema vähemalt 0.0001.");
            }

            if ((to - from) / step > 20_000)
            {
                throw new FormulaException("Graafikul võib olla kuni 20 000 sammu. Suurenda sammu.");
            }

            var t = Compute(formula, from, to, restrictDomain);
            var p = GraphBuilder.Build(t, from, to, step);

            return new
            {
                formula,
                p.X,
                p.Y,
                zeros = p.Zeros,
                extrema = p.Extrema,
                inflections = p.InflectionPoints
            };
        }

        public object GetSolution(string formula, double from, double to, bool restrictDomain = false)
        {
            return new
            {
                parts = SolutionBuilder.Build(Compute(formula, from, to, restrictDomain))
            };
        }

        public static string? ValidateRange(double from, double to)
        {
            if (!double.IsFinite(from) || !double.IsFinite(to) || from < -1000 || to > 1000 || from > 1000 || to < -1000)
            {
                return "Vahemiku otspunktid peavad olema lõigus [-1000; 1000].";
            }

            return to - from < 0.01
                ? "Vahemiku algus peab olema väiksem kui lõpp ja vahemik vähemalt 0.01 pikkune."
                : null;
        }
    }
}
