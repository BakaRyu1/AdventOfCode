using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day11 : DayRunner<long[]>
{
    public override long[] Parse(FileReference file)
    {
        return file.GetText().AsSpan().Trim().SplitLongs().ToArray();
    }

    public override void Part1(long[] data, RunSettings settings)
    {
        Process(data, settings, 25);
    }

    public override void Part2(long[] data, RunSettings settings)
    {
        Process(data, settings, 75);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day11), settings.Example ? "day11-example.txt" : "day11-input.txt");
    }

    private static void Process(long[] data, RunSettings settings, int blinks)
    {
        var cache = new Dictionary<(long, int), long>();
        var count = 0L;
        if (settings.Verbose)
        {
            Console.WriteLine(string.Join(' ', data));
            var previewBlinks = Math.Min(3, blinks);
            IEnumerable<long> stones = data;
            for (int i = 0; i < previewBlinks; ++i)
            {
                stones = Blink(stones).ToArray();
                Console.WriteLine("\t=> " + string.Join(' ', stones));
            }
            if (blinks > 3)
                Console.WriteLine("\t...");
        }
        foreach (var stone in data)
        {
            var stoneCount = CountBlink(stone, blinks, cache);
            if (settings.Verbose)
            {
                Console.WriteLine("Stone " + stone + " generate " + stoneCount + " stones");
                Console.WriteLine("\t(cache size is " + cache.Count + ")");
            }
            count += stoneCount;
        }
        Console.WriteLine("The total number of stones after " + blinks + " blinks is " + count);
    }

    private static IEnumerable<long> Blink(IEnumerable<long> stones)
    {
        foreach (var stone in stones)
        {
            if (stone == 0)
            {
                yield return 1;
                continue;
            }
            var str = stone.ToString();
            if ((str.Length % 2) == 0)
            {
                var half = str.Length / 2;
                yield return str.AsSpan().Slice(0, half).ToInt();
                yield return str.AsSpan().Slice(half).ToInt();
                continue;
            }
            yield return stone * 2024;
        }
    }

    private static long CountBlink(long stone, int blinks, Dictionary<(long, int), long> cache)
    {
        if (blinks <= 0)
            return 1;
        if (cache.TryGetValue((stone, blinks), out var count))
            return count;
        if (stone == 0)
        {
            count = CountBlink(1, blinks - 1, cache);
            cache[(stone, blinks)] = count;
            return count;
        }
        var str = stone.ToString();
        if ((str.Length % 2) == 0)
        {
            var half = str.Length / 2;
            count = CountBlink(str.AsSpan().Slice(0, half).ToInt(), blinks - 1, cache);
            count += CountBlink(str.AsSpan().Slice(half).ToInt(), blinks - 1, cache);
            cache[(stone, blinks)] = count;
            return count;
        }
        count = CountBlink(stone * 2024, blinks - 1, cache);
        cache[(stone, blinks)] = count;
        return count;
    }
}
