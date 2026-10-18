using System.Numerics;
using MathNet.Numerics;
using MathNet.Symbolics;

namespace csharp_math.Services.Mathematics;
/// <summary>Symbolic expression helpers and guarded simplification for real functions.</summary>
public static class ExpressionMath
{
    public static readonly Expression X = Expression.Symbol("x");
    public static bool ContainsVariable(Expression e) => e switch
    {
        Expression.Identifier => true,
        Expression.Sum s => s.Item.Any(ContainsVariable),
        Expression.Product p => p.Item.Any(ContainsVariable),
        Expression.Power p => ContainsVariable(p.Item1) || ContainsVariable(p.Item2),
        Expression.Function f => ContainsVariable(f.Item2),
        Expression.FunctionN f => f.Item2.Any(ContainsVariable),
        _ => false
    };
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

    public static bool IsPositive(BigRational q) => q.Numerator.Sign > 0;
    public static bool IsNegative(BigRational q) => q.Numerator.Sign < 0;
    public static bool IsInteger(BigRational q) => q.Denominator.IsOne;
    public static double ToDouble(BigRational q) => (double)q.Numerator / (double)q.Denominator;
    public static Expression Rational(BigInteger numerator, BigInteger denominator) => SymbolicExpression.IntegerFraction(numerator, denominator).Expression;
    public static Expression Integer(int n) => SymbolicExpression.Int32(n).Expression;
    public static SymbolicExpression Sym(Expression e) => new(e);
    public static bool IsRationalFunction(Expression e) => e switch
    {
        Expression.Number or Expression.Identifier or Expression.Constant => true,
        Expression.Sum s => s.Item.All(IsRationalFunction),
        Expression.Product p => p.Item.All(IsRationalFunction),
        Expression.Power { Item2: Expression.Number n } p => IsInteger(n.Item) && IsRationalFunction(p.Item1),
        _ => !ContainsVariable(e)
    };
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

    public static Expression Differentiate(Expression e) => Simplify(Sym(e).Differentiate(Sym(X)).Expression);
}
