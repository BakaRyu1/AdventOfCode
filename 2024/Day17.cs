using AdventOfCode.Utils;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace AdventOfCode._2024;

internal partial class Day17 : DayRunner<Day17.Data>
{
    public enum InstructionType
    {
        Adv,
        Bxl,
        Bst,
        Jnz,
        Bxc,
        Out,
        Bdv,
        Cdv
    }

    public class Data
    {
        public long A, B, C;
        public required int[] Instructions;
    }
    public override Data Parse(FileReference file)
    {
        var lines = file.GetLines();
        long? A = null;
        long? B = null;
        long? C = null;
        int[]? Instructions = null;
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var registerMatch = RegisterPattern().Match(line);
            if (registerMatch.Success)
            {
                var value = registerMatch.Groups["num"].ValueSpan.ToInt();
                switch (registerMatch.Groups["reg"].ValueSpan)
                {
                    case "A":
                        if (A != null)
                        {
                            Console.Error.WriteLine("Register A is defined multiple times.");
                            throw new InvalidOperationException();
                        }
                        A = value;
                        break;
                    case "B":
                        if (B != null)
                        {
                            Console.Error.WriteLine("Register B is defined multiple times.");
                            throw new InvalidOperationException();
                        }
                        B = value;
                        break;
                    case "C":
                        if (C != null)
                        {
                            Console.Error.WriteLine("Register C is defined multiple times.");
                            throw new InvalidOperationException();
                        }
                        C = value;
                        break;
                    default: throw new InvalidOperationException();
                }
                continue;
            }
            var programMatch = ProgramPattern().Match(line);
            if (programMatch.Success)
            {
                if (Instructions != null)
                {
                    Console.Error.WriteLine("Instructions is defined multiple times.");
                    throw new InvalidOperationException();
                }
                Instructions = programMatch.Groups["instructions"].ValueSpan
                    .SplitInts(',')
                    .Select(part =>
                    {
                        if (part > 8)
                        {
                            Console.Error.WriteLine("Invalid instruction: " + part);
                            throw new InvalidOperationException();
                        }
                        return part;
                    })
                    .ToArray();
                continue;
            }
            Console.Error.WriteLine("Couldn't parse line: " + line);
            throw new InvalidOperationException();
        }
        if (A == null || B == null || C == null || Instructions == null)
        {
            Console.WriteLine("Program is not fully specified.");
            throw new InvalidOperationException();
        }
        return new()
        {
            A = A.Value,
            B = B.Value,
            C = C.Value,
            Instructions = Instructions
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var state = new State(data);
        //ExecuteProgram(data, state);
        var program = CompileProgram(data);
        program(state);
        if (settings.Verbose)
        {
            PrintProgram(data);
            Console.WriteLine("A = " + state.A);
            Console.WriteLine("B = " + state.B);
            Console.WriteLine("C = " + state.C);
            Console.WriteLine("InstructionPointer = " + state.InstructionPointer);
        }
        Console.WriteLine("Program output is " + string.Join(',', state.Output));
    }

    public override void Part2(Data data, RunSettings settings)
    {
        if (settings.Example)
        {
            Console.WriteLine("Skipping part 2 because it doesn't work on the example.");
            return;
        }
        var program = CompileProgram(data);
        var A = 1L;
        if (settings.Verbose)
        {
            Console.WriteLine("Searching for program: " + string.Join(',', data.Instructions));
        }
        do
        {
            var state = new State(data)
            {
                A = A
            };
            program(state);
            var skipLength = data.Instructions.Length - state.Output.Count;
            if (data.Instructions.Skip(skipLength).SequenceEqual(state.Output))
            {
                if (settings.Verbose)
                {
                    Console.WriteLine("Found with A = " + A + " => " + string.Join(',', state.Output));
                    Console.WriteLine("\tBase 8 is " + string.Join(',', GetBase8(A)));
                }
                if (state.Output.Count == data.Instructions.Length)
                    break;
                A = A << 3;
            }
            else
                A = A + 1;
        } while (A >= 0);
        if (A >= 0)
            Console.WriteLine("The actual value of A is " + A);
        else
            Console.WriteLine("Failed to find valid value for A.");
    }

    private class State(Data data)
    {
        public long A = data.A;
        public long B = data.B;
        public long C = data.C;
        public int InstructionPointer = 0;
        public List<int> Output = [];

        public void Reset(Data data)
        {
            A = data.A; B = data.B; C = data.C;
            InstructionPointer = 0;
            Output.Clear();
        }
    }

    private static Expression CompileCombo(int combo, Expression A, Expression B, Expression C)
    {
        switch (combo)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                return Expression.Constant((long)combo);
            case 4: return A;
            case 5: return B;
            case 6: return C;
            default:
                Console.Error.WriteLine("Combo operand " + combo + " isn't supported.");
                throw new InvalidOperationException();
        }
    }

    private static Action<State> CompileProgram(Data data)
    {
        var exprs = new List<Expression>();
        var labels = new Dictionary<int, LabelTarget>();
        var state = Expression.Parameter(typeof(State), "state");
        var A = Expression.Field(state, "A");
        var B = Expression.Field(state, "B");
        var C = Expression.Field(state, "C");
        var Output = Expression.Field(state, "Output");
        
        foreach (var pair in data.Instructions.Chunk(2))
        {
            var instruction = (InstructionType)pair[0];
            var operand = pair[1];
            switch (instruction)
            {
                case InstructionType.Adv:
                    exprs.Add(Expression.Assign(A, Expression.RightShift(A, Expression.Convert(CompileCombo(operand, A, B, C), typeof(int)))));
                    break;
                case InstructionType.Bxl:
                    exprs.Add(Expression.ExclusiveOrAssign(B, Expression.Constant((long)operand)));
                    break;
                case InstructionType.Bst:
                    exprs.Add(Expression.Assign(B, Expression.Modulo(CompileCombo(operand, A, B, C), Expression.Constant(8L))));
                    break;
                case InstructionType.Jnz:
                    if ((operand % 2) != 0)
                        throw new InvalidOperationException();
                    if (!labels.TryGetValue(operand / 2, out var label))
                        labels[operand / 2] = label = Expression.Label("line_" + operand);
                    exprs.Add(Expression.IfThen(Expression.NotEqual(A, Expression.Constant(0L)), Expression.Goto(label)));
                    break;
                case InstructionType.Bxc:
                    exprs.Add(Expression.ExclusiveOrAssign(B, C));
                    break;
                case InstructionType.Out:
                    var value = Expression.Convert(Expression.Modulo(CompileCombo(operand, A, B, C), Expression.Constant(8L)), typeof(int));
                    exprs.Add(Expression.Call(Output, "Add", null, value));
                    break;
                case InstructionType.Bdv:
                    exprs.Add(Expression.Assign(B, Expression.RightShift(A, Expression.Convert(CompileCombo(operand, A, B, C), typeof(int)))));
                    break;
                case InstructionType.Cdv:
                    exprs.Add(Expression.Assign(C, Expression.RightShift(A, Expression.Convert(CompileCombo(operand, A, B, C), typeof(int)))));
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        var breakTarget = Expression.Label();
        var block = Expression.Block(
            exprs.SelectMany<Expression, Expression>((expr, i) =>
            {
                if (labels.TryGetValue(i, out var label))
                    return [Expression.Label(label), expr];
                return [expr];
            })
        );
        return Expression.Lambda<Action<State>>(block, state).Compile();
    }

    private static string GetComboOperandName(int combo)
    {
        switch (combo)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                return combo.ToString();
            case 4: return "A";
            case 5: return "B";
            case 6: return "C";
            default:
                Console.Error.WriteLine("Combo operand " + combo + " isn't supported.");
                throw new InvalidOperationException();
        }
    }

    private static List<int> GetBase8(long value)
    {
        var list = new List<int>();
        while (value > 0)
        {
            list.Add((int)(value & 0x7));
            value = value >> 3;
        }
        return list;
    }

    private static void PrintProgram(Data data)
    {
        foreach (var pair in data.Instructions.Chunk(2))
        {
            var instruction = (InstructionType)pair[0];
            var operand = pair[1];
            switch (instruction)
            {
                case InstructionType.Adv:
                    Console.WriteLine("A = A >> " + GetComboOperandName(operand));
                    break;
                case InstructionType.Bxl:
                    Console.WriteLine("B = B ^ " + operand);
                    break;
                case InstructionType.Bst:
                    Console.WriteLine("B = " + GetComboOperandName(operand) + " % 8");
                    break;
                case InstructionType.Jnz:
                    Console.WriteLine("Jnz " + operand);
                    break;
                case InstructionType.Bxc:
                    Console.WriteLine("B = B ^ C");
                    break;
                case InstructionType.Out:
                    Console.WriteLine("Output += " + GetComboOperandName(operand) + " % 8");
                    break;
                case InstructionType.Bdv:
                    Console.WriteLine("B = A >> " + GetComboOperandName(operand));
                    break;
                case InstructionType.Cdv:
                    Console.WriteLine("C = A >> " + GetComboOperandName(operand));
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    private static void ExecuteProgram(Data data, State state)
    {
        while (state.InstructionPointer < data.Instructions.Length)
        {
            var instruction = (InstructionType)data.Instructions[state.InstructionPointer];
            var operand = data.Instructions[state.InstructionPointer + 1];
            switch (instruction)
            {
                case InstructionType.Adv:
                    state.A = state.A >> (int)GetComboOperand(state, operand);
                    break;
                case InstructionType.Bxl:
                    state.B = state.B ^ operand;
                    break;
                case InstructionType.Bst:
                    state.B = GetComboOperand(state, operand) % 8;
                    break;
                case InstructionType.Jnz:
                    if (state.A != 0)
                    {
                        state.InstructionPointer = operand;
                        continue;
                    }
                    break;
                case InstructionType.Bxc:
                    state.B = state.B ^ state.C;
                    break;
                case InstructionType.Out:
                    state.Output.Add((int)(GetComboOperand(state, operand) % 8));
                    break;
                case InstructionType.Bdv:
                    state.B = state.A >> (int)GetComboOperand(state, operand);
                    break;
                case InstructionType.Cdv:
                    state.C = state.A >> (int)GetComboOperand(state, operand);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            state.InstructionPointer += 2;
        }
    }

    private static long GetComboOperand(State state, int combo)
    {
        switch (combo)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                return combo;
            case 4: return state.A;
            case 5: return state.B;
            case 6: return state.C;
            default:
                Console.Error.WriteLine("Combo operand " + combo + " isn't supported.");
                throw new InvalidOperationException();
        }
    }

    [GeneratedRegex(@"^\s*Register\s+(?<reg>[A-C])\s*:\s*(?<num>\d+)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex RegisterPattern();
    [GeneratedRegex(@"^\s*Program\s*:\s*(?<instructions>\d+(?:\s*,\s*\d+)+)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex ProgramPattern();
}
