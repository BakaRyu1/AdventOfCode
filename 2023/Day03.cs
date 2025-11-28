using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2023;

internal partial class Day03 : DayRunner<Array2D<char>>
{
    public override Array2D<char> Parse(FileReference file)
    {
        var lines = file.GetLines().ToArray();

        var width = lines.Min(line => line.Length);
        if (lines.Any(line => line.Length != width))
        {
            Console.Error.WriteLine("Invalid schematic: at least one line isn't the same length.");
        }
        return Array2D<char>.From(lines);
    }

    public override void Part1(Array2D<char> schematic, RunSettings settings)
    {
        var sum = 0;
        for (int y = 0; y < schematic.Height; ++y)
        {
            var line = schematic.AsSpan(0, y);
            foreach (var match in NumberPattern().EnumerateMatches(line))
            {
                if (!HasSymbol(schematic, match.Index, y, match.Length))
                    continue;
                var partNumber = line.Slice(match.Index, match.Length).ToInt();
                sum += partNumber;
            }
        }
        Console.WriteLine("Sum of part IDs is " + sum);
    }

    public override void Part2(Array2D<char> schematic, RunSettings settings)
    {
        var sum = 0;
        for (var y = 0; y < schematic.Height; ++y)
        {
            for (var x = 0; x < schematic.Width; ++x)
            {
                if (schematic[x, y] != '*')
                    continue;
                var numbers = GetGearNumbers(schematic, x, y);
                if (numbers.Count != 2)
                    continue;
                var ratio = numbers[0] * numbers[1];
                sum += ratio;
            }
        }
        Console.WriteLine("Sum of part ratios is " + sum);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day03), settings.Example ? "day03-example.txt" : "day03-input.txt");
    }

    private static readonly System.Buffers.SearchValues<char> NonSymbols = System.Buffers.SearchValues.Create("0123456789.");
    private static readonly System.Buffers.SearchValues<char> Digits = System.Buffers.SearchValues.Create("0123456789");
    [GeneratedRegex(@"\d+", RegexOptions.CultureInvariant)]
    private static partial Regex NumberPattern();

    private static bool HasSymbol(Array2D<char> schematic, int x, int y, int length)
    {
        var startX = Math.Max(0, x - 1);
        var endX = Math.Min(schematic.Width - 1, x + length);
        var searchLength = endX - startX + 1;
        if (schematic.AsSpan(startX, y, 1).IndexOfAnyExcept(NonSymbols) >= 0)
            return true;
        if (schematic.AsSpan(endX, y, 1).IndexOfAnyExcept(NonSymbols) >= 0)
            return true;
        if (y > 0 && schematic.AsSpan(startX, y - 1, searchLength).IndexOfAnyExcept(NonSymbols) >= 0)
            return true;
        if ((y + 1) < schematic.Height && schematic.AsSpan(startX, y + 1, searchLength).IndexOfAnyExcept(NonSymbols) >= 0)
            return true;
        return false;
    }

    private static int FindNumberStart(Array2D<char> schematic, int x, int y)
    {
        if (!Digits.Contains(schematic[x, y]))
            return -1;
        var startX = schematic.AsSpan(0, y, x).LastIndexOfAnyExcept(Digits) + 1;
        return startX;
    }

    private static int GetGearNumber(Array2D<char> schematic, int x, int y)
    {
        var searchString = schematic.AsSpan(x, y);
        var length = searchString.IndexOfAnyExcept(Digits);
        if (length < 0)
            length = searchString.Length;
        return searchString[..length].ToInt();
    }

    private static List<int> GetGearNumbers(Array2D<char> schematic, int x, int y)
    {
        var maxX = schematic.Width - 1;
        var startY = Math.Max(0, y - 1);
        var endY = Math.Min(schematic.Height - 1, y + 1);
        var numbers = new List<int>();
        for (var cy = startY; cy <= endY; cy++)
        {
            var leftStart = x >= 1 ? FindNumberStart(schematic, x - 1, cy) : -1;
            if (leftStart >= 0)
                numbers.Add(GetGearNumber(schematic, leftStart, cy));
            var centerStart = FindNumberStart(schematic, x, cy);
            if (centerStart >= 0 && centerStart != leftStart)
                numbers.Add(GetGearNumber(schematic, centerStart, cy));
            var rightStart = (x + 1) <= maxX ? FindNumberStart(schematic, x + 1, cy) : -1;
            if (rightStart >= 0 && rightStart != leftStart && rightStart != centerStart)
                numbers.Add(GetGearNumber(schematic, rightStart, cy));
        }
        return numbers;
    }
}
