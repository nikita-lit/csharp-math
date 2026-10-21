using MathNet.Symbolics;

namespace csharp_math.Services.Mathematics
{
    public enum ConstraintKind
    {
        NonZero,
        NonNegative,
        Positive
    }

    public sealed record DomainConstraint(Expression Expression, ConstraintKind Kind, string Reason = "")
    {
        public Func<double, double> Function { get; } = ExpressionEvaluator.Compile(Expression);

        public bool IsSatisfied(double x)
        {
            var g = Function(x);
            return Kind switch
            {
                ConstraintKind.NonZero => !double.IsNaN(g) && g != 0,
                ConstraintKind.NonNegative => g >= 0,
                _ => g > 0
            };
        }
    }

    /// <summary>
    /// Collects domain restrictions before expression simplification can remove them.
    /// </summary>
    public sealed class ConstraintCollector
    {
        private readonly Dictionary<string, DomainConstraint> _constraints = new();

        public IReadOnlyList<DomainConstraint> All => _constraints.Values.ToList();

        public static ConstraintCollector Collect(Expression e)
        {
            var t = new ConstraintCollector();
            t.CollectRecursively(e);
            return t;
        }

        private void CollectRecursively(Expression e)
        {
            switch (e)
            {
                case Expression.Sum s:
                    foreach (var part in s.Item)
                    {
                        CollectRecursively(part);
                    }
                    break;

                case Expression.Product p:
                    foreach (var part in p.Item)
                    {
                        CollectRecursively(part);
                    }
                    break;

                case Expression.Power p:
                    ForPower(p.Item1, p.Item2);
                    CollectRecursively(p.Item1);
                    CollectRecursively(p.Item2);
                    break;

                case Expression.Function f:
                    ForFunction(f.Item1, f.Item2);
                    CollectRecursively(f.Item2);
                    break;
            }
        }

        public bool ForDivision(Expression denominator)
        {
            return Add(denominator, ConstraintKind.NonZero, "nimetaja ≠ 0");
        }

        public bool ForPower(Expression baseExpression, Expression exponent)
        {
            if (ExpressionMath.IsRationalNumber(exponent, out var q))
            {
                if (ExpressionMath.IsInteger(q))
                {
                    return !ExpressionMath.IsNegative(q) || Add(baseExpression, ConstraintKind.NonZero, "nimetaja ≠ 0");
                }

                if (q.Denominator.IsEven)
                {
                    return ExpressionMath.IsNegative(q)
                        ? Add(baseExpression, ConstraintKind.Positive, "juur nimetajas – juuritav > 0")
                        : Add(baseExpression, ConstraintKind.NonNegative, "paarisjuure alune ≥ 0");
                }

                return !ExpressionMath.IsNegative(q) || Add(baseExpression, ConstraintKind.NonZero, "nimetaja ≠ 0");
            }

            return Add(baseExpression, ConstraintKind.Positive, "muutuva astendajaga astme alus > 0");
        }

        public bool ForFunction(Function f, Expression argument)
        {
            if (f.IsLn)
            {
                return Add(argument, ConstraintKind.Positive, "logaritmitav > 0");
            }

            return true;
        }

        private bool Add(Expression expression, ConstraintKind kind, string reason)
        {
            var constraint = new DomainConstraint(expression, kind, reason);
            if (!ExpressionMath.ContainsVariable(expression))
            {
                return constraint.IsSatisfied(0);
            }

            _constraints.TryAdd($"{kind}:{expression}", constraint);
            return true;
        }
    }
}
