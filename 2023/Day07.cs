using AdventOfCode.Utils;
using System.Buffers;

namespace AdventOfCode._2023;

internal class Day07 : DayRunner<Day07.Hand[]>
{
    public struct Hand
    {
        public char[] Cards;
        public int Bid;
    }
    private enum HandPower : int
    {
        None,
        HighCard,
        OnePair,
        TwoPair,
        ThreeKind,
        FullHouse,
        FourKind,
        FiveKind
    }

    public override Hand[] Parse(FileReference file)
    {
        var hands = new List<Hand>();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            var parts = line.AsSpan().Trim().SplitAsStrings().ToArray();
            if (parts.Length != 2)
            {
                Console.Error.WriteLine("Invalid line found: " + line);
                throw new InvalidOperationException();
            }
            var cards = parts[0];
            if (cards.Length != 5)
            {
                Console.Error.WriteLine("Hand doesn't have the required number of cards: " + line);
                throw new InvalidOperationException();
            }
            if (cards.AsSpan().IndexOfAnyExcept(CardValues) >= 0)
            {
                Console.Error.WriteLine("Hand have an invalid card: " + line);
                throw new InvalidOperationException();
            }
            var bid = parts[1].AsSpan().ToInt();
            hands.Add(new Hand()
            {
                Cards = cards.ToCharArray(),
                Bid = bid
            });
        }
        return [.. hands];
    }

    public override void Part1(Hand[] hands, RunSettings settings)
    {
        var sortedHands = (Hand[])hands.Clone();
        Array.Sort(sortedHands, Comparer<Hand>.Create((a, b) =>
        {
            var aHandPower = GetHandPower(a.Cards);
            var bHandPower = GetHandPower(b.Cards);
            if (aHandPower != bHandPower)
                return Math.Sign((int)aHandPower - (int)bHandPower);
            for (int i = 0; i < 5; ++i)
            {
                var aCardPower = CardPowers[a.Cards[i]];
                var bCardPower = CardPowers[b.Cards[i]];
                if (aCardPower != bCardPower)
                    return Math.Sign(aCardPower - bCardPower);
            }
            return 0;
        }));
        var winnings = 0;
        for (var i = 0; i < sortedHands.Length; ++i)
        {
            winnings += sortedHands[i].Bid * (i + 1);
        }
        Console.WriteLine("Winnings are " + winnings);
    }

    public override void Part2(Hand[] hands, RunSettings settings)
    {
        var sortedHands = (Hand[])hands.Clone();
        Array.Sort(sortedHands, Comparer<Hand>.Create((a, b) =>
        {
            var aHandPower = GetHandPowerWithJokers(a.Cards);
            var bHandPower = GetHandPowerWithJokers(b.Cards);
            if (aHandPower != bHandPower)
                return Math.Sign((int)aHandPower - (int)bHandPower);
            for (int i = 0; i < 5; ++i)
            {
                var aCardPower = a.Cards[i] != 'J' ? CardPowers[a.Cards[i]] : 1;
                var bCardPower = b.Cards[i] != 'J' ? CardPowers[b.Cards[i]] : 1;
                if (aCardPower != bCardPower)
                    return Math.Sign(aCardPower - bCardPower);
            }
            return 0;
        }));
        var winnings = 0;
        for (var i = 0; i < sortedHands.Length; ++i)
        {
            winnings += sortedHands[i].Bid * (i + 1);
        }
        Console.WriteLine("Winnings (with jokers) are " + winnings);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day07), settings.Example ? "day07-example.txt" : "day07-input.txt");
    }

    private static readonly string Cards = "23456789TJQKA";
    private static readonly SearchValues<char> CardValues = SearchValues.Create(Cards);
    private static Dictionary<char, int> CardPowers = Cards.Index().ToDictionary(a => a.Item, a => a.Index + 2);
    private static HandPower GetHandPower(char[] cards)
    {
        var uniqueCards = cards.Distinct().ToArray();
        if (uniqueCards.Length == 5)
            return HandPower.HighCard;
        if (uniqueCards.Length == 1)
            return HandPower.FiveKind;
        if (uniqueCards.Length == 2 && uniqueCards.Any(card => cards.Count(c => c == card) == 4))
            return HandPower.FourKind;
        if (uniqueCards.Length == 2 && uniqueCards.Any(card => cards.Count(c => c == card) == 3))
            return HandPower.FullHouse;
        if (uniqueCards.Length == 3 && uniqueCards.Any(card => cards.Count(c => c == card) == 3))
            return HandPower.ThreeKind;
        var pairsCount = uniqueCards.Count(card => cards.Count(c => c == card) == 2);
        if (pairsCount == 2)
            return HandPower.TwoPair;
        if (pairsCount == 1)
            return HandPower.OnePair;
        return HandPower.None;
    }
    private static HandPower GetHandPowerWithJokers(char[] cards)
    {
        var jokers = cards.Count(card => card == 'J');
        if (jokers == 0)
            return GetHandPower(cards);
        var uniqueCards = cards.Where(card => card != 'J').Distinct().ToArray();
        if (uniqueCards.Length <= 1)
            return HandPower.FiveKind;
        if (uniqueCards.Length == 2 && uniqueCards.Any(card => (cards.Count(c => c == card) + jokers) == 4))
            return HandPower.FourKind;
        if (uniqueCards.Length == 2 && uniqueCards.Any(card => (cards.Count(c => c == card) + jokers) == 3))
            return HandPower.FullHouse;
        if (uniqueCards.Length == 3 && uniqueCards.Any(card => (cards.Count(c => c == card) + jokers) == 3))
            return HandPower.ThreeKind;
        var pairsCount = uniqueCards.Count(card => cards.Count(c => c == card) == 2);
        var potentialPairsCount = Math.Min(uniqueCards.Count(card => cards.Count(c => c == card) == 1), jokers);
        if ((pairsCount + potentialPairsCount) >= 2)
            return HandPower.TwoPair;
        if ((pairsCount + potentialPairsCount) >= 1)
            return HandPower.OnePair;
        if ((uniqueCards.Length + jokers) == 5)
            return HandPower.HighCard;
        return HandPower.None; // Unlikely
    }
}
