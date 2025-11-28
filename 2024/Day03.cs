using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2024;

internal partial class Day03 : DayRunner<string>
{
    public override string Parse(FileReference file)
    {
        return file.GetText();
    }
    public override void Part1(string text, RunSettings settings)
    {
        var sum = 0;
        var executed = 0;
        var match = MulPattern().Match(text);
        while (match.Success)
        {
            var num1 = match.Groups["num1"].ValueSpan.ToInt();
            var num2 = match.Groups["num2"].ValueSpan.ToInt();
            sum += num1 * num2;
            ++executed;
            match = match.NextMatch();
        }
        if (settings.Verbose)
            Console.WriteLine("Mul executed: " + executed);
        Console.WriteLine("Sum: " + sum);
    }
    public override void Part2(string text, RunSettings settings)
    {
        var sum = 0;
        var executed = 0;
        var ignored = 0;
        var enabled = true;
        var match = EnablingMulPattern().Match(text);
        while (match.Success)
        {
            switch (match.Groups["op"].Value)
            {
                case "mul":
                    if (enabled)
                    {
                        var num1 = match.Groups["num1"].ValueSpan.ToInt();
                        var num2 = match.Groups["num2"].ValueSpan.ToInt();
                        sum += num1 * num2;
                        ++executed;
                    }
                    else
                        ++ignored;
                    break;
                case "do":
                    enabled = true;
                    break;
                case "don't":
                    enabled = false;
                    break;
            }
            match = match.NextMatch();
        }
        if (settings.Verbose)
        {
            Console.WriteLine("Mul executed: " + executed);
            Console.WriteLine("Mul ignored: " + ignored);
        }
        Console.WriteLine("Sum of enabled mul: " + sum);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day03), settings.Example ? "day03-example.txt" : "day03-input.txt");
    }

    [GeneratedRegex(@"mul\((?<num1>\d+),(?<num2>\d+)\)", RegexOptions.CultureInvariant)]
    private static partial Regex MulPattern();
    [GeneratedRegex(@"((?<op>mul)\((?<num1>\d+),(?<num2>\d+)\)|(?<op>do(?:n't)?)\(\))", RegexOptions.CultureInvariant)]
    private static partial Regex EnablingMulPattern();
}
