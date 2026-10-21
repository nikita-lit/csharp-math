using MathNet.Numerics;
using MathNet.Symbolics;
using System.Numerics;

namespace csharp_math.Services.Mathematics
{
    /// <summary>
    /// Symbolic expression helpers and guarded simplification for real functions.
    /// </summary>
    public static class ExpressionMath
    {
        public static readonly Expression X = Expression.Symbol("x");

        public static bool ContainsVariable(Expression e)
        {
            return e switch
            {
                Expression.Identifier => true,
                Expression.Sum s => s.Item.Any(ContainsVariable),
                Expression.Product p => p.Item.Any(ContainsVariable),
                Expression.Power p => ContainsVariable(p.Item1) || ContainsVariable(p.Item2),
                Expression.Function f => ContainsVariable(f.Item2),
                Expression.FunctionN f => f.Item2.Any(ContainsVariable),
                _ => false
            };
        }

        public static bool IsRationalNumber(Expression e, out BigRational number)
        {
            if (e is Expression.Number n)
            {
                number = n.Item;
                return true;
            }

            number = BigRational.Zero;
            return false;
        }

        public static bool IsPositive(BigRational q)
        {
            return q.Numerator.Sign > 0;
        }

        public static bool IsNegative(BigRational q)
        {
            return q.Numerator.Sign < 0;
        }

        public static bool IsInteger(BigRational q)
        {
            return q.Denominator.IsOne;
        }

        public static double ToDouble(BigRational q)
        {
            return (double)q.Numerator / (double)q.Denominator;
        }

        public static Expression Rational(BigInteger numerator, BigInteger denominator)
        {
            return SymbolicExpression.IntegerFraction(numerator, denominator).Expression;
        }

        public static Expression Integer(int n)
        {
            return SymbolicExpression.Int32(n).Expression;
        }

        public static SymbolicExpression Sym(Expression e)
        {
            return new(e);
        }

        public static bool IsRationalFunction(Expression e)
        {
            return e switch
            {
                Expression.Number or Expression.Identifier or Expression.Constant => true,
                Expression.Sum s => s.Item.All(IsRationalFunction),
                Expression.Product p => p.Item.All(IsRationalFunction),
                Expression.Power { Item2: Expression.Number n } p => IsInteger(n.Item) && IsRationalFunction(p.Item1),
                _ => !ContainsVariable(e)
            };
        }

        public static bool IsPolynomial(Expression e)
        {
            return e switch
            {
                Expression.Sum s => s.Item.All(IsPolynomial),
                Expression.Product p => p.Item.All(IsPolynomial),
                Expression.Power { Item2: Expression.Number n } p => IsInteger(n.Item) && !IsNegative(n.Item) && IsPolynomial(p.Item1),
                _ => !ContainsVariable(e) || e.Equals(X)
            };
        }

        // MathNet does not expand exponents when cancelling e^u/e^u. Normalize them
        // explicitly so factoring derivatives cannot repeatedly rediscover the same factor.
        public static Expression NormalizePowers(Expression e, bool reflect = false)
        {
            return e switch
            {
                Expression.Identifier => reflect ? Operators.negate(X) : e,
                Expression.Sum s => s.Item.Select(t => NormalizePowers(t, reflect)).Aggregate(Expression.Zero, Operators.add),
                Expression.Product p => p.Item.Select(t => NormalizePowers(t, reflect)).Aggregate(Expression.One, Operators.multiply),
                Expression.Power p => Operators.pow(NormalizePowers(p.Item1, reflect), Sym(NormalizePowers(p.Item2, reflect)).Expand().Expression),
                Expression.Function f => NormalizeFunction(f.Item1, IsPolynomial(f.Item2) ? Sym(NormalizePowers(f.Item2, reflect)).Expand().Expression : NormalizePowers(f.Item2, reflect)),
                _ => e
            };
        }

        private static Expression NormalizeFunction(Function function, Expression argument)
        {
            if ((function.IsSin || function.IsCos) && ExpressionFormatter.ToText(argument).StartsWith('-'))
            {
                var opposite = Operators.negate(argument);
                if (IsPolynomial(opposite))
                {
                    opposite = Sym(opposite).Expand().Expression;
                }

                var value = Operators.apply(function, opposite);
                return function.IsSin ? Operators.negate(value) : value;
            }

            return Operators.apply(function, argument);
        }

        public static Expression Simplify(Expression e)
        {
            var sym = Sym(e);
            var candidates = new List<Expression>
            {
                e
            };

            TryAddCandidate(candidates, () => sym.Expand().Expression);

            // MathNet can stall on RationalSimplify for expressions with fractional powers.
            if (IsRationalFunction(e))
            {
                var x = Sym(X);
                TryAddCandidate(candidates, () => sym.RationalSimplify(x).Expression);
                TryAddCandidate(candidates, () =>
                {
                    var r = sym.RationalSimplify(x);
                    return (r.Numerator().FactorSquareFree(x) / r.Denominator().FactorSquareFree(x)).Expression;
                });
            }

            return candidates.MinBy(k => ExpressionFormatter.ToText(k).Length)!;

            static void TryAddCandidate(List<Expression> list, Func<Expression> transform)
            {
                try
                {
                    list.Add(transform());
                }
                catch (Exception)
                {
                }
            }
        }

        public static Expression Differentiate(Expression e)
        {
            return Simplify(Sym(e).Differentiate(Sym(X)).Expression);
        }
    }
}
