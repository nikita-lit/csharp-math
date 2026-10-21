using MathNet.Symbolics;

namespace csharp_math.Services.Mathematics
{
    public sealed record ParityResult(string Text, string Formula, string Explanation);

    public sealed record ValueRangeResult(string Text, IReadOnlyList<string> Steps, bool Complete, IntervalSet? Values = null);

    public static class FunctionProperties
    {
        public static ParityResult Parity(Expression f, IntervalSet domain)
        {
            var reflectedDomain = domain.Intervals.Reverse().Select(i => new Interval(-i.End, -i.Start, i.IncludeEnd, i.IncludeStart)).ToList();
            if (!domain.Intervals.SequenceEqual(reflectedDomain))
            {
                return new("ei ole paaris ega paaritu", "X \\ne -X", "Määramispiirkond ei ole nulli suhtes sümmeetriline.");
            }

            var reflected = ExpressionMath.NormalizePowers(f, reflect: true);
            var normalized = ExpressionMath.NormalizePowers(f);
            var same = ExpressionMath.Simplify(Operators.subtract(reflected, normalized)).Equals(Expression.Zero);
            var opposite = ExpressionMath.Simplify(Operators.add(reflected, normalized)).Equals(Expression.Zero);

            return (same, opposite) switch
            {
                (true, true) => new("paaris ja paaritu (nullfunktsioon)", "f(-x)=f(x)=-f(x)=0", "Nullfunktsioon on nii paaris kui ka paaritu."),
                (true, _) => new("paaris", "f(-x)=f(x)", "Määramispiirkond on sümmeetriline ja f(-x) = f(x)."),
                (_, true) => new("paaritu", "f(-x)=-f(x)", "Määramispiirkond on sümmeetriline ja f(-x) = -f(x)."),
                _ => new("ei ole paaris ega paaritu", $"f(-x)={ExpressionFormatter.Latex(reflected)}", "Avaldis ei võrdu ei f(x) ega −f(x).")
            };
        }

        public static ValueRangeResult ValueRange(Expression f, IntervalSet domain, IReadOnlyList<double> criticalPoints, bool complete)
        {
            if (domain.IsEmpty)
            {
                return new("∅", ["Määramispiirkond on tühi."], true);
            }

            if (!complete)
            {
                return new("ei ole täielikult leitud", ["Kogu muutumispiirkonna leidmiseks tuleb leida kõik kriitilised punktid ja piirväärtused."], false);
            }

            var evaluate = ExpressionEvaluator.Compile(f);
            var ranges = new List<Interval>();
            var steps = new List<string>();

            foreach (var interval in domain.Intervals)
            {
                var candidates = new List<(double Value, bool Included)>();
                foreach (var x in criticalPoints.Where(x => x > interval.Start && x < interval.End).Concat(interval.IsPoint ? [interval.Start] : Array.Empty<double>()))
                {
                    var y = ExactNumber.Snap(evaluate(x));
                    if (double.IsFinite(y))
                    {
                        candidates.Add((y, true));
                        steps.Add($"f({ExactNumber.Short(x)}) = {ExactNumber.Short(y)}");
                    }
                }

                foreach (var (x, included, side) in new[] { (interval.Start, interval.IncludeStart, 1), (interval.End, interval.IncludeEnd, -1) })
                {
                    var value = included ? evaluate(x) : EndBehavior.Limit(f, x, side);
                    if (value == null || double.IsNaN(value.Value))
                    {
                        return new("ei ole täielikult leitud", [.. steps, "Ühe piirväärtuse leidmine ei õnnestunud."], false);
                    }

                    var y = double.IsFinite(value.Value) ? ExactNumber.Snap(value.Value) : value.Value;
                    candidates.Add((y, included && double.IsFinite(y)));
                    steps.Add(included
                        ? $"f({ExactNumber.Short(x)}) = {ExactNumber.Short(y)}"
                        : $"Kui x → {ExactNumber.Short(x)}{(double.IsFinite(x) ? side > 0 ? "+" : "−" : "")}, siis f(x) → {ExactNumber.Short(y)}.");
                }

                if (!interval.IsPoint)
                {
                    var middleValue = evaluate(DomainAnalyzer.TestPoint(interval.Start, interval.End));
                    if (double.IsFinite(middleValue))
                    {
                        candidates.Add((ExactNumber.Snap(middleValue), true));
                    }
                }

                var min = candidates.Min(p => p.Value);
                var max = candidates.Max(p => p.Value);
                ranges.Add(new(min, max, candidates.Any(p => Near(p.Value, min) && p.Included), candidates.Any(p => Near(p.Value, max) && p.Included)));
            }

            var merged = new List<Interval>();
            foreach (var interval in ranges.OrderBy(i => i.Start))
            {
                if (merged.Count == 0 || interval.Start > merged[^1].End + 1e-9 || (Near(interval.Start, merged[^1].End) && !interval.IncludeStart && !merged[^1].IncludeEnd))
                {
                    merged.Add(interval);
                }
                else
                {
                    var previous = merged[^1];
                    var end = Math.Max(previous.End, interval.End);
                    merged[^1] = new(
                        previous.Start,
                        end,
                        previous.IncludeStart || (Near(previous.Start, interval.Start) && interval.IncludeStart),
                        (Near(end, previous.End) && previous.IncludeEnd) || (Near(end, interval.End) && interval.IncludeEnd));
                }
            }

            var set = new IntervalSet(merged);
            var approximate = merged.SelectMany(i => new[] { i.Start, i.End }).Any(v => double.IsFinite(v) && !ExactNumber.HasExactForm(v));
            return new(set.ToText() + (approximate ? " (ligikaudu)" : ""), steps.Distinct().ToList(), true, set);
        }

        private static bool Near(double a, double b)
        {
            return a == b || Math.Abs(a - b) < 1e-9;
        }
    }
}
