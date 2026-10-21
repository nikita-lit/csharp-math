using MathNet.Symbolics;

namespace csharp_math.Services.Mathematics
{
    /// <summary>
    /// Explains differentiation using basic rules, the chain rule and product or quotient rules.
    /// </summary>
    public static class DifferentiationExplainer
    {
        public static List<SolutionStep> Steps(Expression f, string name, string derivativeName)
        {
            var steps = new List<SolutionStep>
            {
                new(null, $"{name}(x) = {L(f)}")
            };

            var result = ExpressionMath.Differentiate(f);
            if (!ExpressionMath.ContainsVariable(f))
            {
                steps.Add(new("Konstandi tuletis:", $"{derivativeName}(x) = 0"));
                return steps;
            }

            var substituted = Explain(f, steps);
            if (substituted != null && substituted != L(result))
            {
                steps.Add(new(null, $"{derivativeName}(x) = {substituted}"));
                steps.Add(new("Lihtsustatult:", $"{derivativeName}(x) = {L(result)}"));
            }
            else
            {
                steps.Add(new(null, $"{derivativeName}(x) = {L(result)}"));
            }

            return steps;
        }

        private static string? Explain(Expression f, List<SolutionStep> steps)
        {
            switch (f)
            {
                case Expression.Sum sum:
                    {
                        var terms = ExpressionFormatter.SortTerms(sum.Item);
                        steps.Add(Rule("Summa", "(u \\pm v)' = u' \\pm v'"));
                        var derivatives = new List<string>();
                        foreach (var term in terms)
                        {
                            var d = ExpressionMath.Differentiate(term);
                            var r = ShortRule(term);
                            steps.Add(new(r != null ? $"{Capitalize(r)}:" : null, $"\\left({L(term)}\\right)' = {L(d)}"));
                            if (!d.Equals(Expression.Zero))
                            {
                                derivatives.Add(L(d));
                            }
                        }

                        return derivatives.Count == 0 ? "0" : Sum(derivatives);
                    }

                case Expression.Product product:
                    return ExplainProduct(product.Item.ToList(), steps);

                default:
                    var (title, formula) = BasicRule(f);
                    steps.Add(Rule(title, formula));
                    return ChainRule(f, steps);
            }
        }

        private static string? ExplainProduct(List<Expression> factors, List<SolutionStep> steps)
        {
            var constants = factors.Where(t => !ExpressionMath.ContainsVariable(t)).ToList();
            var numerator = factors.Where(t => ExpressionMath.ContainsVariable(t) && !IsInDenominator(t)).ToList();
            var denominator = factors.Where(t => ExpressionMath.ContainsVariable(t) && IsInDenominator(t)).Select(t => Operators.pow(((Expression.Power)t).Item1, Operators.negate(((Expression.Power)t).Item2))).ToList();
            var variablePart = Multiply(numerator.Concat(factors.Where(t => ExpressionMath.ContainsVariable(t) && IsInDenominator(t))));
            var c = Multiply(constants);
            var cLatex = constants.Count == 0 ? "" : CoefficientLatex(c);

            if (constants.Count > 0)
            {
                steps.Add(Rule("Konstantne tegur", "(c \\cdot u)' = c \\cdot u'"));
            }

            string? innerExpression;
            if (denominator.Count > 0 && (numerator.Count > 0 || constants.Count == 0))
            {
                var u = Multiply(numerator);
                var v = Multiply(denominator);
                steps.Add(Rule("Jagatis", "\\left(\\frac{u}{v}\\right)' = \\frac{u' v - u v'}{v^{2}}"));
                steps.AddRange(Parts(("u", u), ("v", v)));
                innerExpression = $"\\frac{{{S(L(ExpressionMath.Differentiate(u)))} \\cdot {S(L(v))} - {S(L(u))} \\cdot {S(L(ExpressionMath.Differentiate(v)))}}}{{{Base(L(v))}^{{2}}}}";
            }
            else if (numerator.Count + denominator.Count >= 2)
            {
                var all = factors.Where(ExpressionMath.ContainsVariable).ToList();
                var u = all[0];
                var v = Multiply(all.Skip(1));
                steps.Add(Rule("Korrutis", "(u \\cdot v)' = u' v + u v'"));
                steps.AddRange(Parts(("u", u), ("v", v)));
                innerExpression = $"{S(L(ExpressionMath.Differentiate(u)))} \\cdot {S(L(v))} + {S(L(u))} \\cdot {S(L(ExpressionMath.Differentiate(v)))}";
            }
            else
            {
                innerExpression = Explain(variablePart, steps) ?? L(ExpressionMath.Differentiate(variablePart));
            }

            if (constants.Count == 0)
            {
                return innerExpression;
            }

            return cLatex == "-" ? $"-{S(innerExpression)}" : $"{cLatex} \\cdot {S(innerExpression)}";
        }

        private static string? ChainRule(Expression f, List<SolutionStep> steps)
        {
            Expression? innerExpression = f switch
            {
                Expression.Function fn => fn.Item2,
                Expression.Power { Item2: Expression.Number } p => p.Item1,
                Expression.Power p when !ExpressionMath.ContainsVariable(p.Item1) => p.Item2,
                _ => null
            };

            if (innerExpression == null || innerExpression is Expression.Identifier)
            {
                return null;
            }

            var du = ExpressionMath.Differentiate(innerExpression);
            steps.Add(new(null, $"u = {L(innerExpression)}, \\quad u' = {L(du)}"));

            var outerDerivative = f switch
            {
                Expression.Function fn => FunctionDerivative(fn.Item1, L(innerExpression)),
                Expression.Power { Item2: Expression.Number n } => PowerDerivative(n.Item, L(innerExpression)),
                Expression.Power { Item1: Expression.Constant { Item.IsE: true } } => L(f),
                Expression.Power p => $"{L(f)} \\ln {S(L(p.Item1))}",
                _ => L(f)
            };

            return $"{outerDerivative} \\cdot {S(L(du))}";
        }

        private static (string, string) BasicRule(Expression f)
        {
            return f switch
            {
                Expression.Identifier => ("Muutuja tuletis", "(x)' = 1"),
                Expression.Power { Item1: Expression.Identifier, Item2: Expression.Number } => ("Astmefunktsiooni tuletis", "(x^{n})' = n x^{n - 1}"),
                Expression.Power { Item2: Expression.Number } => ("Liitfunktsiooni (astme) tuletis", "(u^{n})' = n u^{n - 1} \\cdot u'"),
                Expression.Power { Item1: Expression.Constant { Item.IsE: true } } p when p.Item2 is Expression.Identifier => ("Eksponentfunktsiooni tuletis", "(e^{x})' = e^{x}"),
                Expression.Power { Item1: Expression.Constant { Item.IsE: true } } => ("Liitfunktsiooni (eksponendi) tuletis", "(e^{u})' = e^{u} \\cdot u'"),
                Expression.Power p when !ExpressionMath.ContainsVariable(p.Item1) => ("Eksponentfunktsiooni tuletis", p.Item2 is Expression.Identifier ? "(a^{x})' = a^{x} \\ln a" : "(a^{u})' = a^{u} \\ln a \\cdot u'"),
                Expression.Power => ("Muutuva astendajaga aste (logaritmiline diferentseerimine)", "(u^{v})' = u^{v} \\left(v' \\ln u + \\frac{v u'}{u}\\right)"),
                Expression.Function fn when fn.Item2 is Expression.Identifier => ("Tuletiste tabelist", $"({FunctionLatex(fn.Item1, "x")})' = {FunctionDerivative(fn.Item1, "x")}"),
                Expression.Function fn => ("Liitfunktsiooni tuletis (ahelreegel)", $"({FunctionLatex(fn.Item1, "u")})' = {FunctionDerivative(fn.Item1, "u")} \\cdot u'"),
                _ => ("Tuletis", $"\\left({L(f)}\\right)'")
            };
        }

        private static string? ShortRule(Expression f)
        {
            if (!ExpressionMath.ContainsVariable(f))
            {
                return "konstandi tuletis on 0";
            }

            if (f is Expression.Product p)
            {
                var variableParts = p.Item.Where(ExpressionMath.ContainsVariable).ToList();
                if (variableParts.Any(IsInDenominator) && variableParts.Any(t => !IsInDenominator(t)))
                {
                    return "jagatise tuletis";
                }

                if (variableParts.Count >= 2)
                {
                    return "korrutise tuletis";
                }

                var rule = ShortRule(variableParts[0]);
                return p.Item.Length > variableParts.Count ? $"konstantne tegur ette, {rule}" : rule;
            }

            return f switch
            {
                Expression.Identifier => "muutuja tuletis on 1",
                Expression.Power { Item1: Expression.Identifier, Item2: Expression.Number } => "astme tuletis",
                Expression.Power { Item2: Expression.Number } => "liitfunktsiooni tuletis",
                Expression.Power power when !ExpressionMath.ContainsVariable(power.Item1) => "eksponentfunktsiooni tuletis",
                Expression.Function { Item2: Expression.Identifier } => "tuletiste tabelist",
                Expression.Function => "liitfunktsiooni tuletis",
                _ => null
            };
        }

        private static string FunctionLatex(Function f, string u)
        {
            return f.IsExp ? $"e^{{{u}}}" : $"{FunctionName(f)} {u}";
        }

        private static string FunctionDerivative(Function f, string u)
        {
            var su = u.Length == 1 ? $" {u}" : $"\\left({u}\\right)";
            if (f.IsSin)
            {
                return $"\\cos{su}";
            }

            if (f.IsCos)
            {
                return $"-\\sin{su}";
            }

            if (f.IsTan)
            {
                return $"\\frac{{1}}{{\\cos^{{2}}{su}}}";
            }

            if (f.IsCot)
            {
                return $"-\\frac{{1}}{{\\sin^{{2}}{su}}}";
            }

            if (f.IsSec)
            {
                return $"\\frac{{\\sin{su}}}{{\\cos^{{2}}{su}}}";
            }

            if (f.IsCsc)
            {
                return $"-\\frac{{\\cos{su}}}{{\\sin^{{2}}{su}}}";
            }

            if (f.IsAsin)
            {
                return $"\\frac{{1}}{{\\sqrt{{1 - {S(u)}^{{2}}}}}}";
            }

            if (f.IsAcos)
            {
                return $"-\\frac{{1}}{{\\sqrt{{1 - {S(u)}^{{2}}}}}}";
            }

            if (f.IsAtan)
            {
                return $"\\frac{{1}}{{1 + {S(u)}^{{2}}}}";
            }

            if (f.IsAcot)
            {
                return $"-\\frac{{1}}{{1 + {S(u)}^{{2}}}}";
            }

            if (f.IsSinh)
            {
                return $"\\cosh{su}";
            }

            if (f.IsCosh)
            {
                return $"\\sinh{su}";
            }

            if (f.IsTanh)
            {
                return $"\\frac{{1}}{{\\cosh^{{2}}{su}}}";
            }

            if (f.IsExp)
            {
                return $"e^{{{u}}}";
            }

            if (f.IsLn)
            {
                return $"\\frac{{1}}{{{u}}}";
            }

            if (f.IsLg)
            {
                return $"\\frac{{1}}{{{S(u)} \\ln 10}}";
            }

            return $"{FunctionName(f)}'{su}";
        }

        private static string FunctionName(Function f)
        {
            if (f.IsSin) return "\\sin";
            if (f.IsCos) return "\\cos";
            if (f.IsTan) return "\\tan";
            if (f.IsCot) return "\\cot";
            if (f.IsSec) return "\\sec";
            if (f.IsCsc) return "\\csc";
            if (f.IsAsin) return "\\arcsin";
            if (f.IsAcos) return "\\arccos";
            if (f.IsAtan) return "\\arctan";
            if (f.IsAcot) return "\\operatorname{arccot}";
            if (f.IsSinh) return "\\sinh";
            if (f.IsCosh) return "\\cosh";
            if (f.IsTanh) return "\\tanh";
            if (f.IsLn) return "\\ln";
            if (f.IsLg) return "\\lg";
            return "\\exp";
        }

        private static string PowerDerivative(MathNet.Numerics.BigRational n, string u)
        {
            var nextPower = L(Expression.NewNumber(n - MathNet.Numerics.BigRational.One));
            return $"{S(L(Expression.NewNumber(n)))} \\cdot {Base(u)}^{{{nextPower}}}";
        }

        private static SolutionStep Rule(string title, string formula)
        {
            return new($"**{title}:**", formula);
        }

        private static IEnumerable<SolutionStep> Parts(params (string name, Expression expression)[] parts)
        {
            foreach (var (name, expression) in parts)
            {
                yield return new(null, $"{name} = {L(expression)}, \\quad {name}' = {L(ExpressionMath.Differentiate(expression))}");
            }
        }

        private static bool IsInDenominator(Expression t)
        {
            return t is Expression.Power { Item2: Expression.Number n } && ExpressionMath.IsNegative(n.Item);
        }

        private static Expression Multiply(IEnumerable<Expression> factors)
        {
            return factors.Aggregate(Expression.One, Operators.multiply);
        }

        private static string CoefficientLatex(Expression c)
        {
            var text = L(c);
            return text switch
            {
                "1" => "",
                "-1" => "-",
                _ => S(text)
            };
        }

        private static string L(Expression e)
        {
            return ExpressionFormatter.Latex(e);
        }

        private static string S(string latex)
        {
            return ExpressionFormatter.Parenthesize(latex);
        }

        private static string Base(string latex)
        {
            return latex.Length == 1 ? latex : $"\\left({latex}\\right)";
        }

        private static string Sum(List<string> terms)
        {
            var result = terms[0];
            foreach (var l in terms.Skip(1))
            {
                result += l.StartsWith('-') ? $" - {l[1..]}" : $" + {l}";
            }

            return result;
        }

        private static string Capitalize(string s)
        {
            return s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
        }
    }
}
