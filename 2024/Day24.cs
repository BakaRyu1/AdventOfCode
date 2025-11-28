using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2024;

internal partial class Day24 : DayRunner<Day24.Data>
{
    public enum Operation
    {
        And,
        Or,
        Xor,
    }
    public class Gate(string input1, Operation operation, string input2, string output)
    {
        public string Input1 = input1;
        public Operation Operation = operation;
        public string Input2 = input2;
        public string Output = output;

        public Gate(Gate gate) : this(gate.Input1, gate.Operation, gate.Input2, gate.Output) { }

        public override string ToString() => $"{Input1} {Operation} {Input2} => {Output}";
    }

    public struct Data
    {
        public Dictionary<string, bool> Initializers;
        public Gate[] Gates;
    }

    public override Data Parse(FileReference file)
    {
        var lines = file.GetLines();
        var initializers = new Dictionary<string, bool>();
        var gates = new List<Gate>();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var initMatch = InitializerPattern().Match(line);
            if (initMatch.Success)
            {
                if (!initializers.TryAdd(initMatch.Groups["name"].Value, initMatch.Groups["value"].Value == "1"))
                {
                    Console.Error.WriteLine("Initializer already exists: " + line);
                    throw new InvalidOperationException();
                }
                continue;
            }
            var gateMatch = GatePattern().Match(line);
            if (gateMatch.Success)
            {
                gates.Add(new(
                    gateMatch.Groups["name1"].Value,
                    gateMatch.Groups["op"].Value switch
                    {
                        "AND" => Operation.And,
                        "OR" => Operation.Or,
                        "XOR" => Operation.Xor,
                        _ => throw new InvalidOperationException()
                    },
                    gateMatch.Groups["name2"].Value,
                    gateMatch.Groups["name3"].Value
                ));
                continue;
            }
            Console.Error.WriteLine("Couldn't parse line: " + line);
            throw new InvalidOperationException();
        }
        return new()
        {
            Initializers = initializers,
            Gates = [.. gates]
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var machine = new Machine(data.Gates);
        machine.Initialize(data.Initializers);
        machine.Process();
        if (settings.Verbose)
        {
            foreach (var name in machine.Values.Keys.Order())
            {
                Console.WriteLine($"{name}: {(machine.Values[name] ? 1 : 0)}");
            }
        }
        var output = machine.ExtractNumber("z");
        Console.WriteLine("Output is " + output);
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var machine = new Machine(data.Gates);

        var carry = machine.GatesFromOutput.Values
            .Where(gate => gate.Output.StartsWith("z"))
            .Select(gate => gate.Output.AsSpan().Slice(1).ToInt())
            .Max();
        var carryName = $"z{carry:00}";

        var inputGates = new List<Gate>();
        var outputGates = new List<Gate>();
        var faultyGates = new HashSet<Gate>();
        foreach (var gate in machine.GatesFromOutput.Values)
        {
            var isInput = ((gate.Input1.StartsWith('x') && gate.Input2.StartsWith('y'))
                    || (gate.Input1.StartsWith('y') && gate.Input2.StartsWith('x')));
            var isOutput = gate.Output.StartsWith('z');
            if (gate.Operation == Operation.Xor)
            {
                if (isInput)
                {
                    if (gate.Input1.EndsWith("00"))
                    {
                        if (gate.Output != "z00")
                        {
                            Console.WriteLine(gate + " (first input must output directly)");
                            faultyGates.Add(gate);
                        }
                        continue;
                    }
                    else if (gate.Output == "z00")
                    {
                        Console.WriteLine(gate + " (first input must output directly)");
                        faultyGates.Add(gate);
                    }
                    if (isOutput)
                    {
                        Console.WriteLine(gate + " (input must not output directly)");
                        faultyGates.Add(gate);
                    }
                }
                else
                {
                    if (!isOutput)
                    {
                        Console.WriteLine(gate + " (non-input xor must be an output)");
                        faultyGates.Add(gate);
                    }
                }
            }
            if (isOutput)
            {
                if (gate.Output == carryName)
                {
                    if (gate.Operation != Operation.Or)
                    {
                        Console.WriteLine(gate + " (output carry must be an or)");
                        faultyGates.Add(gate);
                    }
                }
                else if (gate.Operation != Operation.Xor)
                {
                    Console.WriteLine(gate + " (output non-carry must be a xor)");
                    faultyGates.Add(gate);
                }
            }
            if (gate.Operation == Operation.Xor && isInput)
            {
                if (machine.GatesFromInput.TryGetValue(gate.Output, out var gates))
                {
                    if (!gates.All(gate2 => gate2.Operation == Operation.Xor || gate2.Operation == Operation.And))
                    {
                        Console.WriteLine(gate + " (input xor must be followed by xor/and only)");
                        foreach (var gate2 in gates)
                            Console.WriteLine("\t => " + gate2);
                        faultyGates.Add(gate);
                    }
                    var followup = gates.FirstOrDefault(gate2 => gate2.Operation == Operation.Xor);
                    if (followup != null && !followup.Output.StartsWith('z'))
                    {
                        Console.WriteLine(gate + " (input xor must be followed by xor output)");
                        foreach (var gate2 in gates)
                            Console.WriteLine("\t => " + gate2);
                        faultyGates.Add(gate);
                    }
                }
            }
        }
        foreach (var gate in faultyGates)
        {
            Console.WriteLine(gate);
        }
        var result = TrySwap(machine, faultyGates.Select(gate => gate.Output).ToArray(), [], 4);
        if (result != null)
        {
            foreach (var swaps in result)
            {
                Console.WriteLine(swaps.Item1 + " <=> " + swaps.Item2);
            }
        }
        else
            Console.WriteLine("Not found!");
        /*

        var faultyBits = new SortedSet<int>();
        for (var i = 0; i < 2000; ++i)
        {
            var x = (long)(Random.Shared.NextDouble() * 17592186044417L);
            var y = (long)(Random.Shared.NextDouble() * 17592186044417L);
            machine.Values.Clear();
            machine.SetNumber("x", x);
            machine.SetNumber("y", y);
            machine.Process();
            var z = machine.ExtractNumber("z");
            foreach (var (bit, _, _) in GetFaultyBits(z, x + y))
                faultyBits.Add(bit);
        }
        Console.WriteLine("Faulty bits: " + string.Join(", ", faultyBits));
        */
    }

    private static (string, string)[]? TrySwap(Machine machine, IEnumerable<string> available, IEnumerable<(string, string)> swaps, int remainingSwaps)
    {
        if (remainingSwaps == 0)
        {
            var newMachine = new Machine(machine, swaps);
            for (var i = 0; i < 2000; ++i)
            {
                var x = Random.Shared.NextInt64() & 0xFFFFFFFFFFF;
                var y = Random.Shared.NextInt64() & 0xFFFFFFFFFFF;
                machine.Values.Clear();
                machine.SetNumber("x", x);
                machine.SetNumber("y", y);
                machine.Process();
                var z = machine.ExtractNumber("z");
                if ((x + y) != z)
                    return null;
            }
            return swaps.ToArray();
        }
        var set = new HashSet<string>(available);
        foreach (var name in available)
        {
            set.Remove(name);
            var isOutput = name.StartsWith('z');
            var isXor = machine.GatesFromOutput[name].Operation == Operation.Xor;
            var otherNames = set.ToArray();
            foreach (var otherName in otherNames)
            {
                if (isOutput && machine.GatesFromOutput[otherName].Operation != Operation.Xor)
                    continue;
                if (otherName.StartsWith('z') && !isXor)
                    continue;
                set.Remove(otherName);
                var workingSwaps = TrySwap(machine, set, swaps.Concat([(name, otherName)]), remainingSwaps - 1);
                if (workingSwaps != null)
                    return workingSwaps;
                set.Add(otherName);
            }
            set.Add(name);
        }
        return null;
    }

    private class Machine
    {
        public Dictionary<string, Gate[]> GatesFromInput;
        public Dictionary<string, Gate> GatesFromOutput;
        public Dictionary<string, bool> Values = [];

        public Machine(IEnumerable<Gate> gates)
        {
            GatesFromOutput = gates
                .Select(gate => new Gate(gate))
                .ToDictionary(gate => gate.Output, gate => gate);
            GatesFromInput = RecalculateGates();
            
        }

        public Machine(Machine source, IEnumerable<(string, string)> swaps)
        {
            GatesFromOutput = source.GatesFromOutput.ToDictionary();
            foreach (var pair in swaps)
            {
                var gate1 = GatesFromOutput[pair.Item1];
                var gate2 = GatesFromOutput[pair.Item2];
                GatesFromOutput[pair.Item1]  = new Gate(gate2) { Output = pair.Item1 };
                GatesFromOutput[pair.Item2] = new Gate(gate1) { Output = pair.Item2 };
            }
            GatesFromInput = RecalculateGates();
        }

        private Dictionary<string, Gate[]> RecalculateGates()
        {
            return GatesFromOutput.Values
                .SelectMany<Gate, (string, Gate)>(gate => [(gate.Input1, gate), (gate.Input2, gate)])
                .GroupBy(pair => pair.Item1)
                .ToDictionary(group => group.Key, group => group.Select(pair => pair.Item2).ToArray());
        }

        public IEnumerable<string> GetFinalOutputs(string input)
        {
            var queue = new Queue<string>();
            queue.Enqueue(input);
            while (queue.Count > 0)
            {
                var value = queue.Dequeue();
                if (GatesFromInput.TryGetValue(value, out var gates))
                {
                    foreach (var gate in gates)
                        queue.Enqueue(gate.Output);
                }
                else
                    yield return value;
            }
        }

        public void Process()
        {
            var queue = new Queue<string>();
            var queued = new HashSet<string>();
            foreach (var name in Values.Keys)
            {
                queue.Enqueue(name);
                queued.Add(name);
            }
            while (queue.Count > 0)
            {
                var wire = queue.Dequeue();
                queued.Remove(wire);
                if (!GatesFromInput.TryGetValue(wire, out var gates))
                    continue;
                foreach (var gate in gates)
                {
                    if (!Values.TryGetValue(gate.Input1, out var value1))
                        continue;
                    if (!Values.TryGetValue(gate.Input2, out var value2))
                        continue;
                    var result = gate.Operation switch
                    {
                        Operation.And => value1 && value2,
                        Operation.Or => value1 || value2,
                        Operation.Xor => value1 != value2,
                        _ => throw new InvalidOperationException()
                    };
                    Values[gate.Output] = result;
                    if (!queued.Contains(gate.Output))
                    {
                        queued.Add(gate.Output);
                        queue.Enqueue(gate.Output);
                    }
                }
            }
        }

        public void Initialize(Dictionary<string, bool> initializers)
        {
            foreach (var (name, value) in initializers)
                Values[name] = value;
        }

        public long ExtractNumber(string prefix)
        {
            var output = 0L;
            foreach (var (name, value) in Values)
            {
                if (!value || !name.StartsWith(prefix))
                    continue;
                var num = name.AsSpan().Slice(prefix.Length).ToInt();
                output |= 1L << num;
            }
            return output;
        }

        public void ClearNumber(string prefix)
        {
            foreach (var name in Values.Keys.Where(name => name.StartsWith(prefix)).ToArray())
            {
                Values.Remove(name);
            }
        }

        public void SetNumber(string prefix, long value, bool clear = true)
        {
            if (clear)
                ClearNumber(prefix);
            var num = 0;
            while (value > 0)
            {
                Values[$"{prefix}{num:00}"] = (value & 0x1) != 0;
                value >>>= 1;
                ++num;
            }
        }
    }

    

    

    [GeneratedRegex(@"^\s*(?<name>[a-z0-9]+)\s*:\s*(?<value>[01])\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex InitializerPattern();
    [GeneratedRegex(@"^\s*(?<name1>[a-z0-9]+)\s+(?<op>AND|OR|XOR)\s+(?<name2>[a-z0-9]+)\s*->\s*(?<name3>[a-z0-9]+)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex GatePattern();
    [GeneratedRegex(@"^z(?<num>\d+)$", RegexOptions.CultureInvariant)]
    private static partial Regex OutputNamePattern();
}
