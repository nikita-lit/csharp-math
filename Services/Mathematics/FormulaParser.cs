using MathNet.Symbolics;
using System.Globalization;
using System.Numerics;

namespace csharp_math.Services.Mathematics
{
    public sealed class FormulaParseException(string message) : Exception(message);

    public sealed record ParsedFormula(Expression Expression, IReadOnlyList<DomainConstraint> Constraints);

    /// <summary>
    /// Parses formulas with conventional precedence, implicit multiplication and domain constraints.
    /// </summary>
    public static class FormulaParser
    {
        private static readonly Dictionary<string, Function> Functions = new()
        {
            ["sin"] = Function.Sin,
            ["cos"] = Function.Cos,
            ["exp"] = Function.Exp,
            ["ln"] = Function.Ln,
        };

        private static readonly string[] SpecialFunctions = ["sqrt", "abs"];
        private static readonly string[] Names = Functions.Keys.Concat(SpecialFunctions).Concat(["pi", "π", "x", "e"]).OrderByDescending(n => n.Length).ToArray();

        public static ParsedFormula Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new FormulaParseException("Valem on tühi.");
            }

            // Preserve constraints before symbolic simplification can cancel denominators.
            var reader = new Reader(Tokenize(input));
            var expression = reader.ReadAll();
            return new ParsedFormula(expression, reader.Constraints.All);
        }

        private enum Kind
        {
            NumericLiteral,
            Name,
            Sign,
            End
        }

        private readonly record struct Token(Kind Kind, string Text, int Position);

        private static List<Token> Tokenize(string s)
        {
            var result = new List<Token>();
            var i = 0;
            while (i < s.Length)
            {
                var c = s[i];
                var pos = i + 1;
                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                if (char.IsDigit(c) || (c is '.' or ',' && i + 1 < s.Length && char.IsDigit(s[i + 1])))
                {
                    var start = i;
                    while (i < s.Length && char.IsDigit(s[i]))
                    {
                        i++;
                    }

                    if (i < s.Length && s[i] is '.' or ',' && i + 1 < s.Length && char.IsDigit(s[i + 1]))
                    {
                        i++;
                        while (i < s.Length && char.IsDigit(s[i]))
                        {
                            i++;
                        }
                    }

                    result.Add(new Token(Kind.NumericLiteral, s[start..i].Replace(',', '.'), pos));
                    continue;
                }

                if (char.IsLetter(c))
                {
                    var start = i;
                    while (i < s.Length && char.IsLetter(s[i]))
                    {
                        i++;
                    }

                    SplitWord(s[start..i].ToLowerInvariant(), start, result);
                    continue;
                }

                switch (c)
                {
                    case '+':
                        result.Add(new Token(Kind.Sign, "+", pos));
                        break;
                    case '-' or '−' or '–':
                        result.Add(new Token(Kind.Sign, "-", pos));
                        break;
                    case '*' or '·' or '×' or '⋅':
                        result.Add(new Token(Kind.Sign, "*", pos));
                        break;
                    case '/' or '÷' or ':':
                        result.Add(new Token(Kind.Sign, "/", pos));
                        break;
                    case '^':
                        result.Add(new Token(Kind.Sign, "^", pos));
                        break;
                    case '(' or '[' or '{':
                        result.Add(new Token(Kind.Sign, "(", pos));
                        break;
                    case ')' or ']' or '}':
                        result.Add(new Token(Kind.Sign, ")", pos));
                        break;
                    case '²':
                        result.Add(new Token(Kind.Sign, "^", pos));
                        result.Add(new Token(Kind.NumericLiteral, "2", pos));
                        break;
                    case '³':
                        result.Add(new Token(Kind.Sign, "^", pos));
                        result.Add(new Token(Kind.NumericLiteral, "3", pos));
                        break;
                    case '√':
                        result.Add(new Token(Kind.Name, "sqrt", pos));
                        break;
                    default:
                        throw new FormulaParseException($"Tundmatu märk '{c}' (positsioon {pos}).");
                }

                i++;
            }

            result.Add(new Token(Kind.End, "", s.Length + 1));
            return result;
        }

        private static void SplitWord(string word, int start, List<Token> result)
        {
            var i = 0;
            while (i < word.Length)
            {
                var name = Names.FirstOrDefault(n => string.CompareOrdinal(word, i, n, 0, n.Length) == 0);
                if (name == null)
                {
                    throw new FormulaParseException($"Tundmatu tähis '{word[i..]}' (positsioon {start + i + 1}). Muutuja peab olema x; lubatud funktsioonid: " + "sin, cos, exp, ln, sqrt; konstandid pi ja e.");
                }

                result.Add(new Token(Kind.Name, name == "π" ? "pi" : name, start + i + 1));
                i += name.Length;
            }
        }

        private sealed class Reader(List<Token> tokens)
        {
            private int _i;

            public ConstraintCollector Constraints { get; } = new();

            private Token Current => tokens[_i];

            private bool Is(string sign)
            {
                return Current.Kind == Kind.Sign && Current.Text == sign;
            }

            public Expression ReadAll()
            {
                var expression = Sum();
                if (Current.Kind != Kind.End)
                {
                    throw Error(Current.Text == ")" ? "Liigne sulg ')'" : $"Ootamatu '{Current.Text}'");
                }

                return expression;
            }

            private Expression Sum()
            {
                var v = Term();
                while (Is("+") || Is("-"))
                {
                    var negative = Is("-");
                    _i++;
                    var r = Term();
                    v = negative ? Operators.subtract(v, r) : Operators.add(v, r);
                }

                return v;
            }

            private Expression Term()
            {
                var v = Unary();
                while (true)
                {
                    if (Is("*"))
                    {
                        _i++;
                        v = Operators.multiply(v, Unary());
                    }
                    else if (Is("/"))
                    {
                        _i++;
                        var position = Current.Position;
                        var denominator = Unary();
                        if (!Constraints.ForDivision(denominator))
                        {
                            throw new FormulaParseException($"Jagamine nulliga (positsioon {position}).");
                        }

                        v = Operators.divide(v, denominator);
                    }
                    else if (Current.Kind is Kind.NumericLiteral or Kind.Name || Is("("))
                    {
                        v = Operators.multiply(v, Power());
                    }
                    else
                    {
                        return v;
                    }
                }
            }

            // Unary signs bind after powers: -x^2 means -(x^2).
            private Expression Unary()
            {
                if (Is("-"))
                {
                    _i++;
                    return Operators.negate(Unary());
                }

                if (Is("+"))
                {
                    _i++;
                    return Unary();
                }

                return Power();
            }

            private Expression Power()
            {
                var baseExpression = Primary();
                if (!Is("^"))
                {
                    return baseExpression;
                }

                var position = Current.Position;
                _i++;
                var exponent = Unary();
                return PowerWithConstraint(baseExpression, exponent, position);
            }

            private Expression PowerWithConstraint(Expression baseExpression, Expression exponent, int position)
            {
                if (!Constraints.ForPower(baseExpression, exponent))
                {
                    throw new FormulaParseException($"Aste ei ole reaalarvudes määratud (positsioon {position}).");
                }

                return Operators.pow(baseExpression, exponent);
            }

            private Expression Primary()
            {
                var m = Current;
                switch (m.Kind)
                {
                    case Kind.NumericLiteral:
                        _i++;
                        return ParseNumber(m.Text);

                    case Kind.Sign when m.Text == "(":
                        {
                            _i++;
                            var inner = Sum();
                            if (!Is(")"))
                            {
                                throw Error("Puudub sulgev sulg ')'");
                            }

                            _i++;
                            return inner;
                        }

                    case Kind.Name:
                        _i++;
                        return m.Text switch
                        {
                            "x" => ExpressionMath.X,
                            "pi" => Expression.Pi,
                            "e" => Expression.E,
                            "abs" => throw new FormulaParseException("abs() ei ole toetatud: absoluutväärtusega funktsiooni ei saa siin uurida."),
                            "sqrt" => PowerWithConstraint(Argument(), ExpressionMath.Rational(1, 2), m.Position),
                            _ => FunctionWithConstraint(Functions[m.Text], Argument(), m.Position)
                        };

                    case Kind.End:
                        throw Error("Valem lõppes ootamatult");

                    default:
                        throw Error($"Ootamatu '{m.Text}'");
                }
            }

            private Expression Argument()
            {
                if (Current.Kind == Kind.End || Is(")") || Is("+") || Is("*") || Is("/") || Is("^"))
                {
                    throw Error("Funktsioonil puudub argument");
                }

                return Is("(") ? Primary() : Power();
            }

            private Expression FunctionWithConstraint(Function f, Expression argument, int position)
            {
                if (!Constraints.ForFunction(f, argument))
                {
                    throw new FormulaParseException($"Funktsioon ei ole selles punktis määratud (positsioon {position}).");
                }

                return Operators.apply(f, argument);
            }

            private static Expression ParseNumber(string text)
            {
                var point = text.IndexOf('.');
                if (point < 0)
                {
                    return ExpressionMath.Rational(BigInteger.Parse(text, CultureInfo.InvariantCulture), 1);
                }

                var decimalPlaces = text.Length - point - 1;
                var numerator = BigInteger.Parse(text.Remove(point, 1), CultureInfo.InvariantCulture);
                return ExpressionMath.Rational(numerator, BigInteger.Pow(10, decimalPlaces));
            }

            private FormulaParseException Error(string message)
            {
                return new($"{message} (positsioon {Current.Position}).");
            }
        }
    }
}
