using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day12 : DayRunner<Array2D<char>>
{
    private class Region(char type)
    {
        public char Type = type;
        public int Area = 0;
        public int Perimeter = 0;
        public int Sides = 0;
    }
    public override Array2D<char> Parse(FileReference file)
    {
        return Array2D<char>.From(file.GetLines().ToArray());
    }

    public override void Part1(Array2D<char> data, RunSettings settings)
    {
        var regions = new List<Region>();
        var regionMap = Array2D<Region?>.FromSize(data, null);
        for (var y = 0; y < data.Height; ++y)
        {
            for (var x = 0; x < data.Width; ++x)
            {
                if (regionMap[x, y] == null)
                    regions.Add(ExploreRegion(data, (x, y), regionMap));
            }
        }
        if (settings.Verbose)
        {
            PrintRegions(regions, regionMap);
            foreach (var region in regions)
            {
                Console.WriteLine("Region of " + region.Type + " with price " + region.Area + " * " + region.Perimeter + " = " + (region.Area * region.Perimeter));
            }
        }
        var price = regions.Sum(region => region.Area * region.Perimeter);
        Console.WriteLine("Total price is " + price);
    }

    public override void Part2(Array2D<char> data, RunSettings settings)
    {
        var regions = new List<Region>();
        var regionMap = Array2D<Region?>.FromSize(data, null);
        for (var y = 0; y < data.Height; ++y)
        {
            for (var x = 0; x < data.Width; ++x)
            {
                if (regionMap[x, y] == null)
                    regions.Add(ExploreRegion(data, (x, y), regionMap));
            }
        }
        CalculateSides(data, regionMap);
        if (settings.Verbose)
        {
            foreach (var region in regions)
            {
                Console.WriteLine("Region of " + region.Type + " with price " + region.Area + " * " + region.Sides + " = " + (region.Area * region.Sides));
            }
        }
        var price = regions.Sum(region => region.Area * region.Sides);
        Console.WriteLine("Total discounted price is " + price);
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day12), settings.Example ? "day12-example.txt" : "day12-input.txt");
    }

    private static void PrintRegions(List<Region> regions, Array2D<Region?> regionMap)
    {
        var bgColors = Enum.GetValues<ConsoleColor>();
        var fgColors = bgColors.Skip(8).Concat(bgColors.Take(8));
        var colors = bgColors.Zip(fgColors, (bgColor, fgColor) => (bgColor, fgColor));
        var colorsArray = colors.Skip(1).Concat(colors.Take(1)).ToArray();
        
        var regionsColors = regions
            .Zip(Enumerable.Repeat(colorsArray, (regions.Count + 15) / 16).SelectMany(colors => colors), (region, tuple) => (region, tuple))
            .ToDictionary();
        for (var y = 0; y < regionMap.Height; ++y)
        {
            for (var x = 0; x < regionMap.Width; ++x)
            {
                var region = regionMap[x, y];
                if (region != null)
                {
                    var regionColors = regionsColors[region];
                    Console.ForegroundColor = regionColors.fgColor;
                    Console.BackgroundColor = regionColors.bgColor;
                    Console.Write(region.Type);
                }
                else
                {
                    Console.ResetColor();
                    Console.Write('.');
                }
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private static Region ExploreRegion(Array2D<char> map, Position startPosition, Array2D<Region?> regionMap)
    {
        if (regionMap[startPosition] != null)
            throw new InvalidOperationException();
        var region = new Region(map[startPosition]);
        var visited = Array2D<bool>.FromSize(map, false);
        var queue = new Queue<Position>();
        queue.Enqueue(startPosition);
        while (queue.Count > 0)
        {
            var position = queue.Dequeue();
            if (visited[position])
                continue;
            visited[position] = true;
            if (map[position] != region.Type)
                continue;
            ++region.Area;
            regionMap[position] = region;
            foreach (var direction in Directions.All)
            {
                var neighbor = position + direction;
                if (!neighbor.IsInside(map))
                {
                    ++region.Perimeter;
                    continue;
                }
                if (map[neighbor] != region.Type)
                    ++region.Perimeter;
                queue.Enqueue(neighbor);
            }
        }
        return region;
    }

    private static void CalculateSides(Array2D<char> map, Array2D<Region?> regionMap)
    {
        bool[]? prevFences = null;
        for (var y = 0; y < map.Height; ++y)
        {
            var fences = Enumerable.Repeat(true, 1)
                .Concat(Enumerable.Range(1, map.Width - 1).Select(x => map[x - 1, y] != map[x, y]))
                .Concat(Enumerable.Repeat(true, 1))
                .ToArray();
            for (var i = 0; i < fences.Length; ++i)
            {
                if (fences[i])
                {
                    var leftPos = new Position(i - 1, y);
                    var rightPos = new Position(i, y);
                    if (leftPos.IsInside(map))
                    {
                        if (prevFences == null || !prevFences[i] || map[leftPos.X, y - 1] != map[leftPos])
                            regionMap[leftPos]!.Sides += 1;
                    }
                    if (rightPos.IsInside(map))
                    {
                        if (prevFences == null || !prevFences[i] || map[rightPos.X, y - 1] != map[rightPos])
                            regionMap[rightPos]!.Sides += 1;
                    }
                }
            }
            prevFences = fences;
        }
        prevFences = null;
        for (var x = 0; x < map.Width; ++x)
        {
            var fences = Enumerable.Repeat(true, 1)
                .Concat(Enumerable.Range(1, map.Height - 1).Select(y => map[x, y - 1] != map[x, y]))
                .Concat(Enumerable.Repeat(true, 1))
                .ToArray();
            for (var i = 0; i < fences.Length; ++i)
            {
                if (fences[i])
                {
                    var topPos = new Position(x, i - 1);
                    var bottomPos = new Position(x, i);
                    if (topPos.IsInside(map))
                    {
                        if (prevFences == null || !prevFences[i] || map[x - 1, topPos.Y] != map[topPos])
                            regionMap[topPos]!.Sides += 1;
                    }
                    if (bottomPos.IsInside(map))
                    {
                        if (prevFences == null || !prevFences[i] || map[x - 1, bottomPos.Y] != map[bottomPos])
                            regionMap[bottomPos]!.Sides += 1;
                    }
                }
            }
            prevFences = fences;
        }
    }
}
