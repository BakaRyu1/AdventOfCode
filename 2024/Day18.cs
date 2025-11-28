using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day18 : DayRunner<Position[]>
{
    public override Position[] Parse(FileReference file)
    {
        var lines = file.GetLines();
        var positions = new List<Position>();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var parts = line.AsSpan().SplitInts(',').ToArray();
            if (parts.Length != 2)
            {
                Console.Error.WriteLine("Failed to parse line: " + line);
                throw new InvalidOperationException();
            }
            positions.Add(new(parts[0], parts[1]));
        }
        return [.. positions];
    }

    public override void Part1(Position[] data, RunSettings settings)
    {
        var walls = data.Select((pos, i) => (pos, i)).ToDictionary();
        var size = settings.Example ? new Position(7, 7) : new Position(71, 71);
        var maxTiming = settings.Example ? 22 : 1024;
        var best = CountSteps(size, walls, maxTiming);
        Console.WriteLine("Minimum steps is " + best);
    }

    public override void Part2(Position[] data, RunSettings settings)
    {
        var walls = data.Select((pos, i) => (pos, i)).ToDictionary();
        var size = settings.Example ? new Position(7, 7) : new Position(71, 71);
        var maxTiming = settings.Example ? 22 : 1024;
        var best = -1;
        var low = maxTiming;
        var high = data.Length - 1;
        while (low <= high)
        {
            var mid = (low + high) >>> 1;
            if (settings.Verbose)
                Console.WriteLine("Trying " + mid + " bytes");
            var value = CountSteps(size, walls, mid);
            if (value >= 0)
            {
                low = mid + 1;
                if (settings.Verbose)
                    Console.WriteLine($"\t=> Exit is reachable, searching in range [{low}, {high}].");
            }
            else if (low != mid)
            {
                high = mid;
                if (settings.Verbose)
                    Console.WriteLine($"\t=> Exit is unreachable, searching in range [{low}, {high}].");
            }
            else
            {
                if (settings.Verbose)
                    Console.WriteLine("\tExit is unreachable, best solution found!");
                best = mid;
                break;
            }
        }
        if (best < 0)
            Console.WriteLine("Couldn't find blocking position.");
        else
        {
            var pos = data[best - 1];
            Console.WriteLine($"First blocking position is {pos.X},{pos.Y} at byte {best - 1}");
        }
    }

    private static int CountSteps(Position size, Dictionary<Position, int> walls, int maxTiming)
    {
        var exitPos = size - (1, 1);
        var bestSteps = new Array2D<int>(Enumerable.Repeat(-1, size.X * size.Y), size.X, size.Y);
        bestSteps[(0, 0)] = 0;
        var queue = new Queue<(Position, int)>();
        queue.Enqueue(((0, 0), 0));
        while (queue.Count > 0)
        {
            var (position, steps) = queue.Dequeue();
            var nextSteps = steps + 1;
            foreach (var direction in Directions.All)
            {
                var neighbor = position + direction;
                if (!neighbor.IsInside(bestSteps))
                    continue;
                var localBest = bestSteps[neighbor];
                if (localBest >= 0 && localBest <= nextSteps)
                    continue;
                if (walls.TryGetValue(neighbor, out var timing) && timing < maxTiming)
                    continue;
                bestSteps[neighbor] = nextSteps;
                queue.Enqueue((neighbor, nextSteps));
            }
        }
        var result = bestSteps[exitPos];
        return result;
    }

    private static void PrintMap(Position size, Dictionary<Position, int> walls, int maxTiming)
    {
        for (var y = 0; y < size.Y; ++y)
        {
            for (var x = 0; x < size.X; ++x)
            {
                if (walls.TryGetValue((x, y), out var timing) && timing < maxTiming)
                    Console.Write('#');
                else
                    Console.Write('.');
            }
            Console.WriteLine();
        }
    }
}
