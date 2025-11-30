using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015;
internal partial class Day13 : DayRunner<Dictionary<(string, string), int>>
{
    public override Dictionary<(string, string), int> Parse(FileReference file)
    {
        var happinesses = new Dictionary<(string, string), int>();
        foreach (var line in file.GetLines())
        {
            var match = HappinessPattern().Match(line);
            if (!match.Success)
                throw new InvalidOperationException($"Couldn't parse line: {line}");
            var name = match.Groups["name"].Value;
            var neighbor = match.Groups["neighbor"].Value;
            var op = match.Groups["op"].ValueSpan.SequenceEqual("gain") ? 1 : -1;
            var value = match.Groups["value"].ValueSpan.ToInt() * op;
            happinesses[(name, neighbor)] = value;
        }
        return happinesses;
    }

    public override void Part1(Dictionary<(string, string), int> data, RunSettings settings)
    {
        var guests = data.Keys.Select(tuple => tuple.Item1).ToHashSet();
        var (bestTable, bestHappiness) = GetBestTable(guests, data);
        Console.WriteLine($"Best happiness is {bestHappiness}");
        Console.WriteLine($"Best table is {string.Join(", ", bestTable)}");
    }

    public override void Part2(Dictionary<(string, string), int> data, RunSettings settings)
    {
        var guests = data.Keys.Select(tuple => tuple.Item1).ToHashSet();
        var (bestTable, bestHappiness) = GetBestTable(["Yourself", .. guests], data);
        Console.WriteLine($"Best happiness with yourself is {bestHappiness}");
        Console.WriteLine($"Best table with yourself is {string.Join(", ", bestTable)}");
    }

    [GeneratedRegex(@"^\s*(?<name>\w+)\s+would\s+(?<op>gain|lose)\s+(?<value>\d+)\s+happiness\s+units?\s+by\s+sitting\s+next\s+to\s+(?<neighbor>\w+)\s*\.\s*$")]
    private static partial Regex HappinessPattern();

    private static (string[], int) GetBestTable(IEnumerable<string> guests, Dictionary<(string, string), int> happinesses)
    {
        var queue = new Queue<(string[], string[])>();
        queue.Enqueue(([guests.First()], [.. guests.Skip(1)]));
        string[] bestTable = [];
        int bestHappiness = -1;
        while (queue.Count > 0)
        {
            var (table, availableGuests) = queue.Dequeue();
            if (availableGuests.Length == 1)
            {
                string[] newTable = [.. table, availableGuests.First()];
                var happiness = GetTotalHappiness(newTable, happinesses);
                if (happiness > bestHappiness)
                {
                    bestHappiness = happiness;
                    bestTable = newTable;
                }
            }
            else
            {
                foreach (var name in availableGuests)
                {
                    string[] newTable = [.. table, name];
                    string[] remainingGuests = [.. availableGuests.Where(n => n != name)];
                    queue.Enqueue((newTable, remainingGuests));
                }
            }
        }
        return (bestTable, bestHappiness);
    }

    private static IEnumerable<string> GetNeighbors(string?[] table, int pos)
    {
        var left = pos - 1;
        if (left < 0)
            left = table.Length - 1;
        var right = pos + 1;
        if (right >= table.Length)
            right = 0;
        if (table[left] is string leftNeighbor)
            yield return leftNeighbor;
        if (table[right] is string rightNeighbor)
            yield return rightNeighbor;
    }

    private static int GetHappiness(string?[] table, int pos, Dictionary<(string, string), int> happinesses)
    {
        var happiness = 0;
        var name = table[pos];
        if (name == null)
            return 0;
        foreach (var neighbor in GetNeighbors(table, pos))
            happiness += happinesses.GetValueOrDefault((name, neighbor));
        return happiness;
    }

    private static int GetTotalHappiness(string?[] table, Dictionary<(string, string), int> happinesses)
    {
        var happiness = 0;
        for (var i = 0; i < table.Length; ++i)
            happiness += GetHappiness(table, i, happinesses);
        return happiness;
    }
}
