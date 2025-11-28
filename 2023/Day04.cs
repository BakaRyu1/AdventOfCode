using AdventOfCode.Utils;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace AdventOfCode._2023;

internal partial class Day04 : DayRunner<Day04.CardInfo[]>
{
    public struct CardInfo
    {
        public int ID;
        public ImmutableHashSet<int> WinningNumbers;
        public int[] OwnedNumbers;
    }
    public override CardInfo[] Parse(FileReference file)
    {
        var lines = file.GetLines();
        var games = new List<CardInfo>();
        foreach (var line in lines)
        {
            var match = GamePattern().Match(line);
            if (!match.Success)
            {
                Console.Error.WriteLine("Parsing failed on line: " + line);
                throw new InvalidOperationException();
            }
            var game = new CardInfo
            {
                ID = match.Groups["id"].ValueSpan.ToInt(),
                WinningNumbers = match.Groups["winning"].ValueSpan
                    .SplitInts()
                    .ToImmutableHashSet(),
                OwnedNumbers = match.Groups["owned"].ValueSpan
                    .SplitInts()
                    .ToArray()
            };
            games.Add(game);
        }
        return [.. games];
    }

    private static int IntPow(int x, uint pow)
    {
        int ret = 1;
        while (pow != 0)
        {
            if ((pow & 1) == 1)
                ret *= x;
            x *= x;
            pow >>= 1;
        }
        return ret;
    }

    public override void Part1(CardInfo[] cards, RunSettings settings)
    {
        var totalPoints = 0;
        foreach (var card in cards)
        {
            var winningNumbers = card.OwnedNumbers.Count(number => card.WinningNumbers.Contains(number));
            if (winningNumbers > 0)
            {
                var points = IntPow(2, (uint)(winningNumbers - 1));
                totalPoints += points;
            }
        }
        Console.WriteLine("Total points is " + totalPoints);
    }

    public override void Part2(CardInfo[] cards, RunSettings settings)
    {
        var copies = new int[cards.Length];
        Array.Fill(copies, 1);

        for (var i = 0; i < cards.Length; ++i)
        {
            var card = cards[i];
            var winningNumbers = card.OwnedNumbers.Count(number => card.WinningNumbers.Contains(number));
            for (var num = 1; num <= winningNumbers; ++num)
            {
                copies[i + num] += copies[i];
            }
        }
        var totalCopies = copies.Sum();
        Console.WriteLine("Total copies is " + totalCopies);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day04), settings.Example ? "day04-example.txt" : "day04-input.txt");
    }

    [GeneratedRegex(@"^\s*Card\s+(?<id>\d+)\s*:\s*(?<winning>\d+(?:\s+\d+)*)\s*\|\s*(?<owned>\d+(?:\s+\d+)*)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex GamePattern();
}
