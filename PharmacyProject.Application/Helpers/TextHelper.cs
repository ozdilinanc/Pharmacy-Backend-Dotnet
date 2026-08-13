using System.Globalization;
using System.Text.RegularExpressions;

namespace PharmacyProject.Application.Helpers
{
    public static class TextHelper
    {
        public static string NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

            var digitsOnly = Regex.Replace(phone, @"\D", "");

            if (digitsOnly.StartsWith("90") && digitsOnly.Length > 10) digitsOnly = digitsOnly.Substring(2);
            if (digitsOnly.StartsWith("0")) digitsOnly = digitsOnly.Substring(1);

            return digitsOnly;
        }

        public static string NormalizeName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var lowerName = name.ToLower(new CultureInfo("tr-TR")).Trim();

            return lowerName.Replace("eczanesi", "")
                            .Replace("eczane", "")
                            .Replace("ecz.", "")
                            .Replace("ecz", "")
                            .Trim();
        }

        public static double CalculateSimilarity(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0.0;
            if (source == target) return 100.0;

            int distance = ComputeLevenshteinDistance(source, target);
            int maxLength = Math.Max(source.Length, target.Length);

            return (1.0 - ((double)distance / (double)maxLength)) * 100.0;
        }

        private static int ComputeLevenshteinDistance(string source, string target)
        {
            int n = source.Length;
            int m = target.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        public static string NormalizeLocationName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            return name.ToUpper(new System.Globalization.CultureInfo("tr-TR"))
                       .Replace("Ğ", "G")
                       .Replace("Ü", "U")
                       .Replace("Ş", "S")
                       .Replace("İ", "I")
                       .Replace("Ö", "O")
                       .Replace("Ç", "C")
                       .Replace(" ", "")
                       .Trim();
        }
    }
}