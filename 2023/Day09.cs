using AdventOfCode.Utils;

namespace AdventOfCode._2023;

internal class Day09 : DayRunner<int[][]>
{
    public override int[][] Parse(FileReference file)
    {
        var lines = file.GetLines();
        return lines
            .Select(line => line.AsSpan().SplitInts().ToArray())
            .ToArray();
    }
    public override void Part1(int[][] histories, RunSettings settings)
    {
        var sum = 0;
        foreach (var history in histories)
        {
            var sequences = GetDifferenceSequences(history);
            sequences.Reverse();
            var extrapolated = 0;
            foreach (var sequence in sequences.Skip(1))
                extrapolated = sequence.Last() + extrapolated;

            sum += extrapolated;
        }
        Console.WriteLine("Sum of extrapolated next values is " + sum);
    }
    public override void Part2(int[][] histories, RunSettings settings)
    {
        var sum = 0;
        foreach (var history in histories)
        {
            var sequences = GetDifferenceSequences(history);
            sequences.Reverse();
            var extrapolated = 0;
            foreach (var sequence in sequences.Skip(1))
                extrapolated = sequence.First() - extrapolated;

            sum += extrapolated;
        }
        Console.WriteLine("Sum of extrapolated previous values is " + sum);
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day09), settings.Example ? "day09-example.txt" : "day09-input.txt");
    }

    private static List<int[]> GetDifferenceSequences(int[] initialSequence)
    {
        var sequences = new List<int[]>
        {
            initialSequence
        };
        var currentSequence = initialSequence;
        do
        {
            currentSequence = currentSequence.Zip(currentSequence.Skip(1), (a, b) => b - a).ToArray();
            sequences.Add(currentSequence);
        } while (currentSequence.Any(value => value != 0));
        return sequences;
    }
}
