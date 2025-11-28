using AdventOfCode.Utils;
using System.Data;

namespace AdventOfCode._2024;

internal class Day05 : DayRunner<Day05.Data>
{
    public struct Data
    {
        public Dictionary<int, HashSet<int>> OrderingRules;
        public int[][] Updates;
    }
    public override Data Parse(FileReference file)
    {
        var lines = file.GetLines();
        var data = new Data()
        {
            OrderingRules = []
        };
        var updates = new List<int[]>();
        foreach (var line in lines)
        {
            var lineSpan = line.AsSpan().Trim();
            if (lineSpan.IsEmpty)
                continue;
            if (lineSpan.Contains('|'))
            {
                var parts = lineSpan.SplitInts('|').ToArray();
                if (parts.Length > 2)
                {
                    Console.Error.WriteLine("Rule has excess data: " + line);
                }
                if (!data.OrderingRules.TryGetValue(parts[0], out var set))
                    data.OrderingRules.Add(parts[0], set = []);
                set.Add(parts[1]);
            }
            else if (lineSpan.Contains(','))
            {
                var pages = lineSpan.SplitInts(',').ToArray();
                if ((pages.Length % 2) != 1)
                {
                    Console.Error.WriteLine("Update doesn't has a middle page: " + line);
                }
                updates.Add(pages);
            }
            else
            {
                Console.Error.WriteLine("Couldn't parse page: " + line);
                throw new InvalidOperationException();
            }
        }
        data.Updates = updates.ToArray();
        return data;
    }
    
    public override void Part1(Data data, RunSettings settings)
    {
        var sum = 0;
        var ordered = 0;
        var unordered = 0;
        foreach (var pages in data.Updates)
        {
            if (IsUpdateOrdered(data.OrderingRules, pages))
            {
                ++ordered;
                sum += pages[pages.Length / 2];
            }
            else
                ++unordered;
        }
        if (settings.Verbose)
        {
            Console.WriteLine("Ordered updates: " + ordered);
            Console.WriteLine("Unordered updates: " + unordered);
        }
        Console.WriteLine("Sum of middle pages: " + sum);
    }
    public override void Part2(Data data, RunSettings settings)
    {
        var sum = 0;
        var ordered = 0;
        var unordered = 0;
        var comparer = Comparer<int>.Create((a, b) =>
        {
            if (data.OrderingRules.TryGetValue(a, out var aRules) && aRules.Contains(b))
                return 1;
            if (data.OrderingRules.TryGetValue(b, out var bRules) && bRules.Contains(a))
                return -1;
            return 0;
        });
        foreach (var unorderedPages in data.Updates.Where(update => !IsUpdateOrdered(data.OrderingRules, update)))
        {
            var pages = unorderedPages.OrderBy(page => page, comparer).ToArray();
            sum += pages[pages.Length / 2];
        }
        Console.WriteLine("Sum of corrected middle pages: " + sum);
    }

    private static bool IsUpdateOrdered(Dictionary<int, HashSet<int>> orderingRules, int[] pages)
    {
        for (int i = 1; i < pages.Length; ++i)
        {
            if (!orderingRules.TryGetValue(pages[i], out var pageRules))
                continue;
            if (pages.Take(i).Any(prevPage => pageRules.Contains(prevPage)))
                return false;
        }
        return true;
    }
}
