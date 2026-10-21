using MathNet.Numerics;
using MathNet.Symbolics;
using System.Numerics;

namespace csharp_math.Services.Mathematics
{
    /// <summary>
    /// Explains root finding through factoring, discriminants and Horner division.
    /// </summary>
    public static class EquationExplainer
    {
        private const int MaxDepth = 6;
        private static readonly BigRational Square = BigRational.FromInt(2);
        private static readonly BigRational FourthPower = BigRational.FromInt(4);

        public static List<SolutionStep> Steps(Expression g, string left, RootResult roots, double a, double b)
        {
            var steps = new List<SolutionStep>
            {
                new(null, $"{left} = 0 \\iff {L(g)} = 0")
            };

            Solve(g, steps, a, b, 0);

            if (roots.AllPoints)
            {
                steps.Add(new("Samaselt null – kehtib iga $x$ korral."));
            }
            else if (roots.Values.Count == 0)
            {
                steps.Add(new("**Lahendid puuduvad.**" + (roots.Complete ? "" : $" (Otsiti lõigul {Interval(a, b)}.)")));
            }
            else if (steps[^1].Formula != FormatList(roots.Values))
            {
                steps.Add(new(roots.Complete ? null : $"Lõigul {Interval(a, b)}:", FormatList(roots.Values)));
            }

            return steps;
        }

        public static string FormatList(IReadOnlyList<double> values, string name = "x")
        {
            return string.Join(", \\quad ", values.Select((v, i) => ExactNumber.EqualityLatex(values.Count > 1 ? $"{name}_{{{i + 1}}}" : name, v)));
        }

        private static void Solve(Expression g, List<SolutionStep> s, double a, double b, int depth)
        {
            if (depth > MaxDepth)
            {
                return;
            }

            if (!ExpressionMath.ContainsVariable(g))
            {
                if (ExpressionEvaluator.Compile(g)(0) != 0)
                {
                    s.Add(new($"${L(g)} \\neq 0$."));
                }

                return;
            }

            var factors = RootFinder.Factors(g);
            if (factors != null)
            {
                if (g is Expression.Sum)
                {
                    s.Add(new("Toome ühise teguri sulgude ette:", $"{string.Join(" \\cdot ", factors.Select(t => Parenthesize(L(t))))} = 0"));
                }

                var inDenominator = factors.Where(t => t is Expression.Power { Item2: Expression.Number n } && ExpressionMath.IsNegative(n.Item)).ToList();
                if (inDenominator.Count > 0)
                {
                    factors = factors.Except(inDenominator).ToList();
                    if (factors.Count == 0)
                    {
                        s.Add(new("Lugeja on konstant $\\neq 0$."));
                        return;
                    }

                    var numerator = factors.Aggregate(Expression.One, Operators.multiply);
                    s.Add(new("Murd $= 0 \\iff$ lugeja $= 0$:", $"{L(numerator)} = 0"));
                }

                if (factors.Count > 1)
                {
                    s.Add(new("Korrutis $= 0$, kui mõni tegur $= 0$:", string.Join(" \\quad \\text{või} \\quad ", factors.Select(t => $"{L(t)} = 0"))));
                    foreach (var t in factors)
                    {
                        if (t is Expression.Identifier)
                        {
                            s.Add(new("$x = 0$"));
                            continue;
                        }

                        s.Add(new($"${L(t)} = 0$:"));
                        Solve(t, s, a, b, depth + 1);
                        PartResult(t, s, a, b);
                    }
                }
                else if (factors[0] is not Expression.Identifier)
                {
                    Solve(factors[0], s, a, b, depth + 1);
                }

                return;
            }

            switch (g)
            {
                case Expression.Power { Item2: Expression.Number p } power:
                    if (ExpressionMath.IsPositive(p.Item))
                    {
                        s.Add(new("Aste $= 0 \\iff$ alus $= 0$:", $"{L(power.Item1)} = 0"));
                        if (power.Item1 is not Expression.Identifier)
                        {
                            Solve(power.Item1, s, a, b, depth + 1);
                        }
                    }
                    else
                    {
                        s.Add(new($"${L(g)} \\neq 0$ (lugeja on 1)."));
                    }
                    return;

                case Expression.Power { Item1: var baseExpression } when !ExpressionMath.ContainsVariable(baseExpression):
                case Expression.Function f when f.Item1.IsExp:
                    s.Add(new($"${L(g)} > 0$ alati."));
                    return;

                case Expression.Function f when f.Item1.IsLn || f.Item1.IsLg:
                    s.Add(new(null, $"{L(g)} = 0 \\iff {L(f.Item2)} = 1"));
                    Solve(Operators.subtract(f.Item2, Expression.One), s, a, b, depth + 1);
                    return;

                case Expression.Sum sum:
                    var isolated = RootFinder.Isolate(sum);
                    if (isolated != null)
                    {
                        if (!ExpressionMath.ContainsVariable(isolated))
                        {
                            s.Add(new("Eksponent/juur ei saa olla negatiivne."));
                            return;
                        }

                        s.Add(new("Pöördfunktsiooniga:", $"{L(isolated)} = 0"));
                        Solve(isolated, s, a, b, depth + 1);
                        return;
                    }
                    break;
            }

            var sym = ExpressionMath.Sym(g);
            var x = ExpressionMath.Sym(ExpressionMath.X);
            if (ExpressionMath.IsPolynomial(g))
            {
                Polynomial(sym.Expand(), s, "x");
                return;
            }

            if (ExpressionMath.IsRationalFunction(g))
            {
                try
                {
                    var r = sym.RationalSimplify(x);
                    var numerator = r.Numerator();
                    var denominator = r.Denominator();
                    if (numerator.IsPolynomial(x) && denominator.IsPolynomial(x))
                    {
                        s.Add(new(null, $"\\frac{{{L(numerator.Expression)}}}{{{L(denominator.Expression)}}} = 0 \\iff " + $"{L(numerator.Expression)} = 0, \\quad {L(denominator.Expression)} \\neq 0"));
                        Polynomial(numerator, s, "x");
                        var n = ExpressionEvaluator.Compile(denominator.Expression);
                        var outside = RootFinder.Find(numerator.Expression, a, b).Values.Where(v => Math.Abs(n(v)) <= 1e-12).ToList();
                        if (outside.Count > 0)
                        {
                            s.Add(new($"${FormatList(outside)}$ – nimetaja $= 0$, ei sobi."));
                        }

                        return;
                    }
                }
                catch (Exception)
                {
                }
            }

            s.Add(new($"Algebraliselt ei lahendu – **numbriliselt** lõigul {Interval(a, b)}."));
        }

        private static void PartResult(Expression t, List<SolutionStep> s, double a, double b)
        {
            var j = RootFinder.Find(t, a, b);
            if (j.AllPoints)
            {
                return;
            }

            s.Add(new(j.Values.Count == 0 ? "→ lahendid puuduvad" : $"→ ${FormatList(j.Values)}$"));
        }

        private static void Polynomial(SymbolicExpression p, List<SolutionStep> s, string variable)
        {
            var x = ExpressionMath.Sym(ExpressionMath.X);
            BigRational[] k;

            try
            {
                var coefficients = p.Coefficients(x);
                if (!coefficients.All(c => ExpressionMath.IsRationalNumber(c.Expression, out _)))
                {
                    if (coefficients.Length == 2)
                    {
                        var solution = Operators.divide(Operators.negate(coefficients[0].Expression), coefficients[1].Expression);
                        s.Add(new(null, $"{L(p.Expression)} = 0 \\iff {variable} = {L(solution)}"));
                        return;
                    }

                    s.Add(new("Irratsionaalsed kordajad – juured **numbriliselt**, vastused on ligikaudsed."));
                    return;
                }

                k = coefficients.Select(c => ((Expression.Number)c.Expression).Item).ToArray();
            }
            catch (Exception)
            {
                return;
            }

            PolynomialFromCoefficients(k, s, variable, 0);
        }

        private static void PolynomialFromCoefficients(BigRational[] k, List<SolutionStep> s, string variable, int depth)
        {
            var power = k.Length - 1;
            while (power > 0 && k[power].IsZero)
            {
                power--;
            }

            k = k[..(power + 1)];
            if (power == 0)
            {
                return;
            }

            var m = 0;
            while (k[m].IsZero)
            {
                m++;
            }

            if (m > 0)
            {
                var rest = k[m..];
                var xm = m == 1 ? variable : $"{variable}^{{{m}}}";
                if (rest.Length == 1)
                {
                    if (Pol(k, variable) != variable)
                    {
                        s.Add(new($"${Pol(k, variable)} = 0 \\iff {variable} = 0$."));
                    }

                    return;
                }

                s.Add(new($"${xm}$ sulgude ette:", $"{xm} \\left({Pol(rest, variable)}\\right) = 0 \\iff {variable} = 0 \\quad \\text{{või}} \\quad {Pol(rest, variable)} = 0"));
                k = rest;
                power = k.Length - 1;
            }

            switch (power)
            {
                case 1:
                    Linear(k, s, variable);
                    return;

                case 2:
                    Quadratic(k, s, variable);
                    return;

                case 4 when k[1].IsZero && k[3].IsZero:
                    Biquadratic(k, s, variable);
                    return;
            }

            var r = RationalRoot(k);
            if (depth < 4 && r is not null)
            {
                var rootVal = r;
                var quotient = Horner(k, rootVal);
                var rL = L(Expression.NewNumber(rootVal));
                s.Add(new($"Ratsionaalne juur (vabaliikme jagajate seast): $P({rL}) = 0$."));
                var factor = Pol([-rootVal, BigRational.One], variable);
                s.Add(new($"Horneri skeem, jagame $({factor})$-ga:", $"{Pol(k, variable)} = \\left({factor}\\right)\\left({Pol(quotient, variable)}\\right) = 0"));
                if (quotient.Length > 1)
                {
                    PolynomialFromCoefficients(quotient, s, variable, depth + 1);
                }

                return;
            }

            s.Add(new($"{power}. aste, ratsionaalseid juuri pole – **numbriliselt**, vastused on ligikaudsed."));
        }

        private static void Linear(BigRational[] k, List<SolutionStep> s, string variable)
        {
            var solution = -k[0] / k[1];
            var right = L(Expression.NewNumber(-k[0]));
            var result = L(Expression.NewNumber(solution));
            s.Add(new(null, k[1] == BigRational.One ? $"{Pol(k, variable)} = 0 \\iff {variable} = {right}" : $"{Pol(k, variable)} = 0 \\iff {Pol([BigRational.Zero, k[1]], variable)} = {right} \\iff {variable} = {result}"));
        }

        private static void Quadratic(BigRational[] k, List<SolutionStep> s, string variable)
        {
            var (c, b, a) = (k[0], k[1], k[2]);
            string N(BigRational q) => L(Expression.NewNumber(q));
            string NS(BigRational q) => Parenthesize(N(q));

            if (b.IsZero)
            {
                var right = -c / a;
                s.Add(new(null, $"{Pol(k, variable)} = 0 \\iff {variable}^{{2}} = {N(right)}"));
                if (ExpressionMath.IsNegative(right))
                {
                    s.Add(new("Ruut ei saa olla negatiivne – lahendid puuduvad."));
                }
                else
                {
                    var root = ExactNumber.Latex(Math.Sqrt(ExpressionMath.ToDouble(right)));
                    var rootForm = $"\\sqrt{{{N(right)}}}";
                    s.Add(new(null, root == rootForm ? $"{variable} = \\pm{rootForm}" : $"{variable} = \\pm{rootForm} = \\pm {root}"));
                }

                return;
            }

            var d = b * b - FourthPower * a * c;
            s.Add(new($"$a = {N(a)},\\ b = {N(b)},\\ c = {N(c)}$:", $"D = b^{{2}} - 4ac = {NS(b)}^{{2}} - 4 \\cdot {NS(a)} \\cdot {NS(c)} = {N(d)}"));
            if (ExpressionMath.IsNegative(d))
            {
                s.Add(new("$D < 0$ – lahendid puuduvad."));
                return;
            }

            if (d.IsZero)
            {
                var x0 = -b / (Square * a);
                s.Add(new("$D = 0$:", $"{variable} = -\\frac{{b}}{{2a}} = -\\frac{{{N(b)}}}{{{N(Square * a)}}} = {N(x0)}"));
                return;
            }

            var sqrtD = Math.Sqrt(ExpressionMath.ToDouble(d));
            var (ad, bd) = (ExpressionMath.ToDouble(a), ExpressionMath.ToDouble(b));
            var solutions = new[]
            {
                (-bd - sqrtD) / (2 * ad),
                (-bd + sqrtD) / (2 * ad)
            }.Order().ToList();

            s.Add(new("$D > 0$:", $"{variable}_{{1,2}} = \\frac{{-b \\pm \\sqrt{{D}}}}{{2a}} = \\frac{{{N(-b)} \\pm \\sqrt{{{N(d)}}}}}{{{N(Square * a)}}}"));
            s.Add(new(null, FormatList(solutions.Select(ExactNumber.Snap).ToList(), variable)));
        }

        private static void Biquadratic(BigRational[] k, List<SolutionStep> s, string variable)
        {
            var t = new[]
            {
                k[0],
                k[2],
                k[4]
            };

            s.Add(new($"Asendus $t = {variable}^{{2}} \\geq 0$:", $"{Pol(t, "t")} = 0"));
            Quadratic(t, s, "t");

            var (c, b, a) = (ExpressionMath.ToDouble(t[0]), ExpressionMath.ToDouble(t[1]), ExpressionMath.ToDouble(t[2]));
            var d = b * b - 4 * a * c;
            if (d < 0)
            {
                return;
            }

            var constraintRootValues = d == 0
                ? new[] { -b / (2 * a) }
                : [(-b - Math.Sqrt(d)) / (2 * a), (-b + Math.Sqrt(d)) / (2 * a)];

            foreach (var tj in constraintRootValues.Select(ExactNumber.Snap).Distinct())
            {
                s.Add(new(tj < 0 ? $"$t = {ExactNumber.Latex(tj)} < 0$ – ei sobi." : $"${variable}^{{2}} = {ExactNumber.Latex(tj)} \\Rightarrow {variable} = \\pm {ExactNumber.Latex(Math.Sqrt(tj))}$"));
            }
        }

        private static BigRational? RationalRoot(BigRational[] k)
        {
            var vjk = k.Aggregate(BigInteger.One, (acc, q) => acc * q.Denominator / BigInteger.GreatestCommonDivisor(acc, q.Denominator));
            var t = k.Select(q => q.Numerator * (vjk / q.Denominator)).ToArray();
            var (a0, an) = (BigInteger.Abs(t[0]), BigInteger.Abs(t[^1]));
            if (a0 > 1_000_000 || an > 1_000_000)
            {
                return null;
            }

            var denominators = Divisors((long)an);
            foreach (var p in Divisors((long)a0))
            {
                foreach (var q in denominators)
                {
                    foreach (var sign in new[] { 1, -1 })
                    {
                        var r = BigRational.FromBigIntFraction(sign * p, q);
                        if (Value(k, r).IsZero)
                        {
                            return r;
                        }
                    }
                }
            }

            return null;
        }

        private static List<long> Divisors(long n)
        {
            var divisors = new List<long>();
            for (long i = 1; i * i <= n; i++)
            {
                if (n % i != 0)
                {
                    continue;
                }

                divisors.Add(i);
                if (i * i != n)
                {
                    divisors.Add(n / i);
                }
            }

            return divisors.Order().ToList();
        }

        private static BigRational Value(BigRational[] k, BigRational x)
        {
            var result = BigRational.Zero;
            for (var i = k.Length - 1; i >= 0; i--)
            {
                result = result * x + k[i];
            }

            return result;
        }

        private static BigRational[] Horner(BigRational[] k, BigRational r)
        {
            var n = k.Length - 1;
            var q = new BigRational[n];
            var remainder = k[n];
            for (var i = n - 1; i >= 0; i--)
            {
                q[i] = remainder;
                remainder = remainder * r + k[i];
            }

            return q;
        }

        private static string Pol(BigRational[] k, string variable)
        {
            var m = Expression.Symbol(variable);
            var expression = k.Select((c, i) => Operators.multiply(Expression.NewNumber(c), Operators.pow(m, ExpressionMath.Integer(i)))).Aggregate(Expression.Zero, Operators.add);
            return L(expression);
        }

        private static string L(Expression e)
        {
            return ExpressionFormatter.Latex(e);
        }

        private static string Parenthesize(string latex)
        {
            return ExpressionFormatter.Parenthesize(latex);
        }

        private static string Interval(double a, double b)
        {
            return $"$[{ExactNumber.Latex(a)};\\ {ExactNumber.Latex(b)}]$";
        }
    }
}
