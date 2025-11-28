using System.Globalization;
using System.Text.RegularExpressions;

namespace AdventOfCode.Utils;

internal static partial class StringUtils
{
    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex SpacePattern();

    public static IEnumerable<int> SplitInts(this ReadOnlySpan<char> input, char separator)
    {
        var list = new List<int>();
        foreach (var range in input.Split(separator))
        {
            list.Add(input[range].ToInt());
        }
        return list;
    }
    public static IEnumerable<int> SplitInts(this ReadOnlySpan<char> input, Regex separator)
    {
        var list = new List<int>();
        foreach (var range in separator.EnumerateSplits(input))
        {
            list.Add(input[range].ToInt());
        }
        return list;
    }
    public static IEnumerable<int> SplitInts(this ReadOnlySpan<char> input) => input.SplitInts(SpacePattern());

    public static IEnumerable<long> SplitLongs(this ReadOnlySpan<char> input, char separator)
    {
        var list = new List<long>();
        foreach (var range in input.Split(separator))
        {
            list.Add(input[range].ToLong());
        }
        return list;
    }
    public static IEnumerable<long> SplitLongs(this ReadOnlySpan<char> input, Regex separator)
    {
        var list = new List<long>();
        foreach (var range in separator.EnumerateSplits(input))
        {
            list.Add(input[range].ToLong());
        }
        return list;
    }
    public static IEnumerable<long> SplitLongs(this ReadOnlySpan<char> input) => input.SplitLongs(SpacePattern());

    public static IEnumerable<string> SplitAsStrings(this ReadOnlySpan<char> input, char separator, bool trim = false)
    {
        var list = new List<string>();
        foreach (var range in input.Split(separator))
        {
            var span = input[range];
            if (trim)
                span = span.Trim();
            list.Add(span.ToString());
        }
        return list;
    }
    public static IEnumerable<string> SplitAsStrings(this ReadOnlySpan<char> input, Regex separator, bool trim = false)
    {
        var list = new List<string>();
        foreach (var range in separator.EnumerateSplits(input))
        {
            var span = input[range];
            if (trim)
                span = span.Trim();
            list.Add(span.ToString());
        }
        return list;
    }
    public static IEnumerable<string> SplitAsStrings(this ReadOnlySpan<char> input) => input.SplitAsStrings(SpacePattern());
    public static int ToInt(this ReadOnlySpan<char> input) => int.Parse(input.Trim(), CultureInfo.InvariantCulture);
    public static int ToInt(this Span<char> input) => int.Parse(input.Trim(), CultureInfo.InvariantCulture);
    public static long ToLong(this ReadOnlySpan<char> input) => long.Parse(input.Trim(), CultureInfo.InvariantCulture);
    public static long ToLong(this Span<char> input) => long.Parse(input.Trim(), CultureInfo.InvariantCulture);
}
