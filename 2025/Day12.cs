using AdventOfCode.Utils;

namespace AdventOfCode._2025;
internal class Day12 : DayRunner<Day12.Data>
{
    public struct Region
    {
        public int Width;
        public int Height;
        public int[] Presents;
    }
    public struct Data
    {
        public Array2D<bool>[] PresentShapes;
        public Region[] Regions;
    }

    public override Data Parse(FileReference file)
    {
        var shapes = new List<Array2D<bool>>();
        var rows = new List<bool[]>();
        var insidePresent = false;
        var regions = new List<Region>();
        foreach (var line in file.GetLines())
        {
            if (insidePresent)
            {
                if (line.Length == 0)
                {
                    shapes.Add(Array2D<bool>.From(rows));
                    rows.Clear();
                    insidePresent = false;
                    continue;
                }
                rows.Add([..line.Select(ch => ch switch
                {
                    '.' => false,
                    '#' => true,
                    _ => throw new InvalidOperationException($"Invalid character inside present shape: {ch}")
                })]);
            }
            else
            {
                if (line.Length == 0)
                    continue;
                var pos = line.IndexOf(':');
                if (pos < 0)
                    throw new InvalidOperationException($"Unknown format: {line}");
                if (line.AsSpan(0, pos).Contains('x'))
                {
                    var pos2 = line.AsSpan(0, pos).IndexOf('x');
                    regions.Add(new Region()
                    {
                        Width = line.AsSpan(0, pos2).ToInt(),
                        Height = line.AsSpan((pos2 + 1)..pos).ToInt(),
                        Presents = [.. line.AsSpan(pos + 1).SplitInts()]
                    });
                }
                else
                {
                    var presentIndex = line.AsSpan(0, pos).ToInt();
                    if (presentIndex != shapes.Count)
                        throw new InvalidOperationException($"Unexpected present index: {presentIndex}");
                    insidePresent = true;
                }
            }
        }
        return new()
        {
            PresentShapes = [.. shapes],
            Regions = [.. regions]
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var counts = data.PresentShapes.Select(shape => shape.Data.Count(b => b)).ToArray();
        var shapes = data.PresentShapes.Select(shape => GetAllShapes(shape)).ToArray();
        var count = 0;
        foreach (var (i, region) in data.Regions.Index())
        {
            var regionSize = ((long)region.Width * region.Height);
            if (CalculateMinOccupancy(region.Presents, counts) > regionSize)
            {
                if (settings.Verbose)
                    Console.WriteLine($"Skipping region {i} because there's not enough space.");
                continue;
            }
            if (CalculateMaxOccupancy(region.Presents, data.PresentShapes) <= regionSize)
            {
                ++count;
                if (settings.Verbose)
                    Console.WriteLine($"Skipping region {i} because enough space with leniency.");
                continue;
            }
            if (CanFit(new(region.Width, region.Height), region.Presents, shapes, []))
            {
                if (settings.Verbose)
                    Console.WriteLine($"Region {i} can fit presents.");
                ++count;
            }
        }
        Console.WriteLine($"There are {count} valid regions.");
    }

    public override void Part2(Data data, RunSettings settings)
    {
        Console.WriteLine("Merry Christmas!");
    }

    private static long CalculateMinOccupancy(int[] presents, int[] counts)
        => presents.Zip(counts)
            .Select(pair => (long)pair.First * pair.Second)
            .Aggregate((a, b) => a + b);
    private static long CalculateMaxOccupancy(int[] presents, Array2D<bool>[] shapes)
        => presents.Zip(shapes.Select(shape => shape.Width * shape.Height))
            .Select(pair => (long)pair.First * pair.Second)
            .Aggregate((a, b) => a + b);

    private static bool CanFit(Array2D<bool> map, int[] presents, HashSet<Array2D<bool>>[] shapes, HashSet<(Array2D<bool>, int[])> visited)
    {
        if (!visited.Add((map, presents)))
            return false;
        if (presents.All(p => p == 0))
            return true;
        var presentIndex = Array.FindIndex(presents, i => i > 0);
        int[] newPresents = [.. presents];
        --newPresents[presentIndex];
        foreach (var shape in shapes[presentIndex])
        {
            foreach (var pos in GetAvailableSpaces(map, shape.Width, shape.Height))
            {
                if (CanPlace(map, shape, pos.X, pos.Y))
                {
                    var newMap = Array2D<bool>.From(map);
                    Place(newMap, shape, pos.X, pos.Y);
                    if (CanFit(newMap, newPresents, shapes, visited))
                        return true;
                }
            }
        }
        return false;
    }

    private static HashSet<Array2D<bool>> GetAllShapes(Array2D<bool> present)
    {
        var set = new HashSet<Array2D<bool>>() { present };
        var rotated = present;
        for (var i = 0; i < 4; ++i)
        {
            rotated = rotated.AsClockwiseRotated();
            set.Add(rotated);
        }
        var flipped = present.AsFlippedX();
        set.Add(flipped);
        for (var i = 0; i < 4; ++i)
        {
            flipped = flipped.AsClockwiseRotated();
            set.Add(flipped);
        }
        return set;
    }

    private static bool CanPlace(Array2D<bool> region, Array2D<bool> present, int x, int y)
    {
        for (var cy = 0; cy < present.Height; ++cy)
        {
            for (var cx = 0; cx < present.Width; ++cx)
            {
                if (present[cx, cy])
                {
                    if (region[x + cx, y + cy])
                        return false;
                }
            }
        }
        return true;
    }

    private static void Place(Array2D<bool> map, Array2D<bool> present, int x, int y)
    {
        for (var cy = 0; cy < present.Height; ++cy)
        {
            for (var cx = 0; cx < present.Width; ++cx)
            {
                if (present[cx, cy])
                    map[x + cx, y + cy] = true;
            }
        }
    }

    private static bool IsFilled(Array2D<bool> map, int x, int y, int w, int h)
    {
        for (var cy = 0; cy < h; ++cy)
        {
            for (var cx = 0; cx < w; ++cx)
            {
                if (!map[x + cx, y + cy])
                    return false;
            }
        }
        return true;
    }

    private static IEnumerable<Position> GetAvailableSpaces(Array2D<bool> map, int presentW, int presentH)
    {
        var maxX = map.Width - presentW;
        var maxY = map.Height - presentH;
        for (var y = 0; y <= maxY; ++y)
        {
            for (var x = 0; x <= maxX; ++x)
            {
                if (IsFilled(map, x, y, presentW, presentH))
                    continue;
                yield return new(x, y);
            }
        }
    }
}
