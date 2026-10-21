using MathNet.Numerics;
using MathNet.Symbolics;
using System.Globalization;
using System.Text;

namespace csharp_math.Services.Mathematics
{
    /// <summary>
    /// Formats symbolic expressions as readable text and LaTeX.
    /// </summary>
    public static class ExpressionFormatter
    {
        public static string ToText(Expression e)
        {
            return new Writer(latex: false).V(e, 0);
        }

        public static string Latex(Expression e)
        {
            return new Writer(latex: true).V(e, 0);
        }

        public static string LatexWithSubstitution(Expression e, double x)
        {
            var number = ExactNumber.Latex(x);
            var simple = number.All(char.IsDigit) || number == "\\pi";
            return new Writer(latex: true, simple ? number : $"\\left({number}\\right)").V(e, 0);
        }

        public static List<Expression> SortTerms(IEnumerable<Expression> terms)
        {
            return terms
                .Select((l, i) => (l, i))
                .OrderByDescending(t => Writer.MonomialDegree(t.l))
                .ThenBy(t => t.i)
                .Select(t => t.l)
                .ToList();
        }

        public static string Parenthesize(string latex)
        {
            if (latex.StartsWith('-'))
            {
                return $"\\left({latex}\\right)";
            }

            var depth = 0;
            for (var i = 0; i < latex.Length; i++)
            {
                if (latex[i] == '{' || latex.AsSpan(i).StartsWith("\\left"))
                {
                    depth++;
                }
                else if (latex[i] == '}' || latex.AsSpan(i).StartsWith("\\right"))
                {
                    depth--;
                }
                else if (depth == 0 && (latex.AsSpan(i).StartsWith(" + ") || latex.AsSpan(i).StartsWith(" - ")))
                {
                    return $"\\left({latex}\\right)";
                }
            }

            return latex;
        }

        private sealed class Writer(bool latex, string? variableSubstitution = null)
        {
            public string V(Expression e, int required)
            {
                var (text, precedence) = Format(e);
                if (precedence >= required)
                {
                    return text;
                }

                return latex ? $"\\left({text}\\right)" : $"({text})";
            }

            private (string, int) Format(Expression e)
            {
                return e switch
                {
                    Expression.Number n => FormatRational(n.Item),
                    Expression.Approximation { Item: Approximation.Real r } => (r.Item.ToString("G10", CultureInfo.InvariantCulture), r.Item < 0 ? 1 : 5),
                    Expression.Identifier id => (variableSubstitution ?? id.Item.Item, 5),
                    Expression.Constant c => (c.Item.IsPi ? (latex ? "\\pi" : "pi") : c.Item.IsE ? "e" : "i", 5),
                    Expression.Sum s => (Sum(s.Item.ToList()), 1),
                    Expression.Product p => Product(p.Item.ToList()),
                    Expression.Power p => Power(p.Item1, p.Item2),
                    Expression.Function f => (Function(f.Item1, f.Item2), 4),
                    _ when e.IsPositiveInfinity => (latex ? "\\infty" : "∞", 5),
                    _ when e.IsNegativeInfinity => (latex ? "-\\infty" : "-∞", 1),
                    _ => (e.ToString(), 1)
                };
            }

            private (string, int) FormatRational(BigRational q)
            {
                var isNegative = ExpressionMath.IsNegative(q);
                var numerator = BigIntegerAbs(q.Numerator).ToString(CultureInfo.InvariantCulture);
                var signed = isNegative ? "-" : "";
                if (ExpressionMath.IsInteger(q))
                {
                    return (signed + numerator, isNegative ? 1 : 5);
                }

                var denominator = q.Denominator.ToString(CultureInfo.InvariantCulture);
                return latex
                    ? ($"{signed}\\frac{{{numerator}}}{{{denominator}}}", isNegative ? 1 : 5)
                    : ($"{signed}{numerator}/{denominator}", isNegative ? 1 : 2);
            }

            private string Sum(List<Expression> terms)
            {
                var ordered = terms
                    .Select((l, i) => (l, i, power: MonomialDegree(l)))
                    .OrderByDescending(t => t.power)
                    .ThenBy(t => t.i)
                    .Select(t => t.l)
                    .ToList();

                if (ordered.Count == 2 && IsNegative(ordered[0]) && !IsNegative(ordered[1]))
                {
                    ordered.Reverse();
                }

                var sb = new StringBuilder(V(ordered[0], 1));
                foreach (var term in ordered.Skip(1))
                {
                    if (IsNegative(term))
                    {
                        sb.Append(" - ").Append(V(Operators.negate(term), 2));
                    }
                    else
                    {
                        sb.Append(" + ").Append(V(term, 1));
                    }
                }

                return sb.ToString();
            }

            internal static double MonomialDegree(Expression e)
            {
                return e switch
                {
                    Expression.Number or Expression.Constant => 0,
                    Expression.Identifier => 1,
                    Expression.Power { Item1: Expression.Identifier, Item2: Expression.Number n } => ExpressionMath.ToDouble(n.Item),
                    Expression.Product p when p.Item.All(f => f is Expression.Number or Expression.Constant || MonomialDegree(f) != 0.5) => p.Item.Sum(MonomialDegree),
                    _ => ExpressionMath.ContainsVariable(e) ? 0.5 : 0
                };
            }

            private static bool IsNegative(Expression e)
            {
                return e switch
                {
                    Expression.Number n => ExpressionMath.IsNegative(n.Item),
                    Expression.Approximation { Item: Approximation.Real r } => r.Item < 0,
                    Expression.Product p => p.Item.Head is Expression.Number n && ExpressionMath.IsNegative(n.Item),
                    _ => false
                };
            }

            private (string, int) Product(List<Expression> factors)
            {
                var coefficient = BigRational.One;
                var numerator = new List<Expression>();
                var denominator = new List<Expression>();

                foreach (var t in factors)
                {
                    if (t is Expression.Number n)
                    {
                        coefficient *= n.Item;
                    }
                    else if (t is Expression.Power { Item2: Expression.Number a } p && ExpressionMath.IsNegative(a.Item))
                    {
                        denominator.Add(Operators.pow(p.Item1, Expression.NewNumber(-a.Item)));
                    }
                    else
                    {
                        numerator.Add(t);
                    }
                }

                numerator = numerator.OrderBy(FactorOrder).ToList();
                denominator = denominator.OrderBy(FactorOrder).ToList();
                var sign = ExpressionMath.IsNegative(coefficient) ? "-" : "";
                var absoluteNumerator = BigIntegerAbs(coefficient.Numerator);

                if (!absoluteNumerator.IsOne || numerator.Count == 0)
                {
                    numerator.Insert(0, Expression.NewNumber(BigRational.FromBigInt(absoluteNumerator)));
                }

                if (!coefficient.Denominator.IsOne)
                {
                    denominator.Insert(0, Expression.NewNumber(BigRational.FromBigInt(coefficient.Denominator)));
                }

                if (denominator.Count == 0)
                {
                    return (sign + Factors(numerator), sign == "" ? 2 : 1);
                }

                if (latex)
                {
                    var lu = numerator.Count == 1 ? V(numerator[0], 0) : Factors(numerator);
                    var ni = denominator.Count == 1 ? V(denominator[0], 0) : Factors(denominator);
                    return ($"{sign}\\frac{{{lu}}}{{{ni}}}", sign == "" ? 2 : 1);
                }

                var numeratorText = numerator.Count == 1 ? V(numerator[0], 3) : Factors(numerator);
                var denominatorText = denominator.Count == 1 ? V(denominator[0], 3) : $"({Factors(denominator)})";
                return ($"{sign}{numeratorText}/{denominatorText}", sign == "" ? 2 : 1);
            }

            private static int FactorOrder(Expression t)
            {
                return t switch
                {
                    Expression.Number or Expression.Constant => 0,
                    Expression.Identifier or Expression.Power { Item1: Expression.Identifier, Item2: Expression.Number } => 1,
                    Expression.Sum or Expression.Power { Item1: Expression.Sum } => 2,
                    _ => 3
                };
            }

            private string Factors(List<Expression> factors)
            {
                var sb = new StringBuilder();
                foreach (var t in factors)
                {
                    var part = V(t, 2);
                    if (sb.Length > 0)
                    {
                        if (!latex)
                        {
                            sb.Append('*');
                        }
                        else if (char.IsDigit(part[0]))
                        {
                            sb.Append(" \\cdot ");
                        }
                        else if (variableSubstitution != null && char.IsDigit(sb[^1]))
                        {
                            sb.Append(" \\cdot ");
                        }
                        else if (char.IsLetter(sb[^1]) && char.IsLetter(part[0]))
                        {
                            sb.Append(' ');
                        }
                    }

                    sb.Append(part);
                }

                return sb.ToString();
            }

            private (string, int) Power(Expression baseExpression, Expression exponent)
            {
                if (ExpressionMath.IsRationalNumber(exponent, out var q))
                {
                    if (ExpressionMath.IsNegative(q))
                    {
                        var positive = Operators.pow(baseExpression, Expression.NewNumber(-q));
                        return latex ? ($"\\frac{{1}}{{{V(positive, 0)}}}", 2) : ($"1/{V(positive, 3)}", 2);
                    }

                    if (q == BigRational.FromIntFraction(1, 2))
                    {
                        return (latex ? $"\\sqrt{{{V(baseExpression, 0)}}}" : $"sqrt({V(baseExpression, 0)})", 4);
                    }

                    if (latex && q.Numerator.IsOne && q.Denominator < 10)
                    {
                        return ($"\\sqrt[{q.Denominator}]{{{V(baseExpression, 0)}}}", 4);
                    }
                }

                if (latex && baseExpression is Expression.Function f && !f.Item1.IsExp && ExpressionMath.IsRationalNumber(exponent, out var n) && ExpressionMath.IsInteger(n))
                {
                    return ($"{FunctionName(f.Item1)}^{{{n.Numerator}}}{Argument(f.Item2)}", 4);
                }

                var baseText = V(baseExpression, 5);
                if (latex)
                {
                    return ($"{baseText}^{{{V(exponent, 0)}}}", 3);
                }

                return ($"{baseText}^{V(exponent, 5)}", 3);
            }

            private string Function(Function f, Expression argument)
            {
                if (f.IsExp)
                {
                    return latex ? $"e^{{{V(argument, 0)}}}" : $"e^{V(argument, 5)}";
                }

                if (!latex)
                {
                    return $"{FunctionName(f)}({V(argument, 0)})";
                }

                return FunctionName(f) + Argument(argument);
            }

            private string Argument(Expression argument)
            {
                return argument is Expression.Identifier or Expression.Constant || argument is Expression.Number n && !ExpressionMath.IsNegative(n.Item) && ExpressionMath.IsInteger(n.Item)
                    ? " " + V(argument, 0)
                    : $"\\left({V(argument, 0)}\\right)";
            }

            private string FunctionName(Function f)
            {
                string name = f.IsSin ? "sin" : f.IsCos ? "cos" : f.IsTan ? "tan" : f.IsCot ? "cot" : f.IsSec ? "sec" : f.IsCsc ? "csc" : f.IsAsin ? "arcsin" : f.IsAcos ? "arccos" : f.IsAtan ? "arctan" : f.IsAcot ? "arccot" : f.IsSinh ? "sinh" : f.IsCosh ? "cosh" : f.IsTanh ? "tanh" : f.IsLn ? "ln" : f.IsLg ? "lg" : f.IsAbs ? "abs" : f.ToString().ToLowerInvariant();
                if (!latex)
                {
                    return name;
                }

                return name is "arccot" or "abs" ? $"\\operatorname{{{name}}}" : "\\" + name;
            }

            private static System.Numerics.BigInteger BigIntegerAbs(System.Numerics.BigInteger v)
            {
                return System.Numerics.BigInteger.Abs(v);
            }
        }
    }
}
