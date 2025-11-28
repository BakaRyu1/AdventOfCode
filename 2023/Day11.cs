using AdventOfCode.Utils;

namespace AdventOfCode._2023;

internal class Day11 : DayRunner<Day11.Data>
{
    public struct Data
    {
        public Array2D<bool> Map;
        public SortedSet<int> EmptyRows;
        public SortedSet<int> EmptyColumns;
    }

    public override Data Parse(FileReference file)
    {
        var lines = file.GetLines().ToArray();
        var map = Array2D<bool>.From(lines, ch => ch == '#');
        var emptyRows = Enumerable.Range(0, map.Height)
            .Where(y => map.EnumerateRowPositions(y).All(pos => map[pos] == false));
        var emptyColumns = Enumerable.Range(0, map.Width)
            .Where(x => map.EnumerateColumnPositions(x).All(pos => map[pos] == false));
        return new()
        {
            Map = map,
            EmptyRows = new(emptyRows),
            EmptyColumns = new(emptyColumns)
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var sum = EnumerateDistances(data, 2).Sum();
        Console.WriteLine("Sum of distances is " + sum);
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var sum = EnumerateDistances(data, 1_000_000).Sum();
        Console.WriteLine("Sum of distances (older) is " + sum);
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day11), settings.Example ? "day11-example.txt" : "day11-input.txt");
    }

    private static IEnumerable<long> EnumerateDistances(Data data, long emptyDistance)
    {
        var galaxies = data.Map.FindPositions(true).ToArray();
        return galaxies
            .SelectMany((galaxy1, i) =>
            {
                return galaxies
                    .Skip(i + 1)
                    .Select(galaxy2 =>
                    {
                        var extraX = data.EmptyColumns
                            .GetViewBetween(Math.Min(galaxy1.X, galaxy2.X), Math.Max(galaxy1.X, galaxy2.X))
                            .Count * (emptyDistance - 1);
                        var extraY = data.EmptyRows
                            .GetViewBetween(Math.Min(galaxy1.Y, galaxy2.Y), Math.Max(galaxy1.Y, galaxy2.Y))
                            .Count * (emptyDistance - 1);
                        var diff = (galaxy1 - galaxy2).Abs();
                        var distance = diff.X + diff.Y + extraX + extraY;
                        return distance;
                    });
            });
    }
}
