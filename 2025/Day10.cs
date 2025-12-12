using AdventOfCode.Utils;
using System.Collections.Specialized;

namespace AdventOfCode._2025;
internal partial class Day10 : DayRunner<Day10.Machine[]>
{
    public struct Button
    {
        public BitVector32 Lights;
    }
    public struct Machine
    {
        public BitVector32 Lights;
        public int LightsCount;
        public Button[] Buttons;
        public int[] Joltages;
    }

    public override Machine[] Parse(FileReference file)
    {
        var machines = new List<Machine>();
        foreach (var line in file.GetLines())
        {
            BitVector32? lights = null;
            int lightsCount = 0;
            var buttons = new List<Button>();
            int[]? joltages = null;
            foreach (var part in line.AsSpan().SplitAsStrings())
            {
                if (part.StartsWith('[') && part.EndsWith(']'))
                {
                    if (lights != null)
                        throw new InvalidOperationException($"Lights diagram is already defined: {part}");
                    var bools = part.Skip(1).Take(part.Length - 2).Select(ch => ch switch
                    {
                        '.' => false,
                        '#' => true,
                        _ => throw new InvalidOperationException($"Unknown light state: {ch}")
                    }).ToArray();
                    if (bools.Length > 16)
                        throw new InvalidOperationException("This algorithm does not support more than 16 lights");
                    lights = IntsToVector(bools.Index().Where(tuple => tuple.Item).Select(tuple => tuple.Index));
                    lightsCount = bools.Length;
                }
                else if (part.StartsWith('(') && part.EndsWith(')'))
                {
                    buttons.Add(new() { Lights = IntsToVector(part.AsSpan(1..^1).SplitInts(',')) });
                }
                else if (part.StartsWith('{') && part.EndsWith('}'))
                {
                    if (joltages != null)
                        throw new InvalidOperationException($"Joltages are already defined: {part}");
                    joltages = [.. part.AsSpan(1..^1).SplitInts(',')];
                }
                else
                    throw new InvalidOperationException($"Unknown part type: {part}");
            }
            if (lights == null)
                throw new InvalidOperationException("No lights diagram defined.");
            if (buttons.Count == 0)
                throw new InvalidOperationException("No buttons defined.");
            if (joltages == null)
                throw new InvalidOperationException("No joltages defined.");
            machines.Add(new()
            {
                Lights = lights.Value,
                LightsCount = lightsCount,
                Buttons = [.. buttons],
                Joltages = joltages
            });
        }
        return [.. machines];
    }

    public override void Part1(Machine[] data, RunSettings settings)
    {
        var sum = 0;
        foreach (var (i, machine) in data.Index())
        {
            int[]? bestButtons = null;
            int bestButtonsCount = int.MaxValue;
            foreach (var (buttons, state) in GenerateButtons(machine.Buttons, new()))
            {
                if (state.Equals(machine.Lights))
                {
                    var count = buttons.Aggregate((a, b) => a + b);
                    if (count < bestButtonsCount)
                    {
                        bestButtonsCount = count;
                        bestButtons = buttons;
                    }
                }
            }
            if (settings.Verbose)
                Console.WriteLine($"Machine {i}: {(bestButtons != null ? string.Join(',', bestButtons) : "no solution found.")}");
            sum += bestButtonsCount;
        }
        Console.WriteLine($"Minimum button presses required is {sum}");
    }

    public override void Part2(Machine[] data, RunSettings settings)
    {
        var sum = 0;
        foreach (var (i, machine) in data.Index())
        {
            if (settings.Verbose)
                Console.WriteLine($"Machine {i}:");
            int[] bestButtons = [];
            int bestScore = int.MaxValue;
            var cache = new Dictionary<int[], int[][]>(ArrayComparer<int>.Instance);
            var transitions = GenerateTransitions(machine, new());
            foreach (var buttons in GetButtonsForJoltage(machine, machine.Joltages, transitions, cache))
            {
                var score = buttons.Aggregate((a, b) => a + b);
                if (score < bestScore)
                {
                    if (settings.Verbose)
                        Console.WriteLine($"{string.Join(',', buttons)} => {score} presses");
                    bestScore = score;
                    bestButtons = buttons;
                    continue;
                }
            }
            sum += bestScore;
        }
        Console.WriteLine($"Minimum button presses required is {sum}");
    }

    private static BitVector32 IntsToVector(IEnumerable<int> ints)
    {
        var vector = new BitVector32();
        foreach (var i in ints)
            vector[1 << i] = true;
        return vector;
    }

    private static BitVector32 ExtractFirstBit(IEnumerable<int> joltages)
    {
        var vector = new BitVector32();
        foreach (var (i, num) in joltages.Index())
        {
            if ((num % 2) == 1)
                vector[1 << i] = true;
        }
        return vector;
    }

    private static IEnumerable<int[]> GetButtonsForJoltage(Machine machine, int[] joltages, Dictionary<BitVector32, List<(int[], int[])>> transitions, Dictionary<int[], int[][]> cache)
    {
        if (joltages.All(j => j == 0))
        {
            yield return Enumerable.Repeat(0, machine.Buttons.Length).ToArray();
            yield break;
        }
        if (cache.TryGetValue(joltages, out var cachedResult))
        {
            foreach (var buttons in cachedResult)
                yield return buttons;
            yield break;
        }
        var target = ExtractFirstBit(joltages);
        var result = new List<int[]>();
        if (transitions.TryGetValue(target, out var solutions))
        {
            foreach (var (buttons, buttonsJoltage) in solutions)
            {
                int[] newJoltages = [.. joltages.Zip(buttonsJoltage).Select(pair => pair.First - pair.Second)];
                if (newJoltages.Any(j => j < 0))
                    continue;
                for (var j = 0; j < newJoltages.Length; ++j)
                    newJoltages[j] >>>= 1;
                foreach (var buttons2 in GetButtonsForJoltage(machine, newJoltages, transitions, cache))
                {
                    int[] buttons3 = [.. buttons.Zip(buttons2).Select(pair => pair.First + pair.Second * 2)];
                    yield return buttons3;
                    result.Add(buttons3);
                }
            }
        }
        cache[joltages] = [.. result];
    }

    private static int[] CalculateJoltages(Machine machine, int[] buttons)
    {
        var joltages = new int[machine.LightsCount];
        for (var i = 0; i < buttons.Length; ++i)
        {
            var button = machine.Buttons[i];
            for (var j = 0; j < joltages.Length; ++j)
            {
                if (button.Lights[1 << j])
                    joltages[j] += buttons[i];
            }
        }
        return joltages;
    }
    private static IEnumerable<(int[], BitVector32)> GenerateButtons(IEnumerable<Button> buttons, BitVector32 initialState)
    {
        if (!buttons.Any())
        {
            yield return ([], initialState);
            yield break;
        }
        foreach (var (subSequence, state) in GenerateButtons(buttons.Skip(1), initialState))
            yield return ([0, .. subSequence], state);
        var activeState = new BitVector32(initialState.Data ^ buttons.First().Lights.Data);
        foreach (var (subSequence, state) in GenerateButtons(buttons.Skip(1), activeState))
            yield return ([1, .. subSequence], state);
    }
    private static Dictionary<BitVector32, List<(int[] buttons, int[] joltages)>> GenerateTransitions(Machine machine, BitVector32 initialState)
    {
        var result = new Dictionary<BitVector32, List<(int[], int[])>>();
        foreach (var (buttons, state) in GenerateButtons(machine.Buttons, initialState))
        {
            if (!result.TryGetValue(state, out var list))
                result[state] = list = [];
            list.Add((buttons, CalculateJoltages(machine, buttons)));
        }
        return result;
    }
}
