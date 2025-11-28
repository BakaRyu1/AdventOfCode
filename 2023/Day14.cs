using AdventOfCode.Utils;

namespace AdventOfCode._2023;

internal class Day14 : DayRunner<Array2D<Day14.State>>
{
    public enum State : byte
    {
        Empty,
        Cube,
        Round
    }

    public override Array2D<State> Parse(FileReference file)
    {
        var rows = new List<State[]>();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var row = line.Select(ch =>
            {
                switch (ch)
                {
                    case '.': return State.Empty;
                    case '#': return State.Cube;
                    case 'O': return State.Round;
                    default:
                        Console.Error.WriteLine("Unknown object: " + ch);
                        throw new InvalidOperationException();
                }
            }).ToArray();
            if (rows.Count > 0 && rows[0].Length != row.Length)
            {
                Console.Error.WriteLine("Row of differing length: " + line);
                throw new InvalidOperationException();
            }
            rows.Add(row);
        }
        return Array2D<State>.From(rows);
    }

    public override void Part1(Array2D<State> data, RunSettings settings)
    {
        var map = Array2D<State>.From(data);
        TiltMap(map, Direction.North);
        if (settings.Verbose)
        {
            PrintMap(map);
            Console.WriteLine();
        }
        var load = CalculateLoad(map, Direction.North);
        Console.WriteLine("Load is " + load);
    }
    
    public override void Part2(Array2D<State> data, RunSettings settings)
    {
        var map = Array2D<State>.From(data);
        var cycleIndices = new Dictionary<Array2D<State>, int>();
        var cycle = 0;
        cycleIndices[Array2D<State>.From(map)] = 0;
        if (settings.Verbose)
            PrintMap(map);
        while (cycle < 1000000000)
        {
            TiltMap(map, Direction.North);
            TiltMap(map, Direction.West);
            TiltMap(map, Direction.South);
            TiltMap(map, Direction.East);
            ++cycle;
            if (cycleIndices.TryGetValue(map, out var cycle2))
            {
                var remainder = 1000000000 - cycle;
                var remainderWithoutLoop = remainder % (cycle - cycle2);
                if (settings.Verbose)
                {
                    Console.WriteLine("Found cycle loop from " + cycle2 + " to " + cycle);
                    Console.WriteLine("Skipping " + (remainder - remainderWithoutLoop) + " cycles");
                    Console.WriteLine("Cycles to do after loop: " + remainderWithoutLoop);
                    Console.WriteLine();
                }
                for (var i = 0; i < remainderWithoutLoop; ++i)
                {
                    TiltMap(map, Direction.North);
                    TiltMap(map, Direction.West);
                    TiltMap(map, Direction.South);
                    TiltMap(map, Direction.East);
                }
                break;
            }
            cycleIndices[Array2D<State>.From(map)] = cycle;
        }
        if (settings.Verbose)
        {
            PrintMap(map);
            Console.WriteLine();
        }
        var load = CalculateLoad(map, Direction.North);
        Console.WriteLine("Load is " + load);
    }

    private static void PrintMap(Array2D<State> map)
    {
        for (var y = 0; y < map.Height; ++y)
        {
            for (var x = 0; x < map.Width; ++x)
            {
                switch (map[x, y])
                {
                    case State.Empty:
                        Console.Write('.');
                        break;
                    case State.Round:
                        Console.Write('O');
                        break;
                    case State.Cube:
                        Console.Write('#');
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            Console.WriteLine();
        }
    }

    private static int CalculateLoad(Array2D<State> map, Direction direction)
    {
        if (!Directions.All.Contains(direction))
            throw new InvalidOperationException();
        var load = 0;
        var weight = 1;
        if (direction.IsVertical())
        {
            foreach (var y in direction.EnumerateRangeY(0, map.Height))
            {
                load += Enumerable.Range(0, map.Width).Count(x => map[x, y] == State.Round) * weight;
                ++weight;
            }
        }
        else
        {
            foreach (var x in direction.EnumerateRangeX(0, map.Width))
            {
                load += Enumerable.Range(0, map.Height).Count(y => map[x, y] == State.Round) * weight;
                ++weight;
            }
        }
        return load;
    }

    private static void TiltMap(Array2D<State> map, Direction direction)
    {
        if (!Directions.All.Contains(direction))
            throw new InvalidOperationException();
        if (direction.IsVertical())
        {
            var dy = direction.Delta().Y;
            foreach (var y in direction.Opposite().EnumerateRangeY(0, map.Height).Skip(1))
            {
                for (var x = 0; x < map.Width; ++x)
                {
                    if (map[x, y] != State.Round)
                        continue;
                    var targetY = direction.GetRangeBoundY(0, map.Height);
                    foreach (var y2 in direction.EnumerateRangeY(0, map.Height, y + dy))
                    {
                        if (map[x, y2] != State.Empty)
                        {
                            targetY = y2 - dy;
                            break;
                        }
                    }
                    if (targetY != y)
                    {
                        map[x, y] = State.Empty;
                        map[x, targetY] = State.Round;
                    }
                }
            }
        }
        else
        {
            var dx = direction.Delta().X;
            foreach (var x in direction.Opposite().EnumerateRangeX(0, map.Width).Skip(1))
            {
                for (var y = 0; y < map.Height; ++y)
                {
                    if (map[x, y] != State.Round)
                        continue;
                    var targetX = direction.GetRangeBoundX(0, map.Width);
                    foreach (var x2 in direction.EnumerateRangeX(0, map.Width, x + dx))
                    {
                        if (map[x2, y] != State.Empty)
                        {
                            targetX = x2 - dx;
                            break;
                        }
                    }
                    if (targetX != x)
                    {
                        map[x, y] = State.Empty;
                        map[targetX, y] = State.Round;
                    }
                }
            }
        }
    }


}
