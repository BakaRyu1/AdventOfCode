using AdventOfCode.Utils;

namespace AdventOfCode._2025;
internal class Day07 : DayRunner<Day07.Data>
{
    public struct Data
    {
        public Position Start;
        public Array2D<bool> Map;
    }

    public override Data Parse(FileReference file)
    {
        var rows = new List<bool[]>();
        Position? start = null;
        foreach (var line in file.GetRectangle())
        {
            rows.Add([.. line.Select((ch, i) => {
                if (ch == 'S') {
                    if (start != null)
                        throw new InvalidOperationException("Duplicate starting position found");
                    start = new(i, rows.Count);
                }
                return ch switch {
                    'S' or
                    '.' => false,
                    '^' => true,
                    _ => throw new InvalidOperationException($"Unknown character: {ch}")
                };
            })]);
        }
        if (start == null)
            throw new InvalidOperationException("Missing starting position");
        return new()
        {
            Start = start.Value,
            Map = Array2D<bool>.From(rows)
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var queue = new Queue<Position>();
        var visited = Array2D<bool>.FromSize(data.Map, false);
        var splitted = 0;
        queue.Enqueue(data.Start);
        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();
            while (pos.IsInside(data.Map) && !visited[pos] && !data.Map[pos])
            {
                visited[pos] = true;
                pos += (0, 1);
            }
            if (!pos.IsInside(data.Map) || visited[pos])
                continue;
            visited[pos] = true;
            if (data.Map[pos])
            {
                queue.Enqueue(pos + (-1, 0));
                queue.Enqueue(pos + (1, 0));
                ++splitted;
            }
        }
        Console.WriteLine($"Laser has splitted {splitted} times.");
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var timelines = CalculateTimelines(data.Map, data.Start, []);
        Console.WriteLine($"There are {timelines} timelines.");
    }

    private static long CalculateTimelines(Array2D<bool> map, Position start, Dictionary<Position, long> cache)
    {
        if (cache.TryGetValue(start, out var cachedCount))
            return cachedCount;
        var pos = start;
        while (pos.IsInside(map) && !map[pos])
            pos += (0, 1);
        long count;
        if (pos.IsInside(map) && map[pos])
        {
            var leftCount = CalculateTimelines(map, pos + (-1, 0), cache);
            var rightCount = CalculateTimelines(map, pos + (1, 0), cache);
            count = leftCount + rightCount;
        }
        else
            count = 1;
        cache[start] = count;
        return count;
    }
}
