using AdventOfCode.Utils;

namespace AdventOfCode._2025;
internal class Day05 : DayRunner<Day05.Data>
{
    public struct Data
    {
        public (long start, long end)[] FreshRanges;
        public long[] Available;
    }

    public override Data Parse(FileReference file)
    {
        var inRanges = true;
        var fresh = new List<(long, long)>();
        var available = new List<long>();
        foreach (var line in file.GetLines())
        {
            if (inRanges)
            {
                if (line.Length == 0)
                {
                    inRanges = false;
                    continue;
                }
                var pos = line.IndexOf('-');
                var start = line.AsSpan(0, pos).ToLong();
                var end = line.AsSpan(pos + 1).ToLong();
                fresh.Add((start, end));
            }
            else
            {
                available.Add(line.AsSpan().ToLong());
            }
        }
        return new()
        {
            FreshRanges = [.. fresh],
            Available = [.. available]
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var count = 0;
        foreach (var id in data.Available)
        {
            if (IsFresh(id, data))
            {
                if (settings.Verbose)
                    Console.WriteLine($"Ingredient #{id} is fresh");
                ++count;
            }
        }
        Console.WriteLine($"There are {count} fresh ingredients.");
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var queue = new Queue<(long, long, int)>();
        var count = 0L;
        foreach (var (i, (start, end)) in data.FreshRanges.Index())
            queue.Enqueue((start, end, i + 1));
        while (queue.Count > 0)
        {
            var (start, end, searchStart) = queue.Dequeue();
            var intersected = false;
            for (var i = searchStart; i < data.FreshRanges.Length; ++i)
            {
                var (otherStart, otherEnd) = data.FreshRanges[i];
                if (RangeIntersects(start, end, otherStart, otherEnd))
                {
                    if (settings.Verbose)
                        Console.WriteLine($"Range {start}-{end} intersected with {otherStart}-{otherEnd}");
                    intersected = true;
                    if (start < otherStart)
                        queue.Enqueue((start, otherStart - 1, i + 1));
                    if (end > otherEnd)
                        queue.Enqueue((otherEnd + 1, end, i + 1));
                    break;
                }
            }
            if (!intersected)
            {
                if (settings.Verbose)
                    Console.WriteLine($"Found unique range {start}-{end}");
                count += (end - start + 1);
            }
        }
        Console.WriteLine($"There are {count} ingredients considered fresh.");
    }

    private static bool IsFresh(long id, Data data)
    {
        foreach (var (start, end) in data.FreshRanges)
        {
            if (start <= id && id <= end)
                return true;
        }
        return false;
    }

    private static bool RangeIntersects(long startA, long endA, long startB, long endB)
        => Math.Max(startA, startB) <= Math.Min(endA, endB);
}
