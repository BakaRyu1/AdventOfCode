using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day02 : DayRunner<IEnumerable<int[]>>
{
    public override IEnumerable<int[]> Parse(FileReference file)
    {
        var lines = file.GetLines();
        var levelsList = lines
            .Select(line => line.AsSpan()
                .SplitInts()
                .ToArray());
        return levelsList;
    }

    public override void Part1(IEnumerable<int[]> levelsList, RunSettings settings)
    {
        var safeCount = 0;
        foreach (var levels in levelsList)
        {
            var deltas = GetDeltas(levels);
            if (IsSafeDeltas(deltas))
                ++safeCount;
            else if (settings.Verbose)
            {
                Console.WriteLine("Rejecting: " + string.Join(' ', levels));
                Console.WriteLine("\tDeltas: " + string.Join(' ', deltas));
                if (!IsValidDeltas(deltas))
                    Console.WriteLine("\tReason: invalid values");
                else if (!IsStrictlyMonotonicDeltas(deltas))
                    Console.WriteLine("\tReason: not monotonic values");
            }
        }
        if (settings.Verbose)
            Console.WriteLine();
        Console.WriteLine("Number of safe reports: " + safeCount);
    }
    
    public override void Part2(IEnumerable<int[]> levelsList, RunSettings settings)
    {
        var safeCount = 0;
        foreach (var levels in levelsList)
        {
            var deltas = GetDeltas(levels);
            if (IsSafeDeltas(deltas))
            {
                ++safeCount;
                continue;
            }
            if (IsSafeWithProblemDampener(levels, deltas, delta => delta == 0 || Math.Abs(delta) > 3, settings))
            {
                ++safeCount;
                continue;
            }
            if (IsSafeWithProblemDampener(levels, deltas, delta => delta > 0, settings))
            {
                ++safeCount;
                continue;
            }
            if (IsSafeWithProblemDampener(levels, deltas, delta => delta < 0, settings))
            {
                ++safeCount;
                continue;
            }
            if (settings.Verbose)
            {
                Console.WriteLine("Rejecting: " + string.Join(' ', levels));
                Console.WriteLine("\tDeltas: " + string.Join(' ', deltas));
                if (!IsValidDeltas(deltas))
                    Console.WriteLine("\tReason: invalid values");
                else if (!IsStrictlyMonotonicDeltas(deltas))
                    Console.WriteLine("\tReason: not monotonic values");
            }
        }
        if (settings.Verbose)
            Console.WriteLine();
        Console.WriteLine("Number of safe reports (with problem dampener): " + safeCount);
    }

    private static int[] GetDeltas(IEnumerable<int> levels)
    {
        return levels.Skip(1).Zip(levels, (second, first) => second - first).ToArray();
    }

    private static bool IsValidDeltas(IEnumerable<int> deltas)
    {
        return deltas
            .Select(delta => Math.Abs(delta))
            .All(absDelta => 1 <= absDelta && absDelta <= 3);
    }

    private static bool IsStrictlyMonotonicDeltas(IEnumerable<int> deltas)
    {
        var sign = Math.Sign(deltas.FirstOrDefault());
        if (sign == 0)
            return false;
        return deltas.Skip(1).All(delta => Math.Sign(delta) == sign);
    }
    private static bool IsSafeDeltas(IEnumerable<int> deltas)
    {
        return IsValidDeltas(deltas) && IsStrictlyMonotonicDeltas(deltas);
    }
    private static bool IsSafeWithoutDelta(int[] levels, int deltaPosition, RunSettings settings)
    {
        if (deltaPosition < 0 || deltaPosition >= levels.Length)
        {
            Console.Error.WriteLine("Checking safety with an invalid position: " + deltaPosition + "/" + levels.Count());
            return false;
        }
        var levelsWithoutFirstValue = levels.ToList();
        levelsWithoutFirstValue.RemoveAt(deltaPosition);
        if (IsSafeDeltas(GetDeltas(levelsWithoutFirstValue)))
        {
            if (settings.Verbose)
            {
                Console.WriteLine("Salvaging: " + string.Join(' ', levels));
                Console.WriteLine("\tDeltas: " + string.Join(' ', GetDeltas(levels)));
                Console.WriteLine("\tRemoved: " + levels[deltaPosition] + " (at " + deltaPosition + ")");
            }
            return true;
        }
        var levelsWithoutSecondValue = levels.ToList();
        levelsWithoutSecondValue.RemoveAt(deltaPosition + 1);
        if (IsSafeDeltas(GetDeltas(levelsWithoutSecondValue)))
        {
            if (settings.Verbose)
            {
                Console.WriteLine("Salvaging: " + string.Join(' ', levels));
                Console.WriteLine("\tDeltas: " + string.Join(' ', GetDeltas(levels)));
                Console.WriteLine("\tRemoved: " + levels[deltaPosition + 1] + " (at " + (deltaPosition + 1) + ")");
            }
            return true;
        }
        return false;
    }

    private static bool IsSafeWithProblemDampener(int[] levels, int[] deltas, Predicate<int> problemCondition, RunSettings settings)
    {
        var invalidPosition = Array.FindIndex(deltas, problemCondition);
        if (invalidPosition >= 0 && IsSafeWithoutDelta(levels, invalidPosition, settings))
            return true;
        return false;
    }
}
