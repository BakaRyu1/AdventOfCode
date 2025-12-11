using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2015;
internal partial class Day23 : DayRunner<Day23.Instruction[]>
{
    public enum InstructionType
    {
        Half,
        Triple,
        Increase,
        Jump,
        JumpIfEven,
        JumpIfOne
    }
    public struct Instruction
    {
        public InstructionType Type;
        public char Register;
        public int Offset;
    }

    private static readonly Dictionary<string, InstructionType> INSTRUCTIONS = new()
    {
        {"hlf", InstructionType.Half },
        {"tpl", InstructionType.Triple},
        {"inc", InstructionType.Increase },
        {"jmp", InstructionType.Jump },
        {"jie", InstructionType.JumpIfEven },
        {"jio", InstructionType.JumpIfOne }
    };
    public override Instruction[] Parse(FileReference file)
    {
        var instructions = new List<Instruction>();
        foreach (var line in file.GetLines())
        {
            var match = InstructionPattern().Match(line);
            if (!match.Success)
                throw new InvalidOperationException($"Failed to parse instruction: {line}");
            if (!INSTRUCTIONS.TryGetValue(match.Groups["ins"].Value, out var type))
                throw new InvalidCastException();
            switch (type)
            {
                case InstructionType.Half:
                case InstructionType.Triple:
                case InstructionType.Increase:
                    instructions.Add(new()
                    {
                        Type = type,
                        Register = match.Groups["reg"].ValueSpan[0]
                    });
                    break;
                case InstructionType.Jump:
                    instructions.Add(new()
                    {
                        Type = type,
                        Offset = match.Groups["offset"].ValueSpan.ToInt()
                    });
                    break;
                case InstructionType.JumpIfEven:
                case InstructionType.JumpIfOne:
                    instructions.Add(new()
                    {
                        Type = type,
                        Register = match.Groups["reg"].ValueSpan[0],
                        Offset = match.Groups["offset"].ValueSpan.ToInt()
                    });
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        return [.. instructions];
    }

    public override void Part1(Instruction[] data, RunSettings settings)
    {
        var registers = new Dictionary<char, int>();
        ExecuteInstructions(data, registers);
        if (settings.Verbose)
        {
            foreach (var register in registers.Keys.Order())
                Console.WriteLine($"{register}: {registers[register]}");
        }
        else
            Console.WriteLine($"b: {registers.GetValueOrDefault('b')}");
    }

    public override void Part2(Instruction[] data, RunSettings settings)
    {
        var registers = new Dictionary<char, int> { ['a'] = 1 };
        ExecuteInstructions(data, registers);
        if (settings.Verbose)
        {
            foreach (var register in registers.Keys.Order())
                Console.WriteLine($"{register}: {registers[register]}");
        }
        else
            Console.WriteLine($"b: {registers.GetValueOrDefault('b')}");
    }

    [GeneratedRegex(@"^\s*(?:(?<ins>hlf|tpl|inc)\s+(?<reg>\w)|(?<ins>jmp)\s+(?<offset>[-+]\d+)|(?<ins>jie|jio)\s+(?<reg>\w)\s*,\s*(?<offset>[-+]\d+))\s*$")]
    private static partial Regex InstructionPattern();

    private static void ExecuteInstructions(Instruction[] instructions, Dictionary<char, int> registers)
    {
        var index = 0;
        while (index < instructions.Length)
        {
            var ins = instructions[index];
            switch (ins.Type)
            {
                case InstructionType.Half:
                    registers[ins.Register] = registers.GetValueOrDefault(ins.Register) / 2;
                    break;
                case InstructionType.Triple:
                    registers[ins.Register] = registers.GetValueOrDefault(ins.Register) * 3;
                    break;
                case InstructionType.Increase:
                    registers[ins.Register] = registers.GetValueOrDefault(ins.Register) + 1;
                    break;
                case InstructionType.Jump:
                    index += ins.Offset;
                    continue;
                case InstructionType.JumpIfEven:
                    if ((registers.GetValueOrDefault(ins.Register) % 2) == 0)
                    {
                        index += ins.Offset;
                        continue;
                    }
                    break;
                case InstructionType.JumpIfOne:
                    if (registers.GetValueOrDefault(ins.Register) == 1)
                    {
                        index += ins.Offset;
                        continue;
                    }
                    break;
            }
            ++index;
        }
    }
}
