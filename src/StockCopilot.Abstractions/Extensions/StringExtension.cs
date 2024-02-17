using System.Diagnostics.CodeAnalysis;

namespace StockCopilot.Abstractions.Extensions
{
    public static class StringExtension
    {
        public static string ToUpperCamelCase(this string value) => $"{(char)(value[0] - 32)}{value[1..]}";
        public static string ToLowerCamelCase(this string value) => $"{(char)(value[0] + 32)}{value[1..]}";
        public static string AnotherCamelCase(this string value) =>
            value.Length == 0
                ? throw new ArgumentNullException($"At least one char in string {value}")
                : value[0] switch
                {
                    >= 'a' and <= 'z' => value.ToUpperCamelCase(),
                    >= 'A' and <= 'Z' => value.ToLowerCamelCase(),
                    _ => throw new ArgumentOutOfRangeException($"First char should be letter but {value[0]}")
                };
        
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => 
            string.IsNullOrEmpty(str);
        
        public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) => 
            string.IsNullOrWhiteSpace(str);
        
        [return: NotNullIfNotNull(nameof(str))]
        public static string? SafeSubstring(this string? str, int startIndex, int length)
        {
            if (str is null) return null;
            if (startIndex < 0) startIndex = 0;
            if (startIndex >= str.Length) return string.Empty;
            if (length < 0) length = 0;
            if (startIndex + length > str.Length) length = str.Length - startIndex;
            return str.Substring(startIndex, length);
        }

        /// <summary>
        /// Force enumerate the source str
        /// </summary>
        /// <param name="str"></param>
        /// <param name="another"></param>
        /// <returns></returns>
        public static bool SafeEquals(this string? str, string? another)
        {
            if (str is null) return another is null;
            var match = true;
            for (var i = 0; i < str.Length; i++)
            {
                if (!match) continue;
                match = another != null && i < another.Length && str[i] == another[i];
            }

            return match;
        }
    }
}
