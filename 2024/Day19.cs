using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day19 : DayRunner<Day19.Data>
{
    public struct Data
    {
        public string[] Towels;
        public string[] Designs;
    }
    public override Data Parse(FileReference file)
    {
        var towels = new List<string>();
        var designs = new List<string>();
        var lines = file.GetLines();
        var inDesigns = false;
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
            {
                if (towels.Count > 0)
                    inDesigns = true;
                continue;
            }
            if (inDesigns)
            {
                designs.Add(line.Trim());
                if (!IsValidTowel(designs.Last()))
                {
                    Console.Error.WriteLine("Desgin is invalid: " + designs.Last());
                    throw new InvalidOperationException();
                }
            }
            else
            {
                if (towels.Count > 0)
                {
                    Console.Error.WriteLine("Towels are already defined: " + line);
                    throw new InvalidOperationException();
                }
                towels.AddRange(line.AsSpan().SplitAsStrings(',', true));
                if (towels.Any(towel => !IsValidTowel(towel)))
                {
                    Console.Error.WriteLine("Towel is invalid: " + towels.First(towel => !IsValidTowel(towel)));
                    throw new InvalidOperationException();
                }
                towels.Sort((a, b) =>
                {
                    if (a.Length != b.Length)
                        return -(b.Length - a.Length);
                    return string.Compare(a, b, StringComparison.InvariantCulture);
                });
            }
        }
        return new()
        {
            Designs = [.. designs],
            Towels = [.. towels]
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var count = 0;
        foreach (var design in data.Designs)
        {
            PrintTowel(design);
            Console.WriteLine();
            var possible = IsDesignPossible(design, data.Towels);
            Console.WriteLine("\t=> " + (possible ? "possible" : "impossible"));
            if (possible)
                ++count;
        }
        Console.WriteLine("Count of possible designs: " + count);
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var count = 0L;
        var cache = new Dictionary<string, long>();
        foreach (var design in data.Designs)
        {
            PrintTowel(design);
            Console.WriteLine();
            var designCount = CountDesigns(design, data.Towels, cache);
            Console.WriteLine("\t=> " + designCount + " ways");
            count += designCount;
        }
        Console.WriteLine("Count of ways: " + count);
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day19), settings.Example ? "day19-example.txt" : "day19-input.txt");
    }

    private static void PrintTowel(string towel)
    {
        Console.ForegroundColor = ConsoleColor.Black;
        foreach (var ch in towel)
        {
            Console.ForegroundColor = ch == 'b' ? ConsoleColor.White : ConsoleColor.Black;
            Console.BackgroundColor = ch switch
            {
                'w' => ConsoleColor.White,
                'u' => ConsoleColor.Blue,
                'b' => ConsoleColor.DarkGray,
                'r' => ConsoleColor.Red,
                'g' => ConsoleColor.Green,
                _ => throw new InvalidOperationException()
            };
            Console.Write(ch);
        }
        Console.ResetColor();
    }

    private static bool IsValidTowel(string towel)
    {
        return !towel.AsSpan().ContainsAnyExcept("wubrg");
    }

    private static bool IsDesignPossible(ReadOnlySpan<char> design, string[] towels)
    {
        if (design.IsEmpty)
            return true;
        foreach (var towel in towels)
        {
            if (design.StartsWith(towel) && IsDesignPossible(design.Slice(towel.Length), towels))
                return true;
        }
        return false;
    }

    private static long CountDesigns(string design, string[] towels, Dictionary<string, long> cache)
    {
        if (design.Length == 0)
            return 1;
        if (cache.TryGetValue(design, out var count))
            return count;
        count = 0;
        foreach (var towel in towels)
        {
            if (design.StartsWith(towel))
                count += CountDesigns(design.Substring(towel.Length), towels, cache);
        }
        cache[design] = count;
        return count;
    }
}
