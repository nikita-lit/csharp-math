using MathNet.Symbolics;
using System.Globalization;

namespace csharp_math.Services.Mathematics
{
    public sealed record Point(double X, double Y);

    public sealed record Extremum(double X, double Y, string Kind);

    public sealed class AnalysisResult
    {
        public required string FormulaLatex { get; init; }

        public required string Domain { get; init; }

        public required string ValueRange { get; init; }

        public bool RestrictDomain { get; init; }

        public required ParityResult ParityResult { get; init; }

        public string Parity => ParityResult.Text;

        public required ValueRangeResult RangeResult { get; init; }

        public required string Zeros { get; init; }

        public required string Derivative { get; init; }

        public required string DerivativeLatex { get; init; }

        public required string CriticalPoints { get; init; }

        public required string Extrema { get; init; }

        public required string Monotonicity { get; init; }

        public required string Positivity { get; init; }

        public required string SecondDerivative { get; init; }

        public required string SecondDerivativeLatex { get; init; }

        public required string InflectionPoints { get; init; }

        public required string Concavity { get; init; }

        /// <summary>
        /// True when some results apply only to the requested numerical search range.
        /// </summary>
        public required bool IsNumerical { get; init; }

        public required IReadOnlyList<Point> ZeroPoints { get; init; }

        public required IReadOnlyList<Extremum> ExtremumPoints { get; init; }

        public required IReadOnlyList<Point> InflectionPointCoordinates { get; init; }

        public required IReadOnlyList<double> Discontinuities { get; init; }

        public required IntervalSet FunctionDomain { get; init; }

        public required IntervalSet DerivativeDomain { get; init; }

        public required IntervalSet SecondDerivativeDomain { get; init; }

        public required Func<double, double> Evaluate { get; init; }

        public required Func<double, double> EvaluateDerivative { get; init; }

        public required Func<double, double> EvaluateSecondDerivative { get; init; }

        public required IntermediateResults Intermediate { get; init; }
    }

    public sealed record IntermediateResults(
        Expression Function,
        Expression Derivative,
        Expression SecondDerivative,
        IReadOnlyList<DomainConstraint> Constraints,
        double A,
        double B,
        bool DomainComplete,
        bool ZerosComplete,
        bool DerivativeComplete,
        bool SecondDerivativeComplete,
        RootResult FunctionRoots,
        RootResult DerivativeRoots,
        RootResult SecondDerivativeRoots,
        IReadOnlyList<double> StationaryPoints,
        IReadOnlyList<double> UndefinedDerivativePoints,
        IReadOnlyList<double> CriticalPoints,
        IReadOnlyList<double> InflectionCandidates,
        IntervalSet PositivityDomain,
        IntervalSet MonotonicityDomain,
        IntervalSet ConcavityDomain,
        IReadOnlyList<double> MonotonicityBreakpoints,
        IReadOnlyList<double> ConcavityBreakpoints);

    public sealed record LatexFormulas(string Formula, string Derivative, string SecondDerivative);

    /// <summary>
    /// Computes domains, roots, derivatives, sign regions, extrema and inflection points.
    /// </summary>
    public sealed class FunctionAnalysisEngine
    {
        private const string NoneFound = "puuduvad";

        public string? Validate(string formula)
        {
            try
            {
                var f = FormulaParser.Parse(formula).Expression;
                ExpressionMath.Differentiate(ExpressionMath.Differentiate(f));
                return null;
            }
            catch (FormulaParseException error)
            {
                return error.Message;
            }
            catch (Exception)
            {
                return "Selle valemi tuletist ei õnnestunud leida – kontrolli valemi kuju.";
            }
        }

        public LatexFormulas GetLatexFormulas(string formula)
        {
            var f = FormulaParser.Parse(formula).Expression;
            var f1 = ExpressionMath.Differentiate(f);
            return new LatexFormulas(
                ExpressionFormatter.Latex(f),
                ExpressionFormatter.Latex(f1),
                ExpressionFormatter.Latex(ExpressionMath.Differentiate(f1)));
        }

        public AnalysisResult Analyze(string formula, double a, double b, bool restrictDomain = false)
        {
            var parsed = FormulaParser.Parse(formula);
            var f = parsed.Expression;
            var f1 = ExpressionMath.Differentiate(f);
            var f2 = ExpressionMath.Differentiate(f1);
            var F = ExpressionEvaluator.Compile(f);
            var F1 = ExpressionEvaluator.Compile(f1);
            var F2 = ExpressionEvaluator.Compile(f2);
            var (D, domainComplete) = DomainAnalyzer.Domain(parsed.Constraints, a, b);
            var (D1, derivativeDomainComplete) = DomainAnalyzer.Domain(ConstraintCollector.Collect(f1).All, a, b);
            var (D2, secondDerivativeDomainComplete) = DomainAnalyzer.Domain(ConstraintCollector.Collect(f2).All, a, b);

            if (restrictDomain)
            {
                D = D.Intersect(a, b);
            }

            var domainWithinRange = domainComplete && !D.IsEmpty && D.IsWithin(a, b);

            bool Complete(params bool[] parts)
            {
                return domainWithinRange || parts.All(o => o);
            }

            IntervalSet AnalysisDomain(bool complete)
            {
                return complete ? D : D.Intersect(a, b);
            }

            var j0 = RootFinder.Find(f, a, b, D.ClosedEndpoints());
            var zerosComplete = Complete(domainComplete, j0.Complete);
            var zeros = j0.Values.Where(D.Contains).ToList();
            var j1 = RootFinder.Find(f1, a, b);
            var derivativeComplete = Complete(domainComplete, derivativeDomainComplete, j1.Complete);
            var stationaryPoints = j1.Values.Where(x => D.ContainsInterior(x) && D1.Contains(x)).ToList();
            var undefinedDerivativePoints = D1.Endpoints().Where(x => D.ContainsInterior(x) && !D1.Contains(x)).ToList();
            var criticalPoints = stationaryPoints.Concat(undefinedDerivativePoints).Distinct().Order().ToList();
            var extrema = new List<Extremum>();
            var allBreakpoints = criticalPoints.Concat(D.Endpoints()).Concat(D1.Endpoints()).ToList();

            foreach (var c in criticalPoints)
            {
                var delta = Delta(c, allBreakpoints);
                var (left, right) = (F1(c - delta), F1(c + delta));
                var y = ExactNumber.Snap(F(c));
                if (double.IsNaN(y))
                {
                    continue;
                }

                if (left > 0 && right < 0)
                {
                    extrema.Add(new Extremum(c, y, "max"));
                }
                else if (left < 0 && right > 0)
                {
                    extrema.Add(new Extremum(c, y, "min"));
                }
            }

            // On a prescribed closed interval, also compare the endpoint values.
            if (restrictDomain)
            {
                foreach (var endpoint in D.ClosedEndpoints())
                {
                    var y = ExactNumber.Snap(F(endpoint));
                    var isLeft = D.Intervals.Any(i => i.Start == endpoint);
                    var nearby = F(endpoint + (isLeft ? 1 : -1) * Delta(endpoint, allBreakpoints));
                    if (double.IsFinite(y) && double.IsFinite(nearby) && Math.Abs(y - nearby) > 1e-12)
                    {
                        extrema.Add(new Extremum(endpoint, y, y < nearby ? "min" : "max"));
                    }
                }

                extrema.Sort((u, v) => u.X.CompareTo(v.X));
            }

            var monotonicityBreakpoints = j1.Values.Concat(D1.Endpoints()).ToList();
            var (increasing, decreasing) = DomainAnalyzer.SignRegions(F1, AnalysisDomain(derivativeComplete), monotonicityBreakpoints, merge: true);
            var (positive, negativeRegions) = DomainAnalyzer.SignRegions(F, AnalysisDomain(zerosComplete), j0.Values, merge: false, includeEndpoints: true);
            var j2 = RootFinder.Find(f2, a, b);
            var secondDerivativeComplete = Complete(domainComplete, secondDerivativeDomainComplete, j2.Complete);
            var inflectionCandidates = j2.Values.Concat(D2.Endpoints()).Where(D.ContainsInterior).Distinct().Order().ToList();
            var inflectionPoints = new List<Point>();

            foreach (var k in inflectionCandidates)
            {
                var delta = Delta(k, inflectionCandidates.Concat(D.Endpoints()));
                var (left, right) = (F2(k - delta), F2(k + delta));
                var y = ExactNumber.Snap(F(k));
                if (!double.IsNaN(y) && left * right < 0)
                {
                    inflectionPoints.Add(new Point(k, y));
                }
            }

            var concavityBreakpoints = j2.Values.Concat(D2.Endpoints()).ToList();
            var (concaveUp, concaveDown) = DomainAnalyzer.SignRegions(F2, AnalysisDomain(secondDerivativeComplete), concavityBreakpoints, merge: true, preventMergeAt: D1.Endpoints());
            var range = $" (vahemikus [{ExactNumber.Decimal(a)}; {ExactNumber.Decimal(b)}])";

            string ScopeNote(bool complete)
            {
                return complete ? "" : range;
            }

            var parity = FunctionProperties.Parity(f, D);
            var valueRange = FunctionProperties.ValueRange(f, D, criticalPoints, domainComplete && derivativeComplete);

            return new AnalysisResult
            {
                FormulaLatex = ExpressionFormatter.Latex(f),
                Domain = D.ToText() + ScopeNote(domainComplete),
                ValueRange = valueRange.Text,
                RestrictDomain = restrictDomain,
                RangeResult = valueRange,
                ParityResult = parity,
                Zeros = j0.AllPoints ? "kõik x ∈ X" : FormatList(zeros, "x") + ScopeNote(zerosComplete),
                Derivative = ExpressionFormatter.ToText(f1),
                DerivativeLatex = ExpressionFormatter.Latex(f1),
                CriticalPoints = j1.AllPoints ? "kõik x ∈ X (funktsioon on konstantne)" : FormatCriticalPoints(criticalPoints, undefinedDerivativePoints) + ScopeNote(derivativeComplete),
                Extrema = extrema.Count == 0 ? NoneFound + ScopeNote(derivativeComplete) : string.Join("; ", extrema.Select(e => ExactNumber.Equality($"{e.Kind} f({ExactNumber.Short(e.X)})", e.Y))) + ScopeNote(derivativeComplete),
                Monotonicity = $"X↑ = {increasing.ToText()}; X↓ = {decreasing.ToText()}" + ScopeNote(derivativeComplete),
                Positivity = $"X⁺ = {positive.ToText()}; X⁻ = {negativeRegions.ToText()}" + ScopeNote(zerosComplete),
                SecondDerivative = ExpressionFormatter.ToText(f2),
                SecondDerivativeLatex = ExpressionFormatter.Latex(f2),
                InflectionPoints = (inflectionPoints.Count == 0 ? NoneFound : string.Join("; ", inflectionPoints.Select((p, i) => $"K{(inflectionPoints.Count > 1 ? Subscript(i + 1) : "")}({ExactNumber.Short(p.X)}; {ExactNumber.Short(p.Y)})"))) + ScopeNote(secondDerivativeComplete),
                Concavity = $"X∪ = {concaveUp.ToText()}; X∩ = {concaveDown.ToText()}" + ScopeNote(secondDerivativeComplete),
                IsNumerical = !(domainComplete && zerosComplete && derivativeComplete && secondDerivativeComplete && valueRange.Complete),
                ZeroPoints = zeros.Select(x => new Point(x, 0)).ToList(),
                ExtremumPoints = extrema,
                InflectionPointCoordinates = inflectionPoints,
                Discontinuities = Uniq(D.Endpoints().Concat(D1.Endpoints()).Concat(D2.Endpoints()).Where(p => !D.ContainsInterior(p) || !D1.Contains(p) || !D2.Contains(p))),
                FunctionDomain = D,
                DerivativeDomain = D1,
                SecondDerivativeDomain = D2,
                Evaluate = F,
                EvaluateDerivative = F1,
                EvaluateSecondDerivative = F2,
                Intermediate = new IntermediateResults(f, f1, f2, parsed.Constraints, a, b, domainComplete, zerosComplete, derivativeComplete, secondDerivativeComplete, j0, j1, j2, stationaryPoints, undefinedDerivativePoints, criticalPoints, inflectionCandidates, AnalysisDomain(zerosComplete), AnalysisDomain(derivativeComplete), AnalysisDomain(secondDerivativeComplete), monotonicityBreakpoints, concavityBreakpoints)
            };
        }

        private static double Delta(double c, IEnumerable<double> otherPoints)
        {
            var nearest = otherPoints.Where(p => Math.Abs(p - c) > IntervalSet.Tolerance).Select(p => Math.Abs(p - c)).DefaultIfEmpty(1).Min();
            return Math.Min(1e-4, nearest * 0.4);
        }

        private static List<double> Uniq(IEnumerable<double> points)
        {
            var result = new List<double>();
            foreach (var p in points.Order())
            {
                if (result.Count == 0 || p - result[^1] > IntervalSet.Tolerance)
                {
                    result.Add(p);
                }
            }

            return result;
        }

        private static string FormatList(IReadOnlyList<double> values, string name)
        {
            return values.Count == 0
                ? NoneFound
                : string.Join("; ", values.Select((v, i) => ExactNumber.Equality(values.Count > 1 ? name + Subscript(i + 1) : name, v)));
        }

        private static string FormatCriticalPoints(IReadOnlyList<double> criticalPoints, IReadOnlyList<double> undefinedDerivativePoints)
        {
            if (criticalPoints.Count == 0)
            {
                return NoneFound;
            }

            return string.Join("; ", criticalPoints.Select((v, i) => ExactNumber.Equality(criticalPoints.Count > 1 ? "x" + Subscript(i + 1) : "x", v) + (undefinedDerivativePoints.Any(t => Math.Abs(t - v) <= IntervalSet.Tolerance) ? " (f' puudub)" : "")));
        }

        private static string Subscript(int n)
        {
            return string.Concat(n.ToString(CultureInfo.InvariantCulture).Select(c => (char)('₀' + (c - '0'))));
        }
    }
}
