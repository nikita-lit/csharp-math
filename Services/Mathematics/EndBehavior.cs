using MathNet.Symbolics;

namespace csharp_math.Services.Mathematics
{
    /// <summary>
    /// One-sided limits for the algebraic, logarithmic and exponential functions in the worksheets.
    /// </summary>
    public static class EndBehavior
    {
        private sealed record Growth(double Coefficient, double Power, double LogPower, double[] Exponent);

        public static double? Limit(Expression f, double point, int side)
        {
            if (double.IsInfinity(point))
            {
                var growth = Leading(f, point < 0 ? -1 : 1);
                if (growth == null)
                {
                    return null;
                }

                var exponential = growth.Exponent.Reverse().FirstOrDefault(v => Math.Abs(v) > 1e-12);
                if (exponential != 0)
                {
                    return exponential < 0 ? 0 : Math.CopySign(double.PositiveInfinity, growth.Coefficient);
                }

                var power = growth.Power != 0 ? growth.Power : growth.LogPower;
                return power < 0 ? 0 : power > 0 ? Math.CopySign(double.PositiveInfinity, growth.Coefficient) : growth.Coefficient;
            }

            var value = ExpressionEvaluator.Compile(f)(point);
            if (double.IsFinite(value))
            {
                return ExactNumber.Snap(value);
            }

            if (ExpressionMath.IsRationalFunction(f))
            {
                var rational = ExpressionMath.Sym(f).RationalSimplify(ExpressionMath.Sym(ExpressionMath.X));
                var numerator = LocalTerm(rational.Numerator().Expression, point);
                var denominator = LocalTerm(rational.Denominator().Expression, point);
                if (numerator != null && denominator != null)
                {
                    var n = numerator.Value;
                    var d = denominator.Value;
                    var power = n.Order - d.Order;
                    var coefficient = n.Coefficient / d.Coefficient;
                    return power > 0 ? 0 : power == 0 ? coefficient : Math.CopySign(double.PositiveInfinity, coefficient * Math.Pow(side, power));
                }
            }

            if (f is Expression.Product product)
            {
                var parts = product.Item.ToList();
                var limits = parts.Select(p => Limit(p, point, side)).ToList();
                if (limits.All(v => v.HasValue))
                {
                    if (limits.Any(v => v == 0) && limits.Any(v => double.IsInfinity(v!.Value)))
                    {
                        // A positive algebraic power tends to zero faster than a logarithm diverges.
                        var divergentParts = parts.Where((_, i) => double.IsInfinity(limits[i]!.Value));
                        if (divergentParts.All(p => p is Expression.Function fn && fn.Item1.IsLn))
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return limits.Aggregate(1.0, (v, p) => v * p!.Value);
                    }
                }
            }

            return f switch
            {
                Expression.Power { Item2: Expression.Number n } p when ExpressionMath.IsPositive(n.Item) && Limit(p.Item1, point, side) == 0 => 0,
                Expression.Function fn when fn.Item1.IsLn => LogLimit(Limit(fn.Item2, point, side)),
                Expression.Function fn when fn.Item1.IsExp => ExpLimit(Limit(fn.Item2, point, side)),
                Expression.Power p when p.Item1.Equals(Expression.E) => ExpLimit(Limit(p.Item2, point, side)),
                _ => null
            };
        }

        private static double? LogLimit(double? value)
        {
            if (value > 0)
            {
                return Math.Log(value.Value);
            }

            return value == 0 ? double.NegativeInfinity : null;
        }

        private static double? ExpLimit(double? value)
        {
            return value.HasValue ? Math.Exp(value.Value) : null;
        }

        private static (int Order, double Coefficient)? LocalTerm(Expression polynomial, double point)
        {
            if (!ExpressionMath.IsPolynomial(polynomial))
            {
                return null;
            }

            var coefficients = Coefficients(polynomial);
            var count = coefficients.Length;
            for (var order = 0; order < count; order++)
            {
                var value = 0.0;
                for (var j = coefficients.Length - 1; j >= 0; j--)
                {
                    value = value * point + coefficients[j];
                }

                var scale = coefficients.Select(Math.Abs).DefaultIfEmpty(1).Max() * Math.Pow(Math.Max(1, Math.Abs(point)), coefficients.Length - 1);
                if (Math.Abs(value) > 1e-9 * Math.Max(1, scale))
                {
                    return (order, value);
                }

                coefficients = coefficients.Skip(1).Select((v, i) => v * (i + 1)).ToArray();
                // The derivative's factorial cancels in a ratio of equal orders; for
                // unequal orders only its sign is used to find an infinite limit.
            }

            return null;
        }

        private static double[] Coefficients(Expression f)
        {
            return ExpressionMath.Sym(f).Expand().Coefficients(ExpressionMath.Sym(ExpressionMath.X))
                .Select(c => ExpressionEvaluator.Compile(c.Expression)(0)).ToArray();
        }

        private static Growth? Leading(Expression f, int direction)
        {
            if (ExpressionMath.IsPolynomial(f))
            {
                var coefficients = Coefficients(f);
                for (var i = coefficients.Length - 1; i >= 0; i--)
                {
                    if (coefficients[i] != 0)
                    {
                        return new Growth(coefficients[i] * Math.Pow(direction, i), i, 0, []);
                    }
                }

                return new Growth(0, 0, 0, []);
            }

            switch (f)
            {
                case Expression.Product p:
                    {
                        var parts = p.Item.Select(t => Leading(t, direction)).ToList();
                        if (parts.Any(t => t == null))
                        {
                            return null;
                        }

                        var exponent = new double[parts.Max(t => t!.Exponent.Length)];
                        foreach (var part in parts)
                        {
                            for (var j = 0; j < part!.Exponent.Length; j++)
                            {
                                exponent[j] += part.Exponent[j];
                            }
                        }

                        return new Growth(parts.Aggregate(1.0, (v, t) => v * t!.Coefficient), parts.Sum(t => t!.Power), parts.Sum(t => t!.LogPower), exponent);
                    }

                case Expression.Sum s:
                    {
                        var parts = s.Item.Select(t => Leading(t, direction)).ToList();
                        if (parts.Any(t => t == null))
                        {
                            return null;
                        }

                        parts.Sort((u, v) => Compare(v!, u!));
                        var first = parts[0]!;
                        var equal = parts.TakeWhile(t => Compare(first, t!) == 0).ToList();
                        var coefficient = equal.Sum(t => t!.Coefficient);
                        return Math.Abs(coefficient) < 1e-12 ? null : first with { Coefficient = coefficient };
                    }

                case Expression.Power p when p.Item1.Equals(Expression.E):
                    return Exponential(p.Item2, direction);

                case Expression.Power { Item2: Expression.Number n } p:
                    {
                        var inner = Leading(p.Item1, direction);
                        if (inner == null)
                        {
                            return null;
                        }

                        var power = ExpressionMath.ToDouble(n.Item);
                        var coefficient = inner.Coefficient < 0 && !n.Item.Denominator.IsEven
                            ? (n.Item.Numerator.IsEven ? 1 : -1) * Math.Pow(-inner.Coefficient, power)
                            : Math.Pow(inner.Coefficient, power);

                        return double.IsFinite(coefficient)
                            ? new Growth(coefficient, inner.Power * power, inner.LogPower * power, inner.Exponent.Select(v => v * power).ToArray())
                            : null;
                    }

                case Expression.Function fn when fn.Item1.IsExp:
                    return Exponential(fn.Item2, direction);

                case Expression.Function fn when fn.Item1.IsLn:
                    {
                        var inner = Leading(fn.Item2, direction);
                        if (inner == null || inner.Coefficient <= 0 || inner.Exponent.Any(v => v != 0))
                        {
                            return null;
                        }

                        return inner.Power != 0
                            ? new Growth(inner.Power, 0, 1, [])
                            : inner.LogPower == 0
                                ? new Growth(Math.Log(inner.Coefficient), 0, 0, [])
                                : null;
                    }

                default:
                    return null;
            }
        }

        private static Growth? Exponential(Expression exponent, int direction)
        {
            if (!ExpressionMath.IsPolynomial(exponent))
            {
                return null;
            }

            var coefficients = Coefficients(exponent);
            var factor = Math.Exp(coefficients[0]);
            coefficients[0] = 0;
            for (var i = 1; i < coefficients.Length; i++)
            {
                coefficients[i] *= Math.Pow(direction, i);
            }

            return new Growth(factor, 0, 0, coefficients);
        }

        private static int Compare(Growth a, Growth b)
        {
            for (var i = Math.Max(a.Exponent.Length, b.Exponent.Length) - 1; i >= 0; i--)
            {
                var difference = (i < a.Exponent.Length ? a.Exponent[i] : 0) - (i < b.Exponent.Length ? b.Exponent[i] : 0);
                if (Math.Abs(difference) > 1e-12)
                {
                    return Math.Sign(difference);
                }
            }

            var power = a.Power.CompareTo(b.Power);
            return power != 0 ? power : a.LogPower.CompareTo(b.LogPower);
        }
    }
}
