using AdventOfCode.Utils;

namespace AdventOfCode._2023;

internal class Day06 : DayRunner<Day06.Race[], Day06.Race>
{
    public struct Race
    {
        public long Time;
        public long Distance;
    }

    public override Race[] Parse(FileReference file)
    {
        var races = new List<Race>();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            var parts = line.Split(':');
            if (parts.Length != 2)
            {
                Console.Error.WriteLine("Found unexpected data: " + line);
                throw new InvalidOperationException();
            }
            var numbers = parts[1].AsSpan().Trim().SplitLongs().ToArray();
            if (races.Count > 0 && races.Count != numbers.Length)
            {
                Console.Error.WriteLine("Length doesn't match with previous: " + line);
                throw new InvalidOperationException();
            }
            if (races.Count == 0)
                races.AddRange(numbers.Select(_ => new Race()));
            switch (parts[0])
            {
                case "Time":
                    for (var i = 0; i < numbers.Length; ++i)
                    {
                        var race = races[i];
                        race.Time = numbers[i];
                        races[i] = race;
                    }
                    break;
                case "Distance":
                    for (var i = 0; i < numbers.Length; ++i)
                    {
                        var race = races[i];
                        race.Distance = numbers[i];
                        races[i] = race;
                    }
                    break;
                default:
                    Console.Error.WriteLine("Unknown field name: " + line);
                    throw new InvalidOperationException();
            }
        }
        return [.. races];
    }
    public override Race Parse2(FileReference file)
    {
        var race = new Race();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            var parts = line.Split(':');
            if (parts.Length != 2)
            {
                Console.Error.WriteLine("Found unexpected data: " + line);
                throw new InvalidOperationException();
            }
            var number = parts[1].Trim().Replace(" ", "").AsSpan().ToLong();
            switch (parts[0])
            {
                case "Time":
                    race.Time = number;
                    break;
                case "Distance":
                    race.Distance = number;
                    break;
                default:
                    Console.Error.WriteLine("Unknown field name: " + line);
                    throw new InvalidOperationException();
            }
        }
        return race;
    }
    
    public override void Part1(Race[] races, RunSettings settings)
    {
        var product = 1;
        foreach (var race in races)
        {
            var winningWays = 0;
            for (var holdingTime = 1; holdingTime < race.Time; ++holdingTime)
            {
                var distance = GetDistance(race, holdingTime);
                if (distance > race.Distance)
                    ++winningWays;
            }
            product *= winningWays;
        }
        Console.WriteLine("The product of winning ways is " + product);
    }
    public override void Part2(Race race, RunSettings settings)
    {
        // (time - hold) * hold > distance
        // time * hold - hold * hold > distance
        // -distance + time * hold - hold * hold > 0
        // a = -1, b = time, c = -distance
        var discriminant = ((double)race.Time * race.Time) - (4 * (-1) * (-race.Distance));
        if (discriminant > 0)
        {
            var discriminantRoot = Math.Sqrt(discriminant);
            var hold1 = (-race.Time - discriminantRoot) / (2 * (-1));
            var hold2 = (-race.Time + discriminantRoot) / (2 * (-1));
            var lowerBound = (long)Math.Ceiling(Math.Min(hold1, hold2));
            var upperBound = (long)Math.Floor(Math.Max(hold1, hold2));
            Console.WriteLine("There are " + (upperBound - lowerBound + 1) + " ways to win the true race.");
            Console.WriteLine("Hold between " + lowerBound + "s and " + upperBound + "s.");
        }
        else if (discriminant == 0)
        {
            var hold = (-(double)race.Time) / (2 * (-1));
            Console.WriteLine("There is one way to win the true race.");
            Console.WriteLine("Hold " + hold + "s.");
        }
        else
            Console.Error.WriteLine("No way to solve race!");
    }
    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day06), settings.Example ? "day06-example.txt" : "day06-input.txt");
    }

    private static long GetDistance(Race race, long holdingTime)
    {
        var remainingTime = race.Time - holdingTime;
        return remainingTime * holdingTime;
    }
}
