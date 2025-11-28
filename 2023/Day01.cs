using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2023;

internal partial class Day01 : DayRunner<string[]>
{
    public override string[] Parse(FileReference file)
    {
        return file.GetLines().ToArray();
    }

    public override void Part1(string[] lines, RunSettings settings)
    {
        var sum = 0;
        foreach (var line in lines)
        {
            var matches = DigitsPattern().Matches(line);
            var value = (matches.First().Value + matches.Last().Value).AsSpan().ToInt();
            sum += value;
        }
        Console.WriteLine("Sum is " + sum);
    }

    public override void Part2(string[] lines, RunSettings settings)
    {
        var sum = 0;
        foreach (var line in lines)
        {
            var matches = DigitsWithLettersPattern().Matches(line);
            var value = DigitValue(matches.First().Groups[1].Value) * 10 + DigitValue(matches.Last().Groups[1].Value);
            sum += value;
        }
        Console.WriteLine("Sum (with letters) is " + sum);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day01), settings.Example ? "day01-example1.txt" : "day01-input.txt");
        settings.File2 ??= (settings.Example ? FileReference.Resource(typeof(Day01), "day01-example2.txt") : null);
    }

    [GeneratedRegex(@"\d", RegexOptions.CultureInvariant)]
    private static partial Regex DigitsPattern();
    [GeneratedRegex(@"(?=(\d|zero|one|two|three|four|five|six|seven|eight|nine))", RegexOptions.CultureInvariant)]
    private static partial Regex DigitsWithLettersPattern();

    private static int DigitValue(string digit)
    {
        return digit switch
        {
            "0" or "zero" => 0,
            "1" or "one" => 1,
            "2" or "two" => 2,
            "3" or "three" => 3,
            "4" or "four" => 4,
            "5" or "five" => 5,
            "6" or "six" => 6,
            "7" or "seven" => 7,
            "8" or "eight" => 8,
            "9" or "nine" => 9,
            _ => throw new InvalidOperationException(),
        };
    }
}
