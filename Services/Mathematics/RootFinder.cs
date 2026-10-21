using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using MathNet.Symbolics;

namespace csharp_math.Services.Mathematics
{
    public sealed record RootResult(IReadOnlyList<double> Values, bool Complete, bool AllPoints = false);

    /// <summary>
    /// Finds algebraic roots globally and other roots numerically in the requested range.
    /// </summary>
    public static class RootFinder
    {
        private const int SampleSteps = 4000;

        public static RootResult Find(Expression g, double a, double b, IEnumerable<double>? extraCandidates = null)
        {
            if (!ExpressionMath.ContainsVariable(g))
            {
                var value = ExpressionEvaluator.Compile(g)(0);
                return new RootResult([], true, AllPoints: value == 0);
            }

            var factors = Factors(g);
            if (factors != null)
            {
                var parts = factors.Select(t => Find(t, a, b, extraCandidates)).ToList();
                return new RootResult(Uniq(parts.SelectMany(o => o.Values)), parts.All(o => o.Complete), parts.Any(o => o.AllPoints));
            }

            switch (g)
            {
                case Expression.Power { Item2: Expression.Number p } power:
                    return ExpressionMath.IsPositive(p.Item) ? Find(power.Item1, a, b, extraCandidates) : new RootResult([], true);

                case Expression.Power { Item1: var baseExpression } when !ExpressionMath.ContainsVariable(baseExpression):
                    return new RootResult([], true);

                case Expression.Function f when f.Item1.IsExp:
                    return new RootResult([], true);

                case Expression.Function f when f.Item1.IsLn || f.Item1.IsLg:
                    return Find(Operators.subtract(f.Item2, Expression.One), a, b, extraCandidates);

                case Expression.Sum sum:
                    var isolated = Isolate(sum);
                    if (isolated != null)
                    {
                        return Find(isolated, a, b, extraCandidates);
                    }
                    break;
            }

            var sym = ExpressionMath.Sym(g);
            var x = ExpressionMath.Sym(ExpressionMath.X);
            if (ExpressionMath.IsPolynomial(g))
            {
                var polynomialRoots = PolynomialRoots(sym, x);
                if (polynomialRoots != null)
                {
                    return new RootResult(polynomialRoots, true);
                }
            }

            if (ExpressionMath.IsRationalFunction(g))
            {
                try
                {
                    var rational = sym.RationalSimplify(x);
                    var numerator = rational.Numerator();
                    var denominator = rational.Denominator();
                    if (numerator.IsPolynomial(x) && denominator.IsPolynomial(x))
                    {
                        var numeratorRoots = PolynomialRoots(numerator, x);
                        if (numeratorRoots != null)
                        {
                            var n = ExpressionEvaluator.Compile(denominator.Expression);
                            return new RootResult(numeratorRoots.Where(r => Math.Abs(n(r)) > 1e-12).ToList(), true);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            var dg = ExpressionEvaluator.Compile(ExpressionMath.Sym(g).Differentiate(x).Expression);
            return new RootResult(Numerically(ExpressionEvaluator.Compile(g), dg, a, b, extraCandidates ?? []), false);
        }

        internal static List<Expression>? Factors(Expression g)
        {
            if (g is Expression.Product product)
            {
                var variableFactors = product.Item.Where(ExpressionMath.ContainsVariable).ToList();
                return variableFactors.Count > 0 ? variableFactors : null;
            }

            if (g is not Expression.Sum sum)
            {
                return null;
            }

            var terms = sum.Item.ToList();
            static IEnumerable<Expression> SplitFactors(Expression e) => e is Expression.Product p ? p.Item : [e];
            static (Expression Base, BigRational Power) Power(Expression e) => e is Expression.Power { Item2: Expression.Number n } p ? (p.Item1, n.Item) : (e, BigRational.One);

            var powers = terms.Select(t => SplitFactors(t).Where(ExpressionMath.ContainsVariable).Select(Power).ToList()).ToList();
            var commonBase = powers[0].Select(p => p.Base).FirstOrDefault(b => powers.Skip(1).All(t => t.Any(p => p.Base.Equals(b))));
            Expression? commonFactor = null;
            if (commonBase != null)
            {
                var exponent = powers.Select(t => t.First(p => p.Base.Equals(commonBase)).Power).Min();
                commonFactor = Operators.pow(commonBase, Expression.NewNumber(exponent));
            }

            if (commonFactor == null)
            {
                return null;
            }

            var quotient = ExpressionMath.NormalizePowers(terms.Aggregate(Expression.Zero, (s, l) => Operators.add(s, Operators.divide(l, commonFactor))));
            if (quotient.Equals(g))
            {
                return null;
            }

            return [commonFactor, quotient];
        }

        internal static Expression? Isolate(Expression.Sum sum)
        {
            var variableTerms = sum.Item.Where(ExpressionMath.ContainsVariable).ToList();
            if (variableTerms.Count != 1)
            {
                return null;
            }

            var constantTerm = sum.Item.Where(l => !ExpressionMath.ContainsVariable(l)).Aggregate(Expression.Zero, Operators.add);
            var term = variableTerms[0];
            var coefficient = Expression.One;
            var function = term;

            if (term is Expression.Product product)
            {
                var variableFactors = product.Item.Where(ExpressionMath.ContainsVariable).ToList();
                if (variableFactors.Count != 1)
                {
                    return null;
                }

                function = variableFactors[0];
                coefficient = product.Item.Where(t => !ExpressionMath.ContainsVariable(t)).Aggregate(Expression.One, Operators.multiply);
            }

            var r = Operators.divide(Operators.negate(constantTerm), coefficient);
            var rhsValue = ExpressionEvaluator.Compile(r)(0);
            if (double.IsNaN(rhsValue))
            {
                return null;
            }

            return function switch
            {
                Expression.Function f when f.Item1.IsLn => Operators.subtract(f.Item2, Operators.exp(r)),
                Expression.Function f when f.Item1.IsLg => Operators.subtract(f.Item2, Operators.pow(ExpressionMath.Integer(10), r)),
                Expression.Function f when f.Item1.IsExp => rhsValue > 0 ? Operators.subtract(f.Item2, Operators.ln(r)) : Expression.One,
                Expression.Power { Item1: var c, Item2: var u } when !ExpressionMath.ContainsVariable(c) => rhsValue > 0 ? Operators.subtract(u, Operators.divide(Operators.ln(r), Operators.ln(c))) : Expression.One,
                Expression.Power { Item2: Expression.Number p } power when p.Item == BigRational.FromIntFraction(1, 2) => rhsValue >= 0 ? Operators.subtract(power.Item1, Operators.pow(r, ExpressionMath.Integer(2))) : Expression.One,
                _ => null
            };
        }

        private static List<double>? PolynomialRoots(SymbolicExpression p, SymbolicExpression x)
        {
            p = p.Expand();
            if (!p.IsPolynomial(x))
            {
                return null;
            }

            var squareFree = p;
            try
            {
                var gcd = p.PolynomialGcd(x, p.Differentiate(x));
                if (Degree(gcd, x) > 0)
                {
                    squareFree = p.PolynomialQuotient(x, gcd);
                }
            }
            catch (Exception)
            {
            }

            var coefficients = squareFree.Coefficients(x).Select(k => ExpressionEvaluator.Compile(k.Expression)(0)).ToArray();
            if (coefficients.Any(double.IsNaN))
            {
                return null;
            }

            var power = coefficients.Length - 1;
            while (power > 0 && coefficients[power] == 0)
            {
                power--;
            }

            if (power == 0)
            {
                return [];
            }

            coefficients = coefficients[..(power + 1)];
            var roots = new List<double>();
            foreach (var z in FindRoots.Polynomial(coefficients))
            {
                if (Math.Abs(z.Imaginary) > 1e-7 * Math.Max(1, Math.Abs(z.Real)))
                {
                    continue;
                }

                roots.Add(ExactNumber.Snap(Newton(coefficients, z.Real)));
            }

            return Uniq(roots);
        }

        private static int Degree(SymbolicExpression p, SymbolicExpression x)
        {
            var coefficients = p.Coefficients(x);
            return coefficients.Length - 1;
        }

        private static double Newton(double[] k, double x)
        {
            for (var i = 0; i < 8; i++)
            {
                double p = 0;
                double dp = 0;
                for (var j = k.Length - 1; j >= 0; j--)
                {
                    dp = dp * x + p;
                    p = p * x + k[j];
                }

                if (dp == 0 || !double.IsFinite(p / dp))
                {
                    break;
                }

                var next = x - p / dp;
                if (Math.Abs(next - x) > 1e-3 * Math.Max(1, Math.Abs(x)))
                {
                    break;
                }

                x = next;
            }

            return x;
        }

        private static List<double> Numerically(Func<double, double> g, Func<double, double> dg, double a, double b, IEnumerable<double> extraCandidates)
        {
            var h = (b - a) / SampleSteps;
            var xs = new double[SampleSteps + 1];
            var ys = new double[SampleSteps + 1];
            for (var i = 0; i <= SampleSteps; i++)
            {
                xs[i] = i == SampleSteps ? b : a + i * h;
                ys[i] = g(xs[i]);
            }

            var roots = new List<double>();
            for (var i = 0; i <= SampleSteps; i++)
            {
                // Endpoints such as sin(pi) are not exactly zero in floating point.
                if (ys[i] == 0 || ((i == 0 || i == SampleSteps) && Math.Abs(ys[i]) < 1e-12))
                {
                    roots.Add(xs[i]);
                    continue;
                }

                if (i < SampleSteps && double.IsFinite(ys[i]) && double.IsFinite(ys[i + 1]) && ys[i] * ys[i + 1] < 0 && Brent.TryFindRoot(g, xs[i], xs[i + 1], 1e-14, 200, out var root) && Math.Abs(g(root)) < 1e-7)
                {
                    roots.Add(root);
                }

                if (i > 0 && i < SampleSteps && double.IsFinite(ys[i]) && double.IsFinite(ys[i - 1]) && double.IsFinite(ys[i + 1]) && Math.Sign(ys[i - 1]) == Math.Sign(ys[i]) && Math.Sign(ys[i + 1]) == Math.Sign(ys[i]) && Math.Abs(ys[i]) <= Math.Abs(ys[i - 1]) && Math.Abs(ys[i]) <= Math.Abs(ys[i + 1]) && Math.Abs(ys[i]) < 1e-3 && Brent.TryFindRoot(dg, xs[i - 1], xs[i + 1], 1e-14, 200, out var touching) && Math.Abs(g(touching)) < 1e-10)
                {
                    roots.Add(touching);
                }
            }

            foreach (var k in extraCandidates)
            {
                if (k >= a - 1e-9 && k <= b + 1e-9 && Math.Abs(g(k)) < 1e-10)
                {
                    roots.Add(k);
                }
            }

            return Uniq(roots.Select(ExactNumber.Snap));
        }

        private static List<double> Uniq(IEnumerable<double> values)
        {
            var result = new List<double>();
            foreach (var v in values.Order())
            {
                if (result.Count == 0 || Math.Abs(v - result[^1]) > 1e-7 * Math.Max(1, Math.Abs(v)))
                {
                    result.Add(v);
                }
            }

            return result;
        }
    }
}
