using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day25 : DayRunner<Array2D<bool>[]>
{
    public override Array2D<bool>[] Parse(FileReference file)
    {
        var schematics = new List<Array2D<bool>>();
        var rows = new List<bool[]>();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
            {
                if (rows.Count > 0)
                {
                    schematics.Add(Array2D<bool>.From(rows));
                    rows.Clear();
                }
                continue;
            }
            rows.Add(line.Select(ch => ch switch
            {
                '#' => true,
                '.' => false,
                _ => throw new InvalidOperationException()
            }).ToArray());
        }
        if (rows.Count > 0)
        {
            schematics.Add(Array2D<bool>.From(rows));
            rows.Clear();
        }
        return [.. schematics];
    }

    public override void Part1(Array2D<bool>[] data, RunSettings settings)
    {
        var (keys, locks) = SeparateSchematics(data);
        var keyHeights = keys.Select(key => GetKeyPins(key)).ToArray();
        var lockHeights = locks.Select(lock_ => GetLockPins(lock_)).ToArray();
        var fitCount = 0;
        foreach (var lock_ in lockHeights)
        {
            foreach (var key in keyHeights)
            {
                if (lock_.Zip(key).All(pair => (pair.First + pair.Second) <= 5))
                {
                    ++fitCount;
                }
            }
        }
        Console.WriteLine("Keys and locks pairs that fits: " + fitCount);
    }

    public override void Part2(Array2D<bool>[] data, RunSettings settings)
    {
        
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day25), settings.Example ? "day25-example.txt" : "day25-input.txt");
    }

    private static int[] GetKeyPins(Array2D<bool> key)
    {
        var heights = new int[key.Width];
        for (var x = 0; x < key.Width; ++x)
        {
            for (var y = key.Height - 2; y >= 0; --y)
            {
                if (!key[x, y])
                {
                    heights[x] = (key.Height - 1) - (y + 1);
                    break;
                }
            }
        }
        return heights;
    }

    private static int[] GetLockPins(Array2D<bool> lock_)
    {
        var heights = new int[lock_.Width];
        for (var x = 0; x < lock_.Width; ++x)
        {
            for (var y = 1; y < lock_.Height; ++y)
            {
                if (!lock_[x, y])
                {
                    heights[x] = y - 1;
                    break;
                }
            }
        }
        return heights;
    }

    private static (Array2D<bool>[] keys, Array2D<bool>[] locks) SeparateSchematics(IEnumerable<Array2D<bool>> schematics)
    {
        var keys = new List<Array2D<bool>>();
        var locks = new List<Array2D<bool>>();
        foreach (var schematic in schematics)
        {
            var topFilled = schematic.EnumerateRowPositions(0).Select(pos => schematic[pos]).All(val => val);
            var topEmpty = schematic.EnumerateRowPositions(0).Select(pos => schematic[pos]).All(val => !val);
            var bottomFilled = schematic.EnumerateRowPositions(schematic.Height - 1).Select(pos => schematic[pos]).All(val => val);
            var bottomEmpty = schematic.EnumerateRowPositions(schematic.Height - 1).Select(pos => schematic[pos]).All(val => !val);
            if (topEmpty && bottomFilled)
                keys.Add(schematic);
            else if (topFilled && bottomEmpty)
                locks.Add(schematic);
            else
                throw new InvalidOperationException();
        }
        return (keys.ToArray(), locks.ToArray());
    }
}
