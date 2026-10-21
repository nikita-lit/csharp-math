namespace csharp_math.Services.Mathematics
{
    public readonly record struct Interval(double Start, double End, bool IncludeStart, bool IncludeEnd)
    {
        public bool IsPoint => Start == End;

        public string ToText()
        {
            if (IsPoint)
            {
                return $"{{{ExactNumber.Short(Start)}}}";
            }

            return $"{(IncludeStart ? "[" : "(")}{ExactNumber.Short(Start)}; {ExactNumber.Short(End)}{(IncludeEnd ? "]" : ")")}";
        }

        public string Latex()
        {
            if (IsPoint)
            {
                return $"\\{{{ExactNumber.Latex(Start)}\\}}";
            }

            return $"{(IncludeStart ? "[" : "(")}{ExactNumber.Latex(Start)};\\ {ExactNumber.Latex(End)}{(IncludeEnd ? "]" : ")")}";
        }
    }

    /// <summary>
    /// A union of real intervals with explicit inclusion of each endpoint.
    /// </summary>
    public sealed class IntervalSet(IReadOnlyList<Interval> intervals)
    {
        public const double Tolerance = 1e-9;
        public static readonly IntervalSet RealNumbers = new([new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false)]);

        public IReadOnlyList<Interval> Intervals { get; } = intervals;

        public bool IsEmpty => Intervals.Count == 0;

        public bool Contains(double x)
        {
            return Intervals.Any(l =>
                (x > l.Start + Tolerance || (l.IncludeStart && Math.Abs(x - l.Start) <= Tolerance)) &&
                (x < l.End - Tolerance || (l.IncludeEnd && Math.Abs(x - l.End) <= Tolerance)));
        }

        public bool ContainsInterior(double x)
        {
            return Intervals.Any(l => x > l.Start + Tolerance && x < l.End - Tolerance);
        }

        public IEnumerable<double> ClosedEndpoints()
        {
            return Intervals
                .SelectMany(l => new[] { (l.Start, l.IncludeStart), (l.End, l.IncludeEnd) })
                .Where(o => o.Item2 && double.IsFinite(o.Item1))
                .Select(o => o.Item1)
                .Distinct();
        }

        public IEnumerable<double> Endpoints()
        {
            return Intervals
                .SelectMany(l => new[] { l.Start, l.End })
                .Where(double.IsFinite)
                .Distinct();
        }

        public IntervalSet Intersect(double a, double b)
        {
            var result = new List<Interval>();
            foreach (var l in Intervals)
            {
                var start = Math.Max(l.Start, a);
                var end = Math.Min(l.End, b);
                var includeStart = start == l.Start ? l.IncludeStart : true;
                var includeEnd = end == l.End ? l.IncludeEnd : true;

                if (start < end || (start == end && includeStart && includeEnd))
                {
                    result.Add(new Interval(start, end, includeStart, includeEnd));
                }
            }

            return new IntervalSet(result);
        }

        public bool IsWithin(double a, double b)
        {
            return Intervals.All(l => l.Start >= a - Tolerance && l.End <= b + Tolerance);
        }

        public string ToText()
        {
            return IsEmpty ? "∅" : string.Join(" ∪ ", Intervals.Select(l => l.ToText()));
        }

        public string Latex()
        {
            return IsEmpty ? "\\varnothing" : string.Join(" \\cup ", Intervals.Select(l => l.Latex()));
        }
    }
}
