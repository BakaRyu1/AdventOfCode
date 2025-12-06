using AdventOfCode.Utils;

namespace AdventOfCode._2025;
internal class Day04 : DayRunner<Array2D<bool>>
{
    public override Array2D<bool> Parse(FileReference file)
    {
        var rows = new List<bool[]>();
        foreach (var line in file.GetRectangle())
        {
            rows.Add([.. line.Select(ch => ch switch
            {
                '.' => false,
                '@' => true,
                _ => throw new InvalidOperationException($"Invalid character {ch}")
            })]);
        }
        return Array2D<bool>.From(rows);
    }

    public override void Part1(Array2D<bool> data, RunSettings settings)
    {
        var count = 0;
        foreach (var pos in data.EnumeratePositions())
        { 
            if (data[pos] && IsRollAccessible(data, pos))
                ++count;
        }
        if (settings.Verbose)
            PrintMap(data, true);
        Console.WriteLine($"There are {count} rolls of paper that are accessible.");
    }

    public override void Part2(Array2D<bool> data, RunSettings settings)
    {
        var map = new Array2D<bool>([.. data.Data], data.Width, data.Height);
        var otherMap = new Array2D<bool>(data.Width, data.Height);
        var count = 0;
        var iterations = 0;
        bool changed;
        do
        {
            ++iterations;
            changed = false;
            foreach (var pos in map.EnumeratePositions())
            { 
                if (map[pos] && IsRollAccessible(map, pos))
                {
                    ++count;
                    otherMap[pos] = false;
                    changed = true;
                }
                else
                    otherMap[pos] = map[pos];
            }
            (map, otherMap) = (otherMap, map);
        } while (changed);
        if (settings.Verbose)
        {
            PrintMap(map);
            Console.WriteLine($"Iterations: {iterations}");
        }
        Console.WriteLine($"There are {count} rolls of paper that can be removed.");
    }

    private static void PrintMap(Array2D<bool> map, bool showAccessible = false)
    {
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                if (showAccessible && map[x, y])
                    Console.Write(IsRollAccessible(map, (x, y)) ? 'x' : '@');
                else
                    Console.Write(map[x, y] ? '@' : '.');
            }
            Console.WriteLine();
        }
    }

    private static bool IsRollAccessible(Array2D<bool> map, Position pos)
    {
        var count = 0;
        for (var y = -1; y <= 1; ++y)
        {
            for (var x = -1; x <= 1; ++x)
            {
                if (x == 0 && y == 0)
                    continue;
                var neighbor = pos + (x, y);
                if (neighbor.IsInside(map) && map[neighbor])
                {
                    ++count;
                    if (count >= 4)
                        return false;
                }
            }
        }
        return true;
    }
}
