using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day05 : DayRunner<string[]>
{
    public override string[] Parse(FileReference file)
    {
        return [.. file.GetLines()];
    }


    public override void Part1(string[] data, RunSettings settings)
    {
        var count = 0;
        foreach (var input in data)
        {
            if (Has3Vowels(input) && HasDoubledLetter(input) && !HasBadStrings(input))
            {
                if (settings.Verbose)
                    Console.WriteLine($"{input} is nice");
                ++count;
            }
            else
            {
                if (settings.Verbose)
                    Console.WriteLine($"{input} is naughty");
            }
        }
        Console.WriteLine($"There are {count} nice strings.");
    }

    public override void Part2(string[] data, RunSettings settings)
    {
        var count = 0;
        foreach (var input in data)
        {
            if (HasDuplicatePair(input) && HasMiniPalindrome(input))
            {
                if (settings.Verbose)
                    Console.WriteLine($"{input} is nice");
                ++count;
            }
            else
            {
                if (settings.Verbose)
                    Console.WriteLine($"{input} is naughty");
            }
        }
        Console.WriteLine($"There are {count} nicer strings.");
    }

    private static readonly HashSet<char> VOWELS = ['a', 'e', 'i', 'o', 'u'];
    private static readonly HashSet<(char, char)> BAD_STRINGS = [('a', 'b'), ('c', 'd'), ('p', 'q'), ('x', 'y')];

    private static IEnumerable<(char First, char Second)> PairsOf(IEnumerable<char> input, int delta = 1)
        => input.Zip(input.Skip(delta));

    private static bool Has3Vowels(string input)
        => input.Count(VOWELS.Contains) >= 3;

    private static bool HasDoubledLetter(string input)
        => PairsOf(input).Any(pair => pair.First == pair.Second);

    private static bool HasBadStrings(string input)
        => PairsOf(input).Any(BAD_STRINGS.Contains);

    private static bool HasDuplicatePair(string input)
    {
        return PairsOf(input).Index().Any(tuple => {
            var (i, pair) = tuple;
            return PairsOf(input.Skip(i + 2)).Any(pair2 => pair2 == pair);
        });
    }

    private static bool HasMiniPalindrome(string input)
        => PairsOf(input, 2).Any(pair => pair.First == pair.Second);
}
