using csharp_math.Services.Mathematics;

namespace csharp_math.Models.Mathematics;

public sealed record GraphData(double[] X, double?[] Y, double?[] DerivativeY, double?[] SecondDerivativeY, IReadOnlyList<Point> Zeros, IReadOnlyList<Extremum> Extrema, IReadOnlyList<Point> InflectionPoints, IReadOnlyList<double> Asymptotes, string FormulaLatex, string DerivativeLatex, string SecondDerivativeLatex);
