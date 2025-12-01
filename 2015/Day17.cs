using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day17 : DayRunner<int[]>
{
    public override int[] Parse(FileReference file)
    {
        var sizes = new List<int>();
        foreach (var line in file.GetLines())
        {
            sizes.Add(line.AsSpan().ToInt());
        }
        return [.. sizes];
    }

    public override void Part1(int[] data, RunSettings settings)
    {
        var queue = new Queue<(int[], int[])>();
        var valid = 0;
        queue.Enqueue(([], data));
        while (queue.Count > 0)
        {
            var (selected, sizes) = queue.Dequeue();
            var remaining = selected.Aggregate(settings.Example ? 25 : 150, (num, size) => num - size);
            if (remaining == 0)
            {
                if (settings.Verbose)
                    Console.WriteLine($"- {string.Join(", ", selected)}");
                ++valid;
            }
            else if (sizes.Length > 0)
            {
                var size = sizes[0];
                var otherSizes = sizes[1..];
                if (remaining >= size)
                    queue.Enqueue(([.. selected, size], otherSizes));
                queue.Enqueue((selected, otherSizes));
            }
        }
        Console.WriteLine($"There are {valid} valid sets.");
    }

    public override void Part2(int[] data, RunSettings settings)
    {
        var queue = new Queue<(int[], int[])>();
        var valid = 0;
        var validSize = int.MaxValue;
        queue.Enqueue(([], data));
        while (queue.Count > 0)
        {
            var (selected, sizes) = queue.Dequeue();
            var remaining = selected.Aggregate(settings.Example ? 25 : 150, (num, size) => num - size);
            if (remaining == 0)
            {

                if (selected.Length < validSize)
                {
                    if (settings.Verbose)
                        Console.WriteLine($"New minimum size: {selected.Length}");
                    validSize = selected.Length;
                    valid = 1;
                }
                else if (selected.Length == validSize)
                {
                    if (settings.Verbose)
                        Console.WriteLine($"- {string.Join(", ", selected)}");
                    ++valid;
                }
            }
            else if (sizes.Length > 0)
            {
                var size = sizes[0];
                var otherSizes = sizes[1..];
                if (remaining >= size)
                    queue.Enqueue(([.. selected, size], otherSizes));
                queue.Enqueue((selected, otherSizes));
            }
        }
        Console.WriteLine($"There are {valid} minimum-sized valid sets.");
    }
}
