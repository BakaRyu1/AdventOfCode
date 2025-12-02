using AdventOfCode.Utils;

namespace AdventOfCode._2025;
internal class Day02 : DayRunner<(long first, long last)[]>
{
    public override (long first, long last)[] Parse(FileReference file)
    {
        var ranges = new List<(long, long)>();
        var text = file.GetText().AsSpan();
        foreach (var range in text.Split(','))
        {
            var rangeStr = text[range];
            var pos = rangeStr.IndexOf('-');
            var first = rangeStr[..pos].ToLong();
            var last = rangeStr[(pos + 1)..].ToLong();
            ranges.Add((first, last));
        }
        return [.. ranges];
    }

    public override void Part1((long first, long last)[] data, RunSettings settings)
    {
        var sum = 0L;
        foreach (var (first, last) in data)
        {
            var lastArray = ToNumberArray(last);
            for (var num = ToNumberArray(first); CompareNumbers(num, lastArray) <= 0; num = IncrementNumber(num, num.Length - 1))
            {
                if ((num.Length % 2) != 0)
                {
                    for (var i = 0; i < num.Length; ++i)
                        num[i] = '9';
                    continue;
                }
                if (IsInvalidId_Part1(num))
                {
                    var i = num.AsSpan().ToLong();
                    if (settings.Verbose)
                        Console.WriteLine($"Found {i} in range [{first}-{last}]");
                    sum += i;
                }
            }
        }
        Console.WriteLine($"The sum of invalid ids is {sum}");
    }

    public override void Part2((long first, long last)[] data, RunSettings settings)
    {
        var sum = 0L;
        foreach (var (first, last) in data)
        {
            var lastArray = ToNumberArray(last);
            for (var num = ToNumberArray(first); CompareNumbers(num, lastArray) <= 0; num = IncrementNumber(num, num.Length - 1))
            {
                if (IsInvalidId_Part2(num))
                {
                    var i = num.AsSpan().ToLong();
                    if (settings.Verbose)
                        Console.WriteLine($"Found {i} in range [{first}-{last}]");
                    sum += i;
                }
            }
        }
        Console.WriteLine($"The sum of actual invalid ids is {sum}");
    }

    private static bool IsInvalidId_Part1(ReadOnlySpan<char> str)
    {
        if ((str.Length % 2) != 0)
            return false;
        var halfLength = str.Length / 2;
        return str[..halfLength].SequenceEqual(str[halfLength..]);
    }

    private static bool IsInvalidId_Part2(ReadOnlySpan<char> str)
    {
        var maxPatternLen = str.Length / 2;
        for (var patternLen = 1; patternLen <= maxPatternLen; ++patternLen)
        {
            if ((str.Length % patternLen) != 0)
                continue;
            var pattern = str[..patternLen];
            var patternMatch = true;
            for (var pos = patternLen; pos < str.Length; pos += patternLen)
            {
                if (!pattern.SequenceEqual(str.Slice(pos, patternLen)))
                {
                    patternMatch = false;
                    break;
                }
            }
            if (patternMatch)
                return true;
        }
        return false;
    }

    private static char[] ToNumberArray(long number) => number.ToString().ToCharArray();

    private static char[] IncrementNumber(char[] number, int pos = -1)
    {
        if (pos < 0)
            pos = number.Length - 1;
        var ch = ++number[pos];
        if (ch <= '9')
            return number;
        number[pos] = '0';
        if (pos == 0)
            return ['1', .. Enumerable.Repeat('0', number.Length)];
        return IncrementNumber(number, pos - 1);
    }

    private static int CompareNumbers(char[] a, char[] b)
    {
        if (a.Length != b.Length)
            return a.Length - b.Length;
        for (var i = 0; i < a.Length; ++i)
        {
            if (a[i] != b[i])
                return a[i] - b[i];
        }
        return 0;
    }
}
