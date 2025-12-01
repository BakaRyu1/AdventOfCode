using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day18 : DayRunner<Array2D<bool>>
{
    public override Array2D<bool> Parse(FileReference file)
    {
        var lines = file.GetRectangle();
        var rows = new List<bool[]>();
        foreach (var line in lines)
        {
            rows.Add(line.Select(ch =>
            {
                return ch switch
                {
                    '#' => true,
                    '.' => false,
                    _ => throw new InvalidOperationException("Invalid character found: " + ch)
                };
            }).ToArray());
        }
        return Array2D<bool>.From(rows);
    }

    public override void Part1(Array2D<bool> data, RunSettings settings)
    {
        var state = new Array2D<bool>([..data.Data], data.Width, data.Height);
        var otherState = new Array2D<bool>(data.Width, data.Height);
        var stepsCount = settings.Example ? 4 : 100;
        for (var i = 0; i < stepsCount; ++i)
        {
            EvolveGrid(state, otherState);
            (otherState, state) = (state, otherState);
        }
        if (settings.Verbose)
            PrintGrid(state);
        var count = state.Data.Count(b => b);
        Console.WriteLine($"There are {count} lights on.");
    }

    public override void Part2(Array2D<bool> data, RunSettings settings)
    {
        var state = new Array2D<bool>([.. data.Data], data.Width, data.Height);
        var otherState = new Array2D<bool>(data.Width, data.Height);
        var stepsCount = settings.Example ? 5 : 100;
        ResetCornerStates(state);
        for (var i = 0; i < stepsCount; ++i)
        {
            EvolveGrid(state, otherState);
            (otherState, state) = (state, otherState);
            ResetCornerStates(state);
        }
        if (settings.Verbose)
            PrintGrid(state);
        var count = state.Data.Count(b => b);
        Console.WriteLine($"There are {count} lights on including stuck lights.");
    }

    private static void PrintGrid(Array2D<bool> state)
    {
        for (var y = 0; y < state.Height; ++y)
        {
            for (var x = 0; x < state.Width; ++x)
            {
                Console.Write(state[x, y] ? '#' : '.');
            }
            Console.WriteLine();
        }
    }

    private static void ResetCornerStates(Array2D<bool> state)
    {
        state[0, 0] = true;
        state[state.Width - 1, 0] = true;
        state[0, state.Height - 1] = true;
        state[state.Width - 1, state.Height - 1] = true;
    }

    private static IEnumerable<bool> GetNeighbors(Array2D<bool> state, Position pos)
    {
        for (var y = -1; y <= 1; ++y)
        {
            for (var x = -1; x <= 1; ++x)
            {
                if (x == 0 && y == 0)
                    continue;
                var newPos = pos + (x, y);
                if (newPos.IsInside(state))
                    yield return state[newPos];
                else
                    yield return false;
            }
        }
    }

    private static void EvolveGrid(Array2D<bool> state, Array2D<bool> outState)
    {
        for (var y = 0; y < state.Height; ++y)
        {
            for (var x = 0; x < state.Width; ++x)
            {
                var count = GetNeighbors(state, (x, y)).Count(b => b);
                if (state[x, y])
                {   
                    outState[x, y] = count is 2 or 3;
                }
                else
                {
                    outState[x, y] = count == 3;
                }
            }
        }
    }
}
