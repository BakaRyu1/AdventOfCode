using AdventOfCode.Utils;

namespace AdventOfCode._2015;

internal class Day03 : DayRunner<Direction[]>
{
    public override Direction[] Parse(FileReference file)
    {
        return [.. file.GetText().Select(ch => ch switch
        {
            '^' => Direction.North,
            'v' => Direction.South,
            '>' => Direction.East,
            '<' => Direction.West,
            _ => throw new InvalidOperationException($"Invalid character '{ch}'")
        })];
    }

    public override void Part1(Direction[] data, RunSettings settings)
    {
        var map = new Array2D<bool>(1, 1);
        var pos = new Position(0, 0);
        map[0, 0] = true;
        foreach(var direction in data)
        {
            pos += direction.Delta();
            if (!pos.IsInside(map))
            {
                var shift = -pos.Min(Position.Zero);
                if (shift != Position.Zero)
                    pos += shift;
                map = map.AsResized(pos, shift);
            }
            map[pos] = true;
        }
        var count = map.Data.Count(b => b);
        Console.WriteLine($"Santa delivered to {count} houses.");
    }

    public override void Part2(Direction[] data, RunSettings settings)
    {
        var map = new Array2D<bool>(1, 1);
        var santaPos = new Position(0, 0);
        var roboPos = new Position(0, 0);
        var roboTurn = false;
        map[0, 0] = true;
        foreach (var direction in data)
        {
            ref var pos = ref (roboTurn ? ref roboPos : ref santaPos);
            pos += direction.Delta();
            if (!pos.IsInside(map))
            {
                var shift = -pos.Min(Position.Zero);
                if (shift != Position.Zero)
                {
                    ref var otherPos = ref (roboTurn ? ref santaPos : ref roboPos);
                    pos += shift;
                    otherPos += shift;
                }
                map = map.AsResized(pos, shift);
            }
            map[pos] = true;
            roboTurn = !roboTurn;
        }
        var count = map.Data.Count(b => b);
        Console.WriteLine($"Santa delivered to {count} houses.");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day03), settings.Example ? "day03-example.txt" : "day03-input.txt");
    }
}
