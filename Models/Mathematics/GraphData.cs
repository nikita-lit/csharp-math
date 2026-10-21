using csharp_math.Services.Mathematics;

namespace csharp_math.Models.Mathematics
{
    public sealed record GraphData(
        double[] X,
        double?[] Y,
        IReadOnlyList<Point> Zeros,
        IReadOnlyList<Extremum> Extrema,
        IReadOnlyList<Point> InflectionPoints);
}
