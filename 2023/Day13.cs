using AdventOfCode.Utils;

namespace AdventOfCode._2023;

internal class Day13 : DayRunner<Array2D<bool>[]>
{
    public override Array2D<bool>[] Parse(FileReference file)
    {
        var patterns = new List<Array2D<bool>>();
        var rows = new List<bool[]>();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
            {
                if (rows.Count > 0)
                {
                    patterns.Add(Array2D<bool>.From(rows));
                    rows.Clear();
                }
                continue;
            }
            var row = line.Select(ch =>
            {
                switch (ch)
                {
                    case '#': return true;
                    case '.': return false;
                    default:
                        Console.Error.WriteLine("Unknown object: " + ch);
                        throw new InvalidOperationException();
                }
            }).ToArray();
            if (rows.Count > 0 && rows[0].Length != row.Length)
            {
                Console.Error.WriteLine("Row of differing length: " + line);
                throw new InvalidOperationException();
            }
            rows.Add(row);
        }
        if (rows.Count > 0)
        {
            patterns.Add(Array2D<bool>.From(rows));
            rows.Clear();
        }
        return patterns.ToArray();
    }

    public override void Part1(Array2D<bool>[] data, RunSettings settings)
    {
        var sum = 0;
        foreach (var pattern in data)
        {
            var patternSum = 0;
            var mirrorX = FindVerticalMirror(pattern);
            if (mirrorX >= 0)
                patternSum += mirrorX;
            var mirrorY = FindHorizontalMirror(pattern);
            if (mirrorY >= 0)
                patternSum += 100 * mirrorY;
            if (settings.Verbose)
            {
                //PrintMirror(pattern, mirrorX, mirrorY);
            }
            sum += patternSum;
        }
        Console.WriteLine("Sum of summaries is " + sum);
    }

    public override void Part2(Array2D<bool>[] data, RunSettings settings)
    {
        var sum = 0;
        foreach (var pattern in data)
        {
            var patternSum = 0;
            var (mirrorX, smudgeX) = FindVerticalMirrorWithSmudge(pattern);
            if (mirrorX >= 0)
                patternSum += mirrorX;
            var (mirrorY, smudgeY) = FindHorizontalMirrorWithSmudge(pattern);
            if (mirrorY >= 0)
                patternSum += 100 * mirrorY;
            if (settings.Verbose)
            {
                PrintMirror(pattern, mirrorX, mirrorY, smudgeX, smudgeY);
            }
            sum += patternSum;
        }
        Console.WriteLine("Sum of summaries (with smudge) is " + sum);
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day13), settings.Example ? "day13-example.txt" : "day13-input.txt");
    }

    private static void PrintMirror(Array2D<bool> pattern, int mirrorX, int mirrorY, Position? smudgeX = null, Position? smudgeY = null)
    {
        if (mirrorX >= 0)
            Console.WriteLine(string.Join("", Enumerable.Repeat(" ", mirrorX)) + "><");
        else
            Console.WriteLine();
        for (var y = 0; y < pattern.Height; ++y)
        {
            if (y == (mirrorY - 1))
                Console.Write("v");
            else if (y == mirrorY)
                Console.Write("^");
            else
                Console.Write(" ");
            for (var x = 0; x < pattern.Width; ++x)
            {
                if (smudgeX != null && smudgeX.Value.X == x && smudgeX.Value.Y == y)
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                else if (smudgeY != null && smudgeY.Value.X == x && smudgeY.Value.Y == y)
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                else
                    Console.ForegroundColor = ConsoleColor.Black;
                if ((mirrorX >= 0 && x < mirrorX) || (mirrorY >= 0 && y < mirrorY))
                    Console.BackgroundColor = ConsoleColor.Yellow;
                else
                    Console.BackgroundColor = ConsoleColor.Green;
                Console.Write(pattern[x, y] ? "#" : ".");
            }
            Console.ResetColor();
            if (y == (mirrorY - 1))
                Console.Write("v");
            else if (y == mirrorY)
                Console.Write("^");
            else
                Console.Write(" ");
            Console.WriteLine();
        }
        if (mirrorX >= 0)
            Console.WriteLine(string.Join("", Enumerable.Repeat(" ", mirrorX)) + "><");
        else
            Console.WriteLine();
        if (mirrorX < 0 && mirrorY < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("This mirror has no reflection line!");
            Console.ResetColor();
        }
    }

    private static int FindVerticalMirror(Array2D<bool> pattern)
    {
        var maxX = pattern.Width - 1;
        for (var x = 1; x <= maxX; ++x)
        {
            var maxDistance = Math.Min(x, pattern.Width - x);
            var isMirror = true;
            for (var dist = 1; dist <= maxDistance; ++dist)
            {
                for (var y = 0; y < pattern.Height; ++y)
                {
                    if (pattern[x - dist, y] != pattern[x + dist - 1, y])
                    {
                        isMirror = false;
                        break;
                    }
                }
                if (!isMirror)
                    break;
            }
            if (isMirror)
                return x;
        }
        return -1;
    }

    private static int FindHorizontalMirror(Array2D<bool> pattern)
    {
        var maxY = pattern.Height - 1;
        for (var y = 1; y <= maxY; ++y)
        {
            var maxDistance = Math.Min(y, pattern.Height - y);
            var isMirror = true;
            for (var dist = 1; dist <= maxDistance; ++dist)
            {
                for (var x = 0; x < pattern.Width; ++x)
                {
                    if (pattern[x, y - dist] != pattern[x, y + dist - 1])
                    {
                        isMirror = false;
                        break;
                    }
                }
                if (!isMirror)
                    break;
            }
            if (isMirror)
                return y;
        }
        return -1;
    }

    private static (int, Position) FindVerticalMirrorWithSmudge(Array2D<bool> pattern)
    {
        var maxX = pattern.Width - 1;
        for (var x = 1; x <= maxX; ++x)
        {
            var maxDistance = Math.Min(x, pattern.Width - x);
            var isMirror = true;
            var hasSmudge = false;
            var smudgePos = new Position(-1, -1);
            for (var dist = 1; dist <= maxDistance; ++dist)
            {
                for (var y = 0; y < pattern.Height; ++y)
                {
                    if (pattern[x - dist, y] != pattern[x + dist - 1, y])
                    {
                        if (!hasSmudge)
                        {
                            hasSmudge = true;
                            smudgePos = (x - dist, y);
                        }
                        else
                        {
                            isMirror = false;
                            break;
                        }
                    }
                }
                if (!isMirror)
                    break;
            }
            if (isMirror && hasSmudge)
                return (x, smudgePos);
        }
        return (-1, (-1, -1));
    }

    private static (int, Position) FindHorizontalMirrorWithSmudge(Array2D<bool> pattern)
    {
        var maxY = pattern.Height - 1;
        for (var y = 1; y <= maxY; ++y)
        {
            var maxDistance = Math.Min(y, pattern.Height - y);
            var isMirror = true;
            var hasSmudge = false;
            var smudgePos = new Position(-1, -1);
            for (var dist = 1; dist <= maxDistance; ++dist)
            {
                for (var x = 0; x < pattern.Width; ++x)
                {
                    if (pattern[x, y - dist] != pattern[x, y + dist - 1])
                    {
                        if (!hasSmudge)
                        {
                            hasSmudge = true;
                            smudgePos = (x, y - dist);
                        }
                        else
                        {
                            isMirror = false;
                            break;
                        }
                    }
                }
                if (!isMirror)
                    break;
            }
            if (isMirror && hasSmudge)
                return (y, smudgePos);
        }
        return (-1, (-1, -1));
    }
}
