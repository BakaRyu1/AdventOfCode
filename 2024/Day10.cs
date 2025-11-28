using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day10 : DayRunner<Array2D<int>>
{
    public override Array2D<int> Parse(FileReference file)
    {
        var lines = file.GetLines().ToArray();
        return Array2D<int>.From(lines, ch =>
        {
            if (ch < '0' || ch > '9')
            {
                Console.Error.WriteLine("Invalid topographic character: " + ch);
                throw new InvalidOperationException();
            }
            return ch - '0';
        });
    }

    public override void Part1(Array2D<int> data, RunSettings settings)
    {
        var sum = 0;
        var id = 1;
        foreach (var trailhead in data.FindPositions(0))
        {
            var visited = Array2D<bool>.FromSize(data, false);
            visited[trailhead] = true;
            var score = 0;
            ExploreTrail(data, trailhead, (position, height) =>
            {
                if (visited[position])
                    return false;
                visited[position] = true;
                if (height >= 9)
                    ++score;
                return true;
            });
            if (settings.Verbose)
                Console.WriteLine("Trailhead #" + (id++) + " has score " + score);
            sum += score;
        }
        Console.WriteLine("Sum of scores is " + sum);
    }

    public override void Part2(Array2D<int> data, RunSettings settings)
    {
        var sum = 0;
        var id = 1;
        foreach (var trailhead in data.FindPositions(0))
        {
            var rating = 0;
            ExploreTrail(data, trailhead, (position, height) =>
            {
                if (height >= 9)
                    ++rating;
                return true;
            });
            if (settings.Verbose)
                Console.WriteLine("Trailhead #" + (id++) + " has rating " + rating);
            sum += rating;
        }
        Console.WriteLine("Sum of ratings is " + sum);
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day01), settings.Example ? "day10-example.txt" : "day10-input.txt");
    }

    private static void ExploreTrail(Array2D<int> map, Position trailhead, Func<Position, int, bool> func)
    {
        var queue = new Queue<Position>();
        queue.Enqueue(trailhead);
        while (queue.Count > 0)
        {
            var position = queue.Dequeue();
            var height = map[position];
            foreach (var direction in Directions.All)
            {
                var neighbor = position + direction;
                if (!neighbor.IsInside(map))
                    continue;
                var neighborHeight = map[neighbor];
                if ((height + 1) != neighborHeight)
                    continue;
                if (!func(neighbor, neighborHeight))
                    continue;
                if (neighborHeight < 9)
                    queue.Enqueue(neighbor);
            }
        }
    }
}
