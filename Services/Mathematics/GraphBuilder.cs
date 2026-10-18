using csharp_math.Models.Mathematics;

namespace csharp_math.Services.Mathematics;
/// <summary>Samples the function and both derivatives, inserting breaks at undefined points.</summary>
public static class GraphBuilder
{
    public static GraphData Build(AnalysisResult t, double start, double end, double step)
    {
        var n = (int)Math.Floor((end - start) / step + 1e-9);
        var xs = Enumerable.Range(0, n + 1).Select(i => Math.Round(start + i * step, 10)).ToList();
        if (xs[^1] < end - 1e-12)
            xs.Add(end);
        // Insert discontinuities even if the sampling grid skips them. Each curve uses its own domain.
        var discontinuities = t.Discontinuities.Where(k => k > start && k < end).ToList();
        var read = xs.Where(v => !discontinuities.Any(k => Math.Abs(k - v) < 1e-9)).Concat(discontinuities).Order().ToList();
        bool BreaksAt(double x, IntervalSet domain) => discontinuities.Contains(x) && !domain.Contains(x);
        var x = read.ToArray();
        var y = new double?[x.Length];
        var y1 = new double?[x.Length];
        var y2 = new double?[x.Length];
        for (var i = 0; i < x.Length; i++)
        {
            y[i] = BreaksAt(x[i], t.FunctionDomain) ? null : Value(t.Evaluate(x[i]));
            y1[i] = BreaksAt(x[i], t.DerivativeDomain) ? null : Value(t.EvaluateDerivative(x[i]));
            y2[i] = BreaksAt(x[i], t.SecondDerivativeDomain) ? null : Value(t.EvaluateSecondDerivative(x[i]));
        }

        bool WithinRange(double v) => v >= start - 1e-9 && v <= end + 1e-9;
        return new GraphData(x, y, y1, y2, t.ZeroPoints.Where(p => WithinRange(p.X)).ToList(), t.ExtremumPoints.Where(p => WithinRange(p.X)).ToList(), t.InflectionPointCoordinates.Where(p => WithinRange(p.X)).ToList(), t.Asymptotes.Where(WithinRange).ToList(), t.FormulaLatex, t.DerivativeLatex, t.SecondDerivativeLatex);
    }

    private static double? Value(double v) => double.IsFinite(v) ? Math.Round(v, 10) : null;
}
