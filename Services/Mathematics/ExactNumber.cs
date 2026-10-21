using System.Globalization;
using System.Text.RegularExpressions;

namespace csharp_math.Services.Mathematics
{
    /// <summary>
    /// Recognizes fractions, radicals, multiples of pi and powers of e in numerical results.
    /// </summary>
    public static class ExactNumber
    {
        private const double Precision = 1e-9;

        private static readonly (double power, string text)[] EPowers = new[]
        {
            (1.0, "e"),
            (0.5, "√e"),
            (2.0, "e²"),
            (1.5, "e√e"),
            (3.0, "e³"),
            (3.5, "e³√e"),
            (4.0, "e⁴")
        }.SelectMany(a => new[] { a, (-a.Item1, a.Item2) }).ToArray();

        public static string Decimal(double v)
        {
            var text = Math.Round(v, 4).ToString("0.####", CultureInfo.InvariantCulture);
            return text == "-0" ? "0" : text;
        }

        public static string Short(double v)
        {
            if (double.IsPositiveInfinity(v))
            {
                return "∞";
            }

            if (double.IsNegativeInfinity(v))
            {
                return "-∞";
            }

            return ExactForm(v) ?? Decimal(v);
        }

        public static string Latex(double v)
        {
            if (double.IsPositiveInfinity(v))
            {
                return "\\infty";
            }

            if (double.IsNegativeInfinity(v))
            {
                return "-\\infty";
            }

            var exact = ExactForm(v);
            return exact != null ? TextToLatex(exact) : Decimal(v);
        }

        public static string EqualityLatex(string name, double v)
        {
            var exact = ExactForm(v);
            var decimalText = Decimal(v);
            if (exact == null)
            {
                return $"{name} \\approx {decimalText}";
            }

            return exact == decimalText ? $"{name} = {decimalText}" : $"{name} = {TextToLatex(exact)} \\approx {decimalText}";
        }

        public static bool HasExactForm(double v)
        {
            return ExactForm(v) != null;
        }

        public static string Equality(string name, double v)
        {
            var exact = ExactForm(v);
            var decimalText = Decimal(v);
            if (exact == null)
            {
                return $"{name} ≈ {decimalText}";
            }

            return exact == decimalText ? $"{name} = {exact}" : $"{name} = {exact} ≈ {decimalText}";
        }

        public static double Snap(double v)
        {
            var nearest = Math.Round(v);
            return (Math.Abs(v - nearest) < Precision * Math.Max(1, Math.Abs(v)) ? nearest : v) + 0.0;
        }

        public static string? ExactForm(double v)
        {
            if (!double.IsFinite(v))
            {
                return null;
            }

            var tol = Precision * Math.Max(1, Math.Abs(v));
            if (Math.Abs(v - Math.Round(v)) < tol)
            {
                return Decimal(Math.Round(v));
            }

            if (Math.Abs(v * 1e4 - Math.Round(v * 1e4)) < tol * 1e4)
            {
                return Decimal(v);
            }

            var fraction = Fraction(v, tol);
            if (fraction != null)
            {
                return FormatFraction(fraction.Value.p, fraction.Value.q, "", "");
            }

            var pi = Fraction(v / Math.PI, tol / Math.PI, maxNumerator: 60);
            if (pi != null)
            {
                return FormatFraction(pi.Value.p, pi.Value.q, "π", "");
            }

            var root = Radical(v, tol);
            if (root != null)
            {
                return root;
            }

            if (Math.Abs(v) < 100)
            {
                var squared = Radical(v * v, tol);
                if (squared != null)
                {
                    return (v < 0 ? "-" : "") + $"√({squared})";
                }
            }

            foreach (var (power, text) in EPowers)
            {
                var m = Fraction(v / Math.Exp(power), tol);
                if (m != null)
                {
                    return power > 0
                        ? FormatFraction(m.Value.p, m.Value.q, text, "")
                        : FormatFraction(m.Value.p, m.Value.q, "", text);
                }
            }

            var exponentialValue = Math.Exp(Math.Abs(v));
            if (Math.Abs(v) < 20)
            {
                var ln = Fraction(exponentialValue, tol * exponentialValue, maxNumerator: 100);
                if (ln != null)
                {
                    return (v < 0 ? "-" : "") + (ln.Value.q == 1 ? $"ln {ln.Value.p}" : $"ln({ln.Value.p}/{ln.Value.q})");
                }
            }

            return null;
        }

        private static string TextToLatex(string t)
        {
            if (t.StartsWith("-√("))
            {
                return "-" + TextToLatex(t[1..]);
            }

            if (t.StartsWith("√(") && t.EndsWith(')'))
            {
                return "\\sqrt{" + TextToLatex(t[2..^1]) + "}";
            }

            static string Part(string s)
            {
                if (s.StartsWith('(') && s.EndsWith(')'))
                {
                    s = s[1..^1];
                }

                s = Regex.Replace(s, @"√(\d+|e)", m => $"\\sqrt{{{m.Groups[1].Value}}}");
                return s.Replace("π", "\\pi").Replace("e²", "e^{2}").Replace("e³", "e^{3}").Replace("e⁴", "e^{4}");
            }

            if (t.Contains("ln"))
            {
                var m = Regex.Match(t, @"^(-?)ln\((\d+)/(\d+)\)$");
                return m.Success ? $"{m.Groups[1].Value}\\ln\\frac{{{m.Groups[2].Value}}}{{{m.Groups[3].Value}}}" : t.Replace("ln ", "\\ln ");
            }

            if (t.Count(c => c == '/') != 1)
            {
                return Part(t);
            }

            var sign = t.StartsWith('-') ? "-" : "";
            var unsignedText = t[sign.Length..];
            var divisorIndex = unsignedText.IndexOf('/');

            return $"{sign}\\frac{{{Part(unsignedText[..divisorIndex])}}}{{{Part(unsignedText[(divisorIndex + 1)..])}}}";
        }

        private static (long p, long q)? Fraction(double v, double tol, int maxDenominator = 12, int maxNumerator = 1000)
        {
            for (long q = 1; q <= maxDenominator; q++)
            {
                var p = Math.Round(v * q);
                if (Math.Abs(p) <= maxNumerator && p != 0 && Math.Abs(v - p / q) < tol)
                {
                    return ((long)p, q);
                }
            }

            return null;
        }

        private static string? Radical(double v, double tol)
        {
            long[] candidateCoefficients = [1, 2, 3, 4, 5, 6, 7, 8, 10, 12];
            foreach (var c in candidateCoefficients)
            {
                for (var k = 0; k <= 24; k++)
                {
                    long p = k % 2 == 0 ? k / 2 : -(k + 1) / 2;
                    var t = v * c - p;
                    var m = Math.Round(t * t);
                    if (m < 2 || m > 1000)
                    {
                        continue;
                    }

                    var squareRoot = Math.Sqrt(m);
                    if (squareRoot == Math.Floor(squareRoot))
                    {
                        continue;
                    }

                    if (Math.Abs(Math.Abs(t) - squareRoot) >= tol * c)
                    {
                        continue;
                    }

                    var (coefficient, radicand) = ExtractSquareFactor((long)m);
                    var s = Math.Sign(t) * coefficient;
                    var commonDivisor = Gcd(Gcd(Math.Abs(p), Math.Abs(s)), c);
                    p /= commonDivisor;
                    s /= commonDivisor;
                    var denominator = c / commonDivisor;
                    var radicalPart = (Math.Abs(s) == 1 ? "" : Math.Abs(s).ToString(CultureInfo.InvariantCulture)) + "√" + radicand;
                    string numerator = p == 0 ? (s < 0 ? "-" : "") + radicalPart : $"{p} {(s < 0 ? "-" : "+")} {radicalPart}";
                    if (denominator == 1)
                    {
                        return numerator;
                    }

                    return p == 0 ? $"{numerator}/{denominator}" : $"({numerator})/{denominator}";
                }
            }

            return null;
        }

        private static (long coefficient, long radicand) ExtractSquareFactor(long m)
        {
            long coefficient = 1;
            for (long i = 2; i * i <= m; i++)
            {
                while (m % (i * i) == 0)
                {
                    m /= i * i;
                    coefficient *= i;
                }
            }

            return (coefficient, m);
        }

        private static long Gcd(long a, long b)
        {
            return b == 0 ? Math.Max(a, 1) : Gcd(b, a % b);
        }

        private static string FormatFraction(long p, long q, string numeratorConstant, string denominatorConstant)
        {
            var sign = p < 0 ? "-" : "";
            var ap = Math.Abs(p);
            var numerator = numeratorConstant == "" ? ap.ToString(CultureInfo.InvariantCulture) : ap == 1 ? numeratorConstant : $"{ap}{numeratorConstant}";
            var denominator = denominatorConstant == "" ? (q == 1 ? "" : q.ToString(CultureInfo.InvariantCulture)) : q == 1 ? (denominatorConstant.Length > 2 ? $"({denominatorConstant})" : denominatorConstant) : $"({q}{denominatorConstant})";
            return denominator == "" ? sign + numerator : $"{sign}{numerator}/{denominator}";
        }
    }
}
