using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2024;

internal partial class Day01 : DayRunner<(List<int>, List<int>)>
{
    public override (List<int>, List<int>) Parse(FileReference file)
    {
        var lines = file.GetLines();
        var leftList = new List<int>();
        var rightList = new List<int>();
        foreach (var line in lines)
        {
            var match = LinePattern().Match(line);
            if (!match.Success)
            {
                Console.Error.WriteLine("Parsing failed on line: " + line);
                throw new InvalidOperationException();
            }
            leftList.Add(match.Groups[1].ValueSpan.ToInt());
            rightList.Add(match.Groups[2].ValueSpan.ToInt());
        }
        return (leftList, rightList);
    }

    public override void Part1((List<int>, List<int>) data, RunSettings settings)
    {
        var (leftList, rightList) = data;
        var sortedLeftList = leftList.ToList();
        var sortedRightList = rightList.ToList();
        sortedLeftList.Sort();
        sortedRightList.Sort();
        var distances = sortedLeftList
            .Zip(sortedRightList, (firstEntry, secondEntry) => Math.Abs(firstEntry - secondEntry));
        var totalDistance = distances.Sum();
        if (settings.Verbose)
            Console.WriteLine("Distances: " + string.Join(' ', distances));
        Console.WriteLine("Total distance is: " + totalDistance);
    }

    public override void Part2((List<int>, List<int>) data, RunSettings settings)
    {
        var (leftList, rightList) = data;
        var occurences = rightList
            .CountBy(entry => entry)
            .ToDictionary();
        var scores = leftList
            .Select(entry => entry * occurences.GetValueOrDefault(entry, 0));
        if (settings.Verbose)
            Console.WriteLine("Scores: " + string.Join(' ', scores));
        var totalScore = scores.Sum();

        Console.WriteLine("Total similarity score is: " + totalScore);
    }

    [GeneratedRegex(@"^\s*(\d+)\s+(\d+)\s*$", RegexOptions.CultureInvariant)]
    public static partial Regex LinePattern();
}
