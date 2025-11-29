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
        var houses = new HashSet<Position>();
        var pos = new Position(0, 0);
        houses.Add(pos);
        foreach(var direction in data)
        {
            pos += direction.Delta();
            houses.Add(pos);
        }
        var count = houses.Count;
        Console.WriteLine($"Santa delivered to {count} houses.");
    }

    public override void Part2(Direction[] data, RunSettings settings)
    {
        var houses = new HashSet<Position>();
        var santaPos = new Position(0, 0);
        var roboPos = new Position(0, 0);
        var roboTurn = false;
        houses.Add(santaPos);
        foreach (var direction in data)
        {
            ref var pos = ref (roboTurn ? ref roboPos : ref santaPos);
            pos += direction.Delta();
            houses.Add(pos);
            roboTurn = !roboTurn;
        }
        var count = houses.Count;
        Console.WriteLine($"Santa delivered to {count} houses.");
    }
}
