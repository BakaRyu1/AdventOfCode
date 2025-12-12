using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015;
internal partial class Day25 : DayRunner<(int row, int column)>
{
    public override (int row, int column) Parse(FileReference file)
    {
        var text = file.GetText();
        var match = TextPattern().Match(text);
        if (!match.Success)
            throw new InvalidOperationException($"Invalid input: {text}");
        var row = match.Groups["row"].ValueSpan.ToInt();
        var column = match.Groups["column"].ValueSpan.ToInt();
        return (row, column);
    }

    public override void Part1((int row, int column) data, RunSettings settings)
    {
        foreach (var (pos, code) in GeneratePositions().Zip(GenerateCodes()))
        {
            if ((pos.X + 1) == data.column && (pos.Y + 1) == data.row)
            {
                Console.WriteLine($"The code at row {data.row} column {data.column} is {code}");
                break;
            }
        }
    }

    public override void Part2((int row, int column) data, RunSettings settings)
    {
        Console.WriteLine("Merry Christmas!");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day25), "day25-input.txt");
    }

    [GeneratedRegex(@"^To continue, please consult the code grid in the manual\.  Enter the code at row (?<row>\d+), column (?<column>\d+)\.$")]
    private static partial Regex TextPattern();

    private static IEnumerable<long> GenerateCodes()
    {
        long current = 20151125;
        while (true)
        {
            yield return current;
            current = (current * 252533) % 33554393;
        }
    }

    private static IEnumerable<Position> GeneratePositions()
    {
        var x = 0;
        var y = 0;
        while (true)
        {
            yield return (x, y);
            ++x;
            --y;
            if (y < 0)
            {
                y = x;
                x = 0;
            }
        }
    }
}
