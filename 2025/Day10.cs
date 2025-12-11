using AdventOfCode.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /*
        var sum = 0;
        foreach (var (i, machine) in data.Index())
        {
            int[] bestSequence = [];
            int bestSequenceLen = int.MaxValue;
            var queue = new Queue<(int[], int[])>();
            queue.Enqueue(([..Enumerable.Repeat(0, machine.Buttons.Length)], [.. Enumerable.Repeat(0, machine.LightsCount)]));
            //var visited = new Dictionary<BitVector32, int>();
            
            while (queue.Count > 0)
            {
                var (buttons, joltages) = queue.Dequeue();
                if (Enumerable.Range(0, machine.LightsCount).All(i => joltages[i] == machine.Joltages[i]))
                {
                    bestSequenceLen = buttons.Length;
                    bestSequence = buttons;
                    continue;
                }
                //if (visited.TryGetValue(lightState, out var len) && len < buttons.Length)
                //    continue;
                //visited[lightState] = buttons.Length;
                foreach (var (j, button) in machine.Buttons.Index())
                {
                    //if (!IsRelated(button, machine.Lights, lightState))
                    //    continue;
                    int[] newJoltages = UpdateJoltages(joltages, button);
                    if (!IsJoltageValid(newJoltages, machine))
                        continue;
                    queue.Enqueue(([.. buttons, j], newJoltages));
                }
            }
            if (settings.Verbose)
                Console.WriteLine($"Machine {i}: {string.Join(',', bestSequence)}");
            sum += bestSequenceLen;
        }
        Console.WriteLine($"Minimum button presses required is {sum}");
        */
    }

    private static BitVector32 IntsToVector(IEnumerable<int> ints)
    {
        var vector = new BitVector32();
        foreach (var i in ints)
            vector[1 << i] = true;
        return vector;
    }

    private static int[] CalculateJoltages(Machine machine, int[] buttons)
    {
        int[] joltages = [.. Enumerable.Repeat(0, machine.LightsCount)];
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
    private static int[] UpdateJoltages(int[] joltages, Button button)
    {
        int[] newJoltages = [.. joltages];
        for (var i = 0; i < joltages.Length; ++i)
        {
            if (button.Lights[1 << i])
                ++newJoltages[i];
        }
        return newJoltages;
    }
    private static int[] UpdateJoltages(int[] joltages, int[] otherJoltages)
    {
        int[] newJoltages = [.. joltages];
        for (var i = 0; i < joltages.Length; ++i)
            newJoltages[i] += otherJoltages[i];
        return newJoltages;
    }
    private static bool IsJoltageValid(int[] joltages, Machine machine)
        => Enumerable.Range(0, machine.LightsCount).All(i => joltages[i] <= machine.Joltages[i]);

    public static IEnumerable<(int[], BitVector32)> GenerateButtons(IEnumerable<Button> buttons, BitVector32 initialState)
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
}
