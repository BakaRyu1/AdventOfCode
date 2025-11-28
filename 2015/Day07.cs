using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015;
internal partial class Day07 : DayRunner<Day07.Instruction[]>
{
    public enum Operation
    {
        Assign,
        Not,
        And, Or, LShift, RShift
    }
    public struct Operand(ushort value, string? wire)
    {
        public ushort Value = value;
        public string? Wire = wire;

        public Operand(ushort value) : this(value, null) { }
        public Operand(string wire) : this(0, wire) { }
        public readonly ushort Evaluate(Dictionary<string, ushort> wires)
            => Wire != null ? wires.GetValueOrDefault(Wire) : Value;
        public readonly bool IsReady(Dictionary<string, ushort> wires)
            => Wire == null || wires.ContainsKey(Wire);
    }
    public struct Instruction
    {
        private static readonly HashSet<Operation> BINARY_OPERATIONS = [Operation.And, Operation.Or, Operation.LShift, Operation.RShift];

        public Operation Operation;
        public Operand FirstOperand;
        public Operand SecondOperand;
        public string TargetWire;
        public readonly bool IsReady(Dictionary<string, ushort> wires)
            => FirstOperand.IsReady(wires) && (!BINARY_OPERATIONS.Contains(Operation) || SecondOperand.IsReady(wires));
    }
    public override Instruction[] Parse(FileReference file)
    {
        var data = new List<Instruction>();
        foreach (var line in file.GetLines())
        {
            var matcher = InstructionPattern().Match(line);
            if (!matcher.Success)
                throw new InvalidOperationException($"Failed to parse instruction: {line}");
            var op = matcher.Groups["op"];
            var left = matcher.Groups["left"];
            var rightOperand = ParseOperand(matcher.Groups["right"].ValueSpan);
            if (left.Success)
            {
                var leftOperand = ParseOperand(left.ValueSpan);
                data.Add(new()
                {
                    Operation = op.ValueSpan switch
                    {
                        "AND" => Operation.And,
                        "OR" => Operation.Or,
                        "LSHIFT" => Operation.LShift,
                        "RSHIFT" => Operation.RShift,
                        _ => throw new InvalidOperationException($"Unknown operation: {matcher.Groups["op"].ValueSpan}")
                    },
                    FirstOperand = leftOperand,
                    SecondOperand = rightOperand,
                    TargetWire = matcher.Groups["target"].Value
                });
            }
            else
            {
                var isNot = op.Success && op.ValueSpan.SequenceEqual("NOT");
                data.Add(new()
                {
                    Operation = isNot ? Operation.Not : Operation.Assign,
                    FirstOperand = rightOperand,
                    TargetWire = matcher.Groups["target"].Value
                });
            }
        }
        return [.. data];
    }

    private static void EvaluateInstructions(Dictionary<string, ushort> wires, Instruction[] data)
    {
        var queue = new Queue<Instruction>();
        foreach (var instruction in data)
            queue.Enqueue(instruction);
        string? firstRequeued = null;
        while (queue.Count > 0)
        {
            var instruction = queue.Dequeue();
            if (!instruction.IsReady(wires))
            {
                if (firstRequeued != null && firstRequeued == instruction.TargetWire)
                    throw new InvalidOperationException("Loop detected!");
                firstRequeued ??= instruction.TargetWire;
                queue.Enqueue(instruction);
                continue;
            }
            firstRequeued = null;
            if (wires.ContainsKey(instruction.TargetWire))
                continue;
            wires[instruction.TargetWire] = (ushort)(instruction.Operation switch
            {
                Operation.Assign => instruction.FirstOperand.Evaluate(wires),
                Operation.Not => ~instruction.FirstOperand.Evaluate(wires),
                Operation.And => instruction.FirstOperand.Evaluate(wires) & instruction.SecondOperand.Evaluate(wires),
                Operation.Or => instruction.FirstOperand.Evaluate(wires) | instruction.SecondOperand.Evaluate(wires),
                Operation.LShift => instruction.FirstOperand.Evaluate(wires) << instruction.SecondOperand.Evaluate(wires),
                Operation.RShift => instruction.FirstOperand.Evaluate(wires) >> instruction.SecondOperand.Evaluate(wires),
                _ => throw new InvalidOperationException(),
            });
        }
    }

    public override void Part1(Instruction[] data, RunSettings settings)
    {
        var wires = new Dictionary<string, ushort>();
        EvaluateInstructions(wires, data);
        if (settings.Verbose || !wires.TryGetValue("a", out var a))
        {
            foreach (var key in wires.Keys.Order())
                Console.WriteLine($"{key}: {wires[key]}");
        }
        else
            Console.WriteLine($"a: {a}");
    }

    public override void Part2(Instruction[] data, RunSettings settings)
    {
        var wires = new Dictionary<string, ushort>();
        EvaluateInstructions(wires, data);
        var a = wires.GetValueOrDefault("a");
        Console.WriteLine($"First pass a: {a}");
        wires.Clear();
        wires["b"] = a;
        EvaluateInstructions(wires, data);
        if (settings.Verbose || !wires.TryGetValue("a", out a))
        {
            foreach (var key in wires.Keys.Order())
                Console.WriteLine($"{key}: {wires[key]}");
        }
        else
            Console.WriteLine($"Second pass a: {a}");
    }

    [GeneratedRegex(@"^(?:(?<left>\w+|\d+)\s+(?<op>AND|OR|LSHIFT|RSHIFT)\s+|(?<op>NOT)\s*)?(?<right>\w+|\d+)\s*->\s*(?<target>\w+)$")]
    private partial Regex InstructionPattern();

    private static Operand ParseOperand(ReadOnlySpan<char> input)
        => char.IsAsciiLetter(input[0]) ? new(input.ToString()) : new((ushort)input.ToInt());
}
