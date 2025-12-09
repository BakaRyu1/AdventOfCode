using AdventOfCode.Utils;

namespace AdventOfCode._2025;
internal class Day09 : DayRunner<Position[]>
{
    public override Position[] Parse(FileReference file)
    {
        var positions = new List<Position>();
        foreach (var line in file.GetLines())
        {
            var pos = line.IndexOf(',');
            if (pos < 0)
                throw new InvalidOperationException($"Invalid position: {line}");
            var x = line.AsSpan(0, pos).ToInt();
            var y = line.AsSpan(pos + 1).ToInt();
            positions.Add((x, y));
        }
        return [.. positions];
    }

    public override void Part1(Position[] data, RunSettings settings)
    {
        var largestArea = 0L;
        foreach (var (i, p1) in data.Index())
        {
            foreach (var p2 in data.Skip(i + 1))
            {
                var min = p1.Min(p2);
                var max = p1.Max(p2);
                var diff = max - min + (1, 1);
                var area = (long)diff.X * diff.Y;
                if (area > largestArea)
                {
                    if (settings.Verbose)
                        Console.WriteLine($"{p1}-{p2} => {area}");
                    largestArea = area;
                }
            }
        }
        Console.WriteLine($"Largest area is {largestArea}");
    }

    public override void Part2(Position[] data, RunSettings settings)
    {
        var width = data.Aggregate(0, (num, pos) => Math.Max(num, pos.X)) + 1;
        var height = data.Aggregate(0, (num, pos) => Math.Max(num, pos.Y)) + 1;
        if (settings.Verbose)
            Console.WriteLine("Calculating segments...");
        var segments = GetAllSegments(data).ToList();
        if (settings.Verbose)
            Console.WriteLine("Calculating vertical segments...");
        var verticalSegments = GetVerticalSegments(segments);
        if (settings.Verbose)
            Console.WriteLine("Calculating lines...");
        var lines = GetAllFilledLines(verticalSegments);
        if (settings.Verbose)
            Console.WriteLine("Calculating widths...");
        var widths = GetWidths(lines);
        var largestArea = 0L;
        foreach (var (i, p1) in data.Index())
        {
            foreach (var p2 in data.Skip(i + 1))
            {
                var pMin = p1.Min(p2);
                var pMax = p1.Max(p2);
                var diff = pMax - pMin + (1, 1);
                var area = (long)diff.X * diff.Y;
                if (area <= largestArea)
                    continue; // Only check if area is larger
                if (data.Any(p => pMin.X < p.X && p.X < pMax.X && pMin.Y < p.Y && p.Y < pMax.Y))
                    continue; // Area must not contains any point of the polygon, except on the edges
                if (!IsFilledAtY(pMin.X, pMax.X, lines, widths.Select(pair => pair.y).Where(y => y >= pMin.Y && y <= pMax.Y)))
                    continue; // Check using scanlines, at heights ordered by polygon width
                if (settings.Verbose)
                    Console.WriteLine($"{p1}-{p2} => {area}");
                largestArea = area;
            }
        }
        Console.WriteLine($"Largest valid area is {largestArea}");
    }

    private static IEnumerable<(Position p1, Position p2)> GetAllSegments(Position[] data)
    {
        var p1 = data.Last();
        foreach (var p2 in data)
        {
            yield return (p1.Min(p2), p1.Max(p2));
            p1 = p2;
        }
    }

    private static IEnumerable<(Position, Position)> GetVerticalSegments(IEnumerable<(Position p1, Position p2)> allSegments)
    {
        return [.. allSegments
            .Where(pair => pair.p1.X == pair.p2.X)
            .OrderBy(pair => pair.p1.X)];
    }

    private static List<(int y, int width)> GetWidths(Dictionary<int, (int x1, int x2)[]> lines)
    {
        return [..lines.Select(tuple =>
        {
            var minX = tuple.Value.Aggregate(int.MaxValue, (num, pair) => Math.Min(num, pair.x1));
            var maxX = tuple.Value.Aggregate(int.MinValue, (num, pair) => Math.Max(num, pair.x2));
            return (y: tuple.Key, width: maxX - minX + 1);
        }).OrderBy(tuple => tuple.width)];
    }

    private static IEnumerable<(int, int)> GetFilledLinesAtY(int y, IEnumerable<(Position, Position)> verticalSegments)
    {
        var insideTop = false;
        var insideBottom = false;
        var inside = false;
        var pStart = 0;
        foreach(var (p1, p2) in verticalSegments.Where(pair => y >= pair.Item1.Y && y <= pair.Item2.Y))
        {
            if (y != p1.Y)
                insideTop = !insideTop;
            if (y != p2.Y)
                insideBottom = !insideBottom;
            var newInside = insideTop || insideBottom;
            if (inside != newInside)
            {
                if (newInside)
                    pStart = p1.X;
                else
                    yield return (pStart, p1.X);
                inside = newInside;
            }
        }
    }

    private static Dictionary<int, (int x1, int x2)[]> GetAllFilledLines(IEnumerable<(Position, Position)> verticalSegments)
    {
        var minY = verticalSegments.Aggregate(int.MaxValue, (num, pair) => Math.Min(num, pair.Item1.Y));
        var maxY = verticalSegments.Aggregate(int.MinValue, (num, pair) => Math.Max(num, pair.Item2.Y));
        var result = new Dictionary<int, (int, int)[]>();
        for (var y = minY; y <= maxY; ++y)
            result[y] = [.. GetFilledLinesAtY(y, verticalSegments)];
        return result;
    }

    private static bool IsFilledAtY(int x1, int x2, Dictionary<int, (int, int)[]> lines, IEnumerable<int> ys)
    {
        foreach (var y in ys)
        {
            if (!lines.TryGetValue(y, out var segments))
                return false;
            var found = false;
            foreach (var (x1_2, x2_2) in segments)
            {
                if (x1_2 <= x1 && x2 <= x2_2) // Is fully inside segment
                {
                    found = true;
                    break;
                }
                if (Math.Max(x1, x1_2) <= Math.Min(x2, x2_2)) // Intersects, hence not fully inside
                    return false;
            }
            if (!found)
                return false;
        }
        return true;
    }
}
