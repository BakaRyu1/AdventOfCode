using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2023;

internal partial class Day02 : DayRunner<List<Day02.GameInfo>>
{
    public struct CubeSetInfo()
    {
        public int Red = 0;
        public int Green = 0;
        public int Blue = 0;
    }
    public struct GameInfo
    {
        public int Number;
        public List<CubeSetInfo> CubeSets;
    }

    public override List<GameInfo> Parse(FileReference file)
    {
        var games = new List<GameInfo>();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            var gameMatch = GamePattern().Match(line);
            if (!gameMatch.Success)
            {
                Console.Error.WriteLine("Couldn't parse line: " + line);
                continue;
            }
            var game = new GameInfo
            {
                Number = gameMatch.Groups["num"].ValueSpan.ToInt(),
                CubeSets = []
            };
            foreach (var cubeSetText in gameMatch.Groups["cubes"].Value.Split(";").Select(part => part.Trim()))
            {
                var cubeSet = new CubeSetInfo();
                foreach (var cubeText in cubeSetText.Split(",").Select(cube => cube.Trim()))
                {
                    var cubeMatch = CubePattern().Match(cubeText);
                    if (!cubeMatch.Success)
                    {
                        Console.Error.WriteLine("Couldn't parse cube: " + cubeText);
                        continue;
                    }
                    var count = cubeMatch.Groups["num"].ValueSpan.ToInt();
                    switch (cubeMatch.Groups["color"].ValueSpan)
                    {
                        case "red":
                            cubeSet.Red = count;
                            break;
                        case "green":
                            cubeSet.Green = count;
                            break;
                        case "blue":
                            cubeSet.Blue = count;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                game.CubeSets.Add(cubeSet);
            }
            games.Add(game);
        }
        return games;
    }

    public override void Part1(List<GameInfo> games, RunSettings settings)
    {
        var sum = 0;
        foreach (var game in games)
        {
            var possible = game.CubeSets.All(dice => dice.Red <= 12 && dice.Green <= 13 && dice.Blue <= 14);
            if (possible)
                sum += game.Number;
        }
        Console.WriteLine("Sum of IDs is " + sum);
    }

    public override void Part2(List<GameInfo> games, RunSettings settings)
    {
        var sum = 0;
        foreach (var game in games)
        {
            var red = game.CubeSets.Max(cubeSet => cubeSet.Red);
            var green = game.CubeSets.Max(cubeSet => cubeSet.Green);
            var blue = game.CubeSets.Max(cubeSet => cubeSet.Blue);
            var power = red * green * blue;
            sum += power;
        }
        Console.WriteLine("Sum of powers is " + sum);
    }

    [GeneratedRegex(@"^Game (?<num>\d+): (?<cubes>.*)$", RegexOptions.CultureInvariant)]
    private static partial Regex GamePattern();
    [GeneratedRegex(@"^(?<num>\d+) (?<color>red|blue|green)$", RegexOptions.CultureInvariant)]
    private static partial Regex CubePattern();
}
