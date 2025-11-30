using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015;
internal partial class Day14 : DayRunner<Day14.Reindeer[]>
{
    public struct Reindeer
    {
        public string Name;
        public int Speed;
        public int RunTime;
        public int RestTime;
    }

    public override Reindeer[] Parse(FileReference file)
    {
        var data = new List<Reindeer>();
        foreach (var line in file.GetLines())
        {
            var match = ReindeerPattern().Match(line);
            if (!match.Success)
                throw new InvalidOperationException($"Failed to parse line: {line}");
            data.Add(new()
            {
                Name = match.Groups["name"].Value,
                Speed = match.Groups["speed"].ValueSpan.ToInt(),
                RunTime = match.Groups["runtime"].ValueSpan.ToInt(),
                RestTime = match.Groups["resttime"].ValueSpan.ToInt()
            });
        }
        return [.. data];
    }

    public override void Part1(Reindeer[] data, RunSettings settings)
    {
        var bestDistance = 0;
        var bestName = "";
        foreach (var reindeer in data)
        {
            var distance = 0;
            var isResting = false;
            var remainingTime = 2503;
            while (remainingTime > 0)
            {
                if (isResting)
                    remainingTime -= Math.Min(remainingTime, reindeer.RestTime);
                else
                {
                    var time = Math.Min(remainingTime, reindeer.RunTime);
                    distance += reindeer.Speed * time;
                    remainingTime -= time;
                }
                isResting = !isResting;
            }
            if (distance > bestDistance)
            {
                bestDistance = distance;
                bestName = reindeer.Name;
            }
        }
        Console.WriteLine($"Best distance is {bestDistance} by {bestName}");
    }

    private class ReindeerState
    {
        public int Points;
        public int Distance;
        public int RemainingTime;
        public bool IsResting;
    }

    public override void Part2(Reindeer[] data, RunSettings settings)
    {
        var reindeers = new ReindeerState[data.Length];
        for (var i = 0; i < data.Length; ++i)
        {
            reindeers[i] = new() { RemainingTime = data[i].RunTime };
        }
        for (var i = 1; i <= 2503; ++i)
        {
            var bestDistance = -1;
            ReindeerState bestState = null!;
            foreach (var (state, reindeer) in reindeers.Zip(data))
            {
                if (!state.IsResting)
                    state.Distance += reindeer.Speed;
                if (state.Distance > bestDistance)
                {
                    bestDistance = state.Distance;
                    bestState = state;
                }
                --state.RemainingTime;
                if (state.RemainingTime <= 0)
                {
                    state.IsResting = !state.IsResting;
                    state.RemainingTime = state.IsResting ? reindeer.RestTime : reindeer.RunTime;
                }
            }
            ++bestState.Points;
        }
        var (winState, winReindeer) = reindeers.Zip(data).MaxBy(r => r.First.Points);
        Console.WriteLine($"Best points is {winState.Points} by {winReindeer.Name}");
    }

    [GeneratedRegex(@"^\s*(?<name>\w+)\s+can\s+fly\s+(?<speed>\d+)\s+km\/s\s+for\s+(?<runtime>\d+)\s+seconds?\s*,\s*but\s+then\s+must\s+rest\s+for\s+(?<resttime>\d+)\s+seconds?\s*.\s*$")]
    private static partial Regex ReindeerPattern();
}
