namespace csharp_math.Services.Mathematics
{
    public sealed record SolutionStep(string? Text = null, string? Formula = null, SignTable? Table = null);

    public sealed record SignTable(string SignLabel, string? MeaningLabel, IReadOnlyList<SignTableColumn> Columns);

    public sealed record SignTableColumn(string X, bool IsInterval, string Sign, string? Meaning, string? Check);

    public sealed record SolutionSection(string Key, string Title, IReadOnlyList<SolutionStep> Steps, IReadOnlyList<string> Answers, bool IsNumerical);

    /// <summary>
    /// Builds solution steps, equations and sign tables from intermediate analysis results.
    /// </summary>
    public static class SolutionBuilder
    {
        public static List<SolutionSection> Build(AnalysisResult t)
        {
            var v = t.Intermediate;
            return
            [
                Domain(t, v),
                new("parity", "Paarsus", [new(t.ParityResult.Explanation, t.ParityResult.Formula)], [t.Parity], false),
                Zeros(t, v),
                Positivity(t, v),
                new("derivative", "Esimene tuletis", DifferentiationExplainer.Steps(v.Function, "f", "f'"), [t.Derivative], false),
                CriticalPoints(t, v),
                Monotonicity(t, v),
                Extrema(t, v),
                new("secondDerivative", "Teine tuletis", DifferentiationExplainer.Steps(v.Derivative, "f'", "f''"), [t.SecondDerivative], false),
                Concavity(t, v),
                InflectionPoints(t, v)
            ];
        }

        private static SolutionSection Domain(AnalysisResult t, IntermediateResults v)
        {
            var s = new List<SolutionStep>();
            if (v.Constraints.Count == 0)
            {
                s.Add(new("Piiranguid pole (nimetaja, paarisjuur, logaritm puuduvad):", "X = \\mathbb{R}"));
            }
            else
            {
                foreach (var constraint in v.Constraints)
                {
                    var (set, _) = DomainAnalyzer.Domain([constraint], v.A, v.B);
                    s.Add(new($"{Capitalize(constraint.Reason)}:", $"{L(constraint.Expression)} {Comparison(constraint.Kind)} 0 \\;\\Rightarrow\\; x \\in{SetLatex(set)}"));
                }

                s.Add(new(v.Constraints.Count > 1 ? "Ühisosa:" : null, $"X = {SetLatex(t.FunctionDomain)}"));
            }

            if (!v.DomainComplete)
            {
                s.Add(new($"Leitud numbriliselt lõigul {Interval(v.A, v.B)}."));
            }

            if (t.RestrictDomain)
            {
                s.Add(new("Valitud lõik piirab määramispiirkonda:", $"X = X_f \\cap [{ExactNumber.Latex(v.A)}; {ExactNumber.Latex(v.B)}] = {SetLatex(t.FunctionDomain)}"));
            }

            s.Add(new("Muutumispiirkonna leidmiseks võrdleme kriitiliste punktide ja otspunktide väärtusi ning avatud piiride piirväärtusi."));
            s.AddRange(t.RangeResult.Steps.Select(text => new SolutionStep(text)));
            return new("domain", "Määramispiirkond ja muutumispiirkond", s, [$"X = {t.Domain}", $"Y = {t.ValueRange}"], !v.DomainComplete || !t.RangeResult.Complete);
        }

        private static SolutionSection Zeros(AnalysisResult t, IntermediateResults v)
        {
            var s = EquationExplainer.Steps(v.Function, "f(x)", v.FunctionRoots, v.A, v.B);
            var excluded = v.FunctionRoots.Values.Where(x => !t.FunctionDomain.Contains(x)).ToList();
            if (excluded.Count > 0)
            {
                s.Add(new($"${EquationExplainer.FormatList(excluded)} \\notin X$ – ei sobi."));
            }

            var valid = v.FunctionRoots.Values.Where(t.FunctionDomain.Contains).Where(ExactNumber.HasExactForm).Take(3).ToList();
            if (valid.Count > 0 && !v.FunctionRoots.AllPoints)
            {
                s.Add(new("Kontroll:", string.Join(", \\quad ", valid.Select(x => Value(v.Function, x, 0)))));
            }

            return new("zeros", "Nullkohad", s, [t.Zeros], !v.ZerosComplete);
        }

        private static SolutionSection Positivity(AnalysisResult t, IntermediateResults v)
        {
            var s = new List<SolutionStep>
            {
                new("Märgitabel (testpunkt igas vahemikus):", null, Table(t.Evaluate, "f", v.PositivityDomain, v.FunctionRoots.Values, null, sign => null, _ => null, RangeBoundaries(v, v.ZerosComplete))),
            };

            return new("positivity", "Positiivsus- ja negatiivsuspiirkond", s, [t.Positivity], !v.ZerosComplete);
        }

        private static SolutionSection CriticalPoints(AnalysisResult t, IntermediateResults v)
        {
            var s = EquationExplainer.Steps(v.Derivative, "f'(x)", v.DerivativeRoots, v.A, v.B);
            var excluded = v.DerivativeRoots.Values.Where(x => !v.StationaryPoints.Contains(x)).ToList();
            if (excluded.Count > 0)
            {
                s.Add(new($"${EquationExplainer.FormatList(excluded)}$ ei ole $X$ sisepunkt – ei sobi."));
            }

            if (v.UndefinedDerivativePoints.Count > 0)
            {
                s.Add(new("$f'$ ei ole määratud (kuid $f$ on):", EquationExplainer.FormatList(v.UndefinedDerivativePoints)));
            }

            if (v.CriticalPoints.Count > 0 && (excluded.Count > 0 || v.UndefinedDerivativePoints.Count > 0))
            {
                s.Add(new("Kriitilised punktid:", EquationExplainer.FormatList(v.CriticalPoints)));
            }

            return new("criticalPoints", "Kriitilised punktid", s, [t.CriticalPoints], !v.DerivativeComplete);
        }

        private static SolutionSection Monotonicity(AnalysisResult t, IntermediateResults v)
        {
            var table = Table(t.EvaluateDerivative, "f'", v.MonotonicityDomain, v.MonotonicityBreakpoints, "f(x)", sign => sign switch
            {
                "+" => "\\nearrow",
                "−" => "\\searrow",
                "0" => "\\rightarrow",
                _ => null
            }, x =>
            {
                var extremum = t.ExtremumPoints.FirstOrDefault(e => Math.Abs(e.X - x) <= IntervalSet.Tolerance);
                return extremum != null ? $"\\text{{{extremum.Kind}}}" : null;
            }, RangeBoundaries(v, v.DerivativeComplete));

            var s = new List<SolutionStep>
            {
                new("$f'$ märgid ($+ \\Rightarrow \\nearrow$, $- \\Rightarrow \\searrow$):", null, table)
            };

            if (IsComposite(table))
            {
                s.Add(new("Sama märgiga naabervahemikud ühendatakse ($f'$ märk ei muutu)."));
            }

            return new("monotonicity", "Kasvamis- ja kahanemispiirkond", s, [t.Monotonicity], !v.DerivativeComplete);
        }

        private static SolutionSection Extrema(AnalysisResult t, IntermediateResults v)
        {
            var s = new List<SolutionStep>
            {
                new("Maksimumis muutub f′ märk plussist miinuseks, miinimumis miinusest plussiks.")
            };

            foreach (var c in v.CriticalPoints)
            {
                var e = t.ExtremumPoints.FirstOrDefault(p => Math.Abs(p.X - c) <= IntervalSet.Tolerance);
                if (e == null)
                {
                    s.Add(new($"$x = {ExactNumber.Latex(c)}$: $f'$ märk ei muutu – ekstreemumit pole."));
                }
                else
                {
                    s.Add(new(e.Kind == "max" ? "$+ \\to -$:" : "$- \\to +$:", $"y_{{\\text{{{e.Kind}}}}} = {Value(v.Function, c, e.Y)}"));
                }
            }

            if (t.RestrictDomain)
            {
                foreach (var e in t.ExtremumPoints.Where(e => !v.CriticalPoints.Contains(e.X)))
                {
                    s.Add(new("Lõigu otspunkti võrdleme lõigu sees asuvate väärtustega:", $"{(e.Kind == "max" ? "\\max" : "\\min")} f({ExactNumber.Latex(e.X)}) = {ExactNumber.Latex(e.Y)}"));
                }
            }

            return new("extrema", "Ekstreemumid", s, [t.Extrema], !v.DerivativeComplete);
        }

        private static SolutionSection Concavity(AnalysisResult t, IntermediateResults v)
        {
            var table = Table(t.EvaluateSecondDerivative, "f''", v.ConcavityDomain, v.ConcavityBreakpoints, "f(x)", sign => sign switch
            {
                "+" => "\\cup",
                "−" => "\\cap",
                _ => "—"
            }, x => t.InflectionPointCoordinates.Any(k => Math.Abs(k.X - x) <= IntervalSet.Tolerance) ? "\\text{K}" : null, RangeBoundaries(v, v.SecondDerivativeComplete));

            return new("concavity", "Kumerus ja nõgusus", [new("f″ > 0: nõgus (∪); f″ < 0: kumer (∩).", null, table)], [t.Concavity], !v.SecondDerivativeComplete);
        }

        private static SolutionSection InflectionPoints(AnalysisResult t, IntermediateResults v)
        {
            var s = EquationExplainer.Steps(v.SecondDerivative, "f''(x)", v.SecondDerivativeRoots, v.A, v.B);
            var undefined = v.InflectionCandidates.Where(k => !v.SecondDerivativeRoots.Values.Any(j => Math.Abs(j - k) <= IntervalSet.Tolerance)).ToList();
            if (undefined.Count > 0)
            {
                s.Add(new("$f''$ ei ole määratud:", EquationExplainer.FormatList(undefined)));
            }

            foreach (var k in v.InflectionCandidates)
            {
                var kp = t.InflectionPointCoordinates.FirstOrDefault(p => Math.Abs(p.X - k) <= IntervalSet.Tolerance);
                if (kp == null)
                {
                    s.Add(new($"$x = {ExactNumber.Latex(k)}$: $f''$ märk ei muutu – käänupunkti pole."));
                }
                else
                {
                    s.Add(new("Käänupunkt:", $"y_{{K}} = {Value(v.Function, k, kp.Y)}"));
                }
            }

            return new("inflections", "Käänupunktid", s, [t.InflectionPoints], !v.SecondDerivativeComplete);
        }

        private static SignTable Table(
            Func<double, double> g,
            string symbol,
            IntervalSet domain,
            IEnumerable<double> breakpoints,
            string? meaningLabel,
            Func<string, string?> intervalMeaning,
            Func<double, string?> pointMeaning,
            double[] rangeBoundaries)
        {
            bool IsRangeBoundary(double x)
            {
                return rangeBoundaries.Any(p => Math.Abs(p - x) <= IntervalSet.Tolerance);
            }

            var points = DomainAnalyzer.SortedUnique(breakpoints);
            var columns = new List<SignTableColumn>();

            void Point(double x)
            {
                if (columns.Count > 0 && !columns[^1].IsInterval && columns[^1].X == ExactNumber.Latex(x))
                {
                    return;
                }

                var y = g(x);
                var sign = double.IsNaN(y) ? "∄" : Math.Abs(y) < 1e-9 ? "0" : y > 0 ? "+" : "−";
                columns.Add(new(ExactNumber.Latex(x), false, sign, pointMeaning(x), null));
            }

            void Undefined(double x)
            {
                if (columns.Count > 0 && !columns[^1].IsInterval && columns[^1].X == ExactNumber.Latex(x))
                {
                    return;
                }

                columns.Add(new(ExactNumber.Latex(x), false, "∄", null, null));
            }

            void IsInterval(double u, double w)
            {
                var x = TestPoint(u, w, g);
                var y = g(x);
                var sign = double.IsNaN(y) ? "∄" : Math.Abs(y) < 1e-12 ? "0" : y > 0 ? "+" : "−";
                var check = double.IsNaN(y) ? null : ExactNumber.EqualityLatex($"{symbol}({ExactNumber.Latex(x)})", ExactNumber.Snap(y));
                columns.Add(new(new Interval(u, w, false, false).Latex(), true, sign, intervalMeaning(sign), check));
            }

            var intervals = domain.Intervals;
            for (var i = 0; i < intervals.Count; i++)
            {
                var l = intervals[i];
                if (l.IsPoint)
                {
                    Point(l.Start);
                    continue;
                }

                if (double.IsFinite(l.Start) && !IsRangeBoundary(l.Start))
                {
                    if (l.IncludeStart)
                    {
                        Point(l.Start);
                    }
                    else
                    {
                        Undefined(l.Start);
                    }
                }

                var boundaries = new List<double>
                {
                    l.Start
                };
                boundaries.AddRange(points.Where(p => p > l.Start + IntervalSet.Tolerance && p < l.End - IntervalSet.Tolerance));
                boundaries.Add(l.End);

                for (var j = 0; j < boundaries.Count - 1; j++)
                {
                    if (j > 0)
                    {
                        Point(boundaries[j]);
                    }

                    IsInterval(boundaries[j], boundaries[j + 1]);
                }

                if (double.IsFinite(l.End) && !IsRangeBoundary(l.End))
                {
                    if (l.IncludeEnd)
                    {
                        Point(l.End);
                    }
                    else
                    {
                        Undefined(l.End);
                    }
                }

                if (i < intervals.Count - 1 && intervals[i + 1].Start - l.End > IntervalSet.Tolerance)
                {
                    columns.Add(new(new Interval(l.End, intervals[i + 1].Start, false, false).Latex(), true, "∄", "\\notin X", null));
                }
            }

            return new SignTable($"{symbol}(x)", meaningLabel, columns);
        }

        private static double TestPoint(double u, double v, Func<double, double> g)
        {
            static bool Inner(double x, double u, double v) => x > u + 1e-6 && x < v - 1e-6;

            double choice;
            if (double.IsNegativeInfinity(u) && double.IsPositiveInfinity(v))
            {
                choice = 0;
            }
            else if (double.IsNegativeInfinity(u))
            {
                choice = Math.Ceiling(v) - 1 >= v - 1e-6 ? Math.Ceiling(v) - 2 : Math.Ceiling(v) - 1;
            }
            else if (double.IsPositiveInfinity(v))
            {
                choice = Math.Floor(u) + 1 <= u + 1e-6 ? Math.Floor(u) + 2 : Math.Floor(u) + 1;
            }
            else
            {
                var middle = (u + v) / 2;
                choice = new[]
                {
                    0,
                    Math.Round(middle),
                    Math.Round(middle * 2) / 2,
                    Math.Round(middle * 10) / 10,
                    Math.Round(middle * 100) / 100
                }.FirstOrDefault(x => Inner(x, u, v), middle);
            }

            return double.IsNaN(g(choice)) ? DomainAnalyzer.TestPoint(u, v) : choice + 0.0;
        }

        private static double[] RangeBoundaries(IntermediateResults v, bool complete)
        {
            return complete ? [] : [v.A, v.B];
        }

        private static bool IsComposite(SignTable table)
        {
            var columns = table.Columns;
            for (var i = 0; i + 2 < columns.Count; i++)
            {
                if (columns[i].IsInterval && !columns[i + 1].IsInterval && columns[i + 2].IsInterval && columns[i].Sign == columns[i + 2].Sign && columns[i + 1].Sign == "0")
                {
                    return true;
                }
            }

            return false;
        }

        private static string Value(MathNet.Symbolics.Expression f, double x, double y)
        {
            var result = ExactNumber.HasExactForm(y) ? $"= {ExactNumber.Latex(y)}" : $"\\approx {ExactNumber.Decimal(y)}";
            if (!ExactNumber.HasExactForm(x))
            {
                return $"f({ExactNumber.Latex(x)}) {result}";
            }

            return $"f({ExactNumber.Latex(x)}) = {ExpressionFormatter.LatexWithSubstitution(f, x)} {result}";
        }

        private static string SetLatex(IntervalSet h)
        {
            return h.Intervals is [{ Start: double.NegativeInfinity, End: double.PositiveInfinity }] ? "\\mathbb{R}" : h.Latex();
        }

        private static string Comparison(ConstraintKind kind)
        {
            return kind switch
            {
                ConstraintKind.NonZero => "\\neq",
                ConstraintKind.NonNegative => "\\geq",
                _ => ">"
            };
        }

        private static string L(MathNet.Symbolics.Expression e)
        {
            return ExpressionFormatter.Latex(e);
        }

        private static string Interval(double a, double b)
        {
            return $"$[{ExactNumber.Latex(a)};\\ {ExactNumber.Latex(b)}]$";
        }

        private static string Capitalize(string s)
        {
            return s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
        }
    }
}
