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
            for (var i = first; i <= last; ++i)
            {
                if (IsInvalidId_Part1(i))
                {
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
            for (var i = first; i <= last; ++i)
            {
                if (IsInvalidId_Part2(i))
                {
                    if (settings.Verbose)
                        Console.WriteLine($"Found {i} in range [{first}-{last}]");
                    sum += i;
                }
            }
        }
        Console.WriteLine($"The sum of actual invalid ids is {sum}");
    }

    private static bool IsInvalidId_Part1(long number)
    {
        var str = number.ToString();
        if ((str.Length % 2) != 0)
            return false;
        var halfLength = str.Length / 2;
        return str.AsSpan(0, halfLength).SequenceEqual(str.AsSpan(halfLength));
    }

    private static bool IsInvalidId_Part2(long number)
    {
        var str = number.ToString();
        var maxPatternLen = str.Length / 2;
        for (var patternLen = 1; patternLen <= maxPatternLen; ++patternLen)
        {
            if ((str.Length % patternLen) != 0)
                continue;
            var pattern = str.AsSpan(0, patternLen);
            var patternMatch = true;
            for (var pos = patternLen; pos < str.Length; pos += patternLen)
            {
                if (!pattern.SequenceEqual(str.AsSpan(pos, patternLen)))
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
}
