namespace csharp_math.Services.Mathematics
{
    /// <summary>
    /// Builds domains and sign regions by testing intervals and their boundary points.
    /// </summary>
    public static class DomainAnalyzer
    {
        public static (IntervalSet set, bool complete) Domain(IReadOnlyList<DomainConstraint> constraints, double a, double b)
        {
            if (constraints.Count == 0)
            {
                return (IntervalSet.RealNumbers, true);
            }

            var complete = true;
            var constraintRoots = new List<(DomainConstraint constraint, RootResult roots)>();
            foreach (var t in constraints)
            {
                var roots = RootFinder.Find(t.Expression, a, b);
                if (roots.AllPoints)
                {
                    if (t.Kind == ConstraintKind.NonNegative)
                    {
                        continue;
                    }

                    return (new IntervalSet([]), true);
                }

                complete &= roots.Complete;
                constraintRoots.Add((t, roots));
            }

            var left = complete ? double.NegativeInfinity : a;
            var right = complete ? double.PositiveInfinity : b;
            var breakpoints = SortedUnique(constraintRoots.SelectMany(j => j.roots.Values))
                .Where(p => p > left + IntervalSet.Tolerance && p < right - IntervalSet.Tolerance)
                .ToList();

            bool IsPointValid(double p)
            {
                return constraintRoots.All(j => j.roots.Values.Any(r => Math.Abs(r - p) <= IntervalSet.Tolerance)
                    ? j.constraint.Kind == ConstraintKind.NonNegative
                    : j.constraint.IsSatisfied(p));
            }

            bool IsIntervalValid(double u, double v)
            {
                var t = TestPoint(u, v);
                return constraintRoots.All(j => j.constraint.IsSatisfied(t));
            }

            var pieces = new List<(double u, double v, bool point, bool isValid)>();
            if (!complete)
            {
                pieces.Add((a, a, true, IsPointValid(a)));
            }

            var boundaries = new List<double>
            {
                left
            };
            boundaries.AddRange(breakpoints);
            boundaries.Add(right);

            for (var i = 0; i < boundaries.Count - 1; i++)
            {
                if (i > 0)
                {
                    pieces.Add((boundaries[i], boundaries[i], true, IsPointValid(boundaries[i])));
                }

                pieces.Add((boundaries[i], boundaries[i + 1], false, IsIntervalValid(boundaries[i], boundaries[i + 1])));
            }

            if (!complete)
            {
                pieces.Add((b, b, true, IsPointValid(b)));
            }

            var intervals = new List<Interval>();
            Interval? current = null;
            foreach (var (u, v, point, isValid) in pieces)
            {
                if (!isValid)
                {
                    if (current.HasValue)
                    {
                        intervals.Add(current.Value);
                    }

                    current = null;
                    continue;
                }

                current = current.HasValue
                    ? current.Value with { End = v, IncludeEnd = point }
                    : new Interval(u, v, point, point);
            }

            if (current.HasValue)
            {
                intervals.Add(current.Value);
            }

            return (new IntervalSet(intervals), complete);
        }

        public static (IntervalSet positive, IntervalSet negativeRegions) SignRegions(
            Func<double, double> g,
            IntervalSet domain,
            IEnumerable<double> breakpoints,
            bool merge,
            IEnumerable<double>? preventMergeAt = null,
            bool includeEndpoints = false)
        {
            var points = SortedUnique(breakpoints);
            var separators = SortedUnique(preventMergeAt ?? []);
            var positive = new List<Interval>();
            var negativeRegions = new List<Interval>();

            foreach (var interval in domain.Intervals.Where(l => !l.IsPoint))
            {
                var boundaries = new List<double>
                {
                    interval.Start
                };
                boundaries.AddRange(points.Where(p => p > interval.Start + IntervalSet.Tolerance && p < interval.End - IntervalSet.Tolerance));
                boundaries.Add(interval.End);

                var parts = new List<(double u, double v, int sign)>();
                for (var i = 0; i < boundaries.Count - 1; i++)
                {
                    var y = g(TestPoint(boundaries[i], boundaries[i + 1]));
                    var sign = double.IsNaN(y) || y == 0 ? 0 : Math.Sign(y);
                    var separates = separators.Any(e => Math.Abs(e - boundaries[i]) <= IntervalSet.Tolerance);

                    if (merge && !separates && parts.Count > 0 && parts[^1].sign == sign && sign != 0)
                    {
                        parts[^1] = parts[^1] with { v = boundaries[i + 1] };
                    }
                    else
                    {
                        parts.Add((boundaries[i], boundaries[i + 1], sign));
                    }
                }

                bool Included(double x, bool positiveSign)
                {
                    return includeEndpoints && double.IsFinite(x) && domain.Contains(x)
                        && !points.Any(p => Math.Abs(p - x) <= IntervalSet.Tolerance)
                        && (positiveSign ? g(x) > 0 : g(x) < 0);
                }

                foreach (var (u, v, sign) in parts)
                {
                    if (sign > 0)
                    {
                        positive.Add(new Interval(u, v, Included(u, true), Included(v, true)));
                    }
                    else if (sign < 0)
                    {
                        negativeRegions.Add(new Interval(u, v, Included(u, false), Included(v, false)));
                    }
                }
            }

            return (new IntervalSet(positive), new IntervalSet(negativeRegions));
        }

        public static double TestPoint(double u, double v)
        {
            if (double.IsNegativeInfinity(u) && double.IsPositiveInfinity(v))
            {
                return 0;
            }

            if (double.IsNegativeInfinity(u))
            {
                return v - 1;
            }

            if (double.IsPositiveInfinity(v))
            {
                return u + 1;
            }

            return (u + v) / 2;
        }

        public static List<double> SortedUnique(IEnumerable<double> points)
        {
            var result = new List<double>();
            foreach (var p in points.Where(double.IsFinite).Order())
            {
                if (result.Count == 0 || p - result[^1] > IntervalSet.Tolerance)
                {
                    result.Add(p);
                }
            }

            return result;
        }
    }
}
