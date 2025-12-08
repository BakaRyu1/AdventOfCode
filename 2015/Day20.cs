using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day20 : DayRunner<int>
{
    public override int Parse(FileReference file)
    {
        return file.GetText().AsSpan().ToInt();
    }

    public override void Part1(int data, RunSettings settings)
    {
        var i = 1;
        while (!HouseHasAtLeast(i, data))
            ++i;
        Console.WriteLine($"Found house {i}");
    }

    public override void Part2(int data, RunSettings settings)
    {
        var i = 1;
        while (!HouseHasAtLeast_Part2(i, data))
            ++i;
        Console.WriteLine($"Found house {i}");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day20), "day20-input.txt");
    }

    private static bool HouseHasAtLeast(int house, int min)
    {
        var count = 0;
        foreach (var divisor in MathUtils.GetDivisors(house))
        {
            count += 10 * divisor;
            if (count >= min)
                return true;
        }
        return false;
    }

    private static bool HouseHasAtLeast_Part2(int house, int min)
    {
        var count = 0;
        foreach (var divisor in MathUtils.GetDivisors(house))
        {
            if (house > (divisor * 50))
                continue;
            count += 11 * divisor;
            if (count >= min)
                return true;
        }
        return false;
    }
}
