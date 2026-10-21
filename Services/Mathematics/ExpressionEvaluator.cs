using MathNet.Symbolics;

namespace csharp_math.Services.Mathematics
{
    /// <summary>
    /// Compiles real expressions and preserves real odd roots for negative inputs.
    /// </summary>
    public static class ExpressionEvaluator
    {
        public static Func<double, double> Compile(Expression e)
        {
            var f = K(e);
            return x =>
            {
                var y = f(x);
                return double.IsFinite(y) ? y : double.NaN;
            };
        }

        private static Func<double, double> K(Expression e)
        {
            switch (e)
            {
                case Expression.Number n:
                    var number = ExpressionMath.ToDouble(n.Item);
                    return _ => number;

                case Expression.Approximation { Item: Approximation.Real r }:
                    var real = r.Item;
                    return _ => real;

                case Expression.Identifier:
                    return x => x;

                case Expression.Constant c:
                    var constant = c.Item.IsPi ? Math.PI : c.Item.IsE ? Math.E : double.NaN;
                    return _ => constant;

                case Expression.Sum s:
                    {
                        var parts = s.Item.Select(K).ToArray();
                        return x =>
                        {
                            var sum = 0.0;
                            foreach (var o in parts)
                            {
                                sum += o(x);
                            }

                            return sum;
                        };
                    }

                case Expression.Product p:
                    {
                        var parts = p.Item.Select(K).ToArray();
                        return x =>
                        {
                            var product = 1.0;
                            foreach (var o in parts)
                            {
                                product *= o(x);
                            }

                            return product;
                        };
                    }

                case Expression.Power p:
                    return Power(p.Item1, p.Item2);

                case Expression.Function f:
                    return Function(f.Item1, K(f.Item2));

                default:
                    return _ => double.NaN;
            }
        }

        private static Func<double, double> Power(Expression baseExpression, Expression exponentExpression)
        {
            var b = K(baseExpression);
            if (ExpressionMath.IsRationalNumber(exponentExpression, out var q))
            {
                var exponentValue = ExpressionMath.ToDouble(q);
                if (ExpressionMath.IsInteger(q))
                {
                    var n = (int)q.Numerator;
                    return x => Math.Pow(b(x), n);
                }

                // Odd roots remain real for negative bases; Math.Pow alone would return NaN.
                if (!q.Denominator.IsEven)
                {
                    var oddNumerator = !q.Numerator.IsEven;
                    return x =>
                    {
                        var v = b(x);
                        var r = Math.Pow(Math.Abs(v), exponentValue);
                        return v < 0 && oddNumerator ? -r : r;
                    };
                }

                return x => Math.Pow(b(x), exponentValue);
            }

            var a = K(exponentExpression);
            return x => Math.Pow(b(x), a(x));
        }

        private static Func<double, double> Function(Function f, Func<double, double> a)
        {
            if (f.IsSin)
            {
                return x => Math.Sin(a(x));
            }

            if (f.IsCos)
            {
                return x => Math.Cos(a(x));
            }

            if (f.IsExp)
            {
                return x => Math.Exp(a(x));
            }

            if (f.IsLn)
            {
                return x =>
                {
                    var u = a(x);
                    return u > 0 ? Math.Log(u) : double.NaN;
                };
            }

            return _ => double.NaN;
        }
    }
}
