using AdventOfCode.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode._2025;
internal class Day03 : DayRunner<Array2D<int>>
{
    public override Array2D<int> Parse(FileReference file)
    {
        var rows = new List<int[]>();
        foreach (var line in file.GetRectangle())
        {
            rows.Add([.. line.Select(ch =>
            {
                if (ch < '0' || ch > '9')
                    throw new InvalidOperationException($"Invalid character {ch}");
                return ch - '0';
            })]);
        }
        return Array2D<int>.From(rows);
    }

    public override void Part1(Array2D<int> data, RunSettings settings)
    {
        var sum = 0L;
        for (var y = 0; y < data.Height; ++y)
        {
            var joltage = FindBestJoltage(data.AsEnumerable(0, y), 2, []);
            if (settings.Verbose)
                Console.WriteLine($"Bank {y + 1}: {joltage} jolts");
            sum += joltage;
        }
        Console.WriteLine($"Total joltage is {sum}");
    }

    public override void Part2(Array2D<int> data, RunSettings settings)
    {
        var sum = 0L;
        for (var y = 0; y < data.Height; ++y)
        {
            var joltage = FindBestJoltage(data.AsEnumerable(0, y), 12, []);
            if (settings.Verbose)
                Console.WriteLine($"Bank {y + 1}: {joltage} jolts");
            sum += joltage;
        }
        Console.WriteLine($"Total joltage is {sum}");
    }

    private static long FindBestJoltage(IEnumerable<int> source, int digits, Dictionary<(int, int), long> cache)
    {
        if (digits <= 1)
            return source.Max();
        var length = source.Count() - (digits - 1);
        if (cache.TryGetValue((length, digits), out var cachedValue))
            return cachedValue;
        var bestJoltage = -1L;
        var bestFirstDigit = -1;
        for (var i = 0; i < length; ++i)
        {
            var firstDigit = source.ElementAt(i);
            if (firstDigit < bestFirstDigit)
                continue;
            var otherDigits = FindBestJoltage(source.Skip(i+1), digits - 1, cache);
            var joltage = source.ElementAt(i) * (long)Math.Pow(10, digits - 1) + otherDigits;
            if (joltage > bestJoltage)
            {
                bestJoltage = joltage;
                bestFirstDigit = firstDigit;
            }
        }
        cache[(length, digits)] = bestJoltage;
        return bestJoltage;
    }
}
