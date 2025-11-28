using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day08 : DayRunner<Array2D<char>>
{
    public override Array2D<char> Parse(FileReference file)
    {
        var lines = file.GetLines().ToArray();
        var width = lines[0].Length;
        if (lines.Skip(1).Any(line => line.Length != width))
        {
            Console.Error.WriteLine("Invalid schematic: at least one line isn't the same length.");
            throw new InvalidOperationException();
        }
        return Array2D<char>.From(lines);
    }
    public override void Part1(Array2D<char> map, RunSettings settings)
    {
        var antinodes = Array2D<bool>.FromSize(map, false);
        var frequencies = map.Data.Distinct().Where(ch => ch != '.').ToArray();
        foreach (var freq in frequencies)
        {
            var positions = map.FindPositions(freq).ToArray();
            foreach (var (i, pos1) in positions.Index())
            {
                foreach (var pos2 in positions.Skip(i + 1))
                {
                    var delta = pos2 - pos1;
                    var antinode1 = pos1 - delta;
                    if (antinode1.IsInside(antinodes))
                        antinodes[antinode1] = true;
                    var antinode2 = pos2 + delta;
                    if (antinode2.IsInside(antinodes))
                        antinodes[antinode2] = true;
                }
            }
        }
        if (settings.Verbose)
        {
            PrintMap(map, antinodes);
            Console.WriteLine();
        }
        var count = antinodes.Data.Count(pos => pos);
        Console.WriteLine("Total antinodes is " + count);
    }
    public override void Part2(Array2D<char> map, RunSettings settings)
    {
        var antinodes = Array2D<bool>.FromSize(map, false);
        var frequencies = map.Data.Distinct().Where(ch => ch != '.').ToArray();
        foreach (var freq in frequencies)
        {
            var positions = map.FindPositions(freq).ToArray();
            foreach (var (i, pos1) in positions.Index())
            {
                foreach (var pos2 in positions.Skip(i + 1))
                {
                    antinodes[pos1] = true;
                    antinodes[pos2] = true;
                    var delta = pos2 - pos1;
                    var antinode1 = pos1 - delta;
                    while (antinode1.IsInside(antinodes))
                    {
                        antinodes[antinode1] = true;
                        antinode1 -= delta;
                    }
                    var antinode2 = pos2 + delta;
                    while (antinode2.IsInside(antinodes))
                    {
                        antinodes[antinode2] = true;
                        antinode2 += delta;
                    }
                }
            }
        }
        if (settings.Verbose)
        {
            PrintMap(map, antinodes);
            Console.WriteLine();
        }
        var count = antinodes.Data.Count(pos => pos);
        Console.WriteLine("Total antinodes (with resonant harmonics) is " + count);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day08), settings.Example ? "day08-example.txt" : "day08-input.txt");
    }

    private static void PrintMap(Array2D<char> map, Array2D<bool> antinodes)
    {
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                if (antinodes[x, y])
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else
                    Console.ResetColor();
                Console.Write(map[x, y]);
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
