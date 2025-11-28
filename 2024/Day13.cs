using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2024;

internal partial class Day13 : DayRunner<Day13.Data[]>
{
    public struct Data
    {
        public Position ButtonA;
        public Position ButtonB;
        public Position Prize;
    }

    public override Data[] Parse(FileReference file)
    {
        var lines = file.GetLines();
        var datas = new List<Data>();
        Position? buttonA = null;
        Position? buttonB = null;
        Position? prize = null;
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
            {
                if (buttonA != null && buttonB != null && prize != null)
                {
                    datas.Add(new() { ButtonA = buttonA.Value, ButtonB = buttonB.Value, Prize = prize.Value });
                    buttonA = buttonB = prize = null;
                }
                else if (buttonA != null || buttonB != null || prize != null)
                {
                    Console.Error.WriteLine("Incomplete machine description.");
                    throw new InvalidOperationException();
                }
                continue;
            }
            var match = LinePattern().Match(line);
            if (!match.Success)
            {
                Console.Error.WriteLine("Couldn't parse line: " + line);
                throw new InvalidOperationException();
            }
            var info = new Position(match.Groups["x"].ValueSpan.ToInt(), match.Groups["y"].ValueSpan.ToInt());
            switch (match.Groups["button"].ValueSpan)
            {
                case "A":
                    if (buttonA != null)
                    {
                        Console.Error.WriteLine("Multiple definition for button A: " + line);
                        throw new InvalidOperationException();
                    }
                    buttonA = info;
                    break;
                case "B":
                    if (buttonB != null)
                    {
                        Console.Error.WriteLine("Multiple definition for button B: " + line);
                        throw new InvalidOperationException();
                    }
                    buttonB = info;
                    break;
                case "":
                    if (prize != null)
                    {
                        Console.Error.WriteLine("Multiple definition for prize: " + line);
                        throw new InvalidOperationException();
                    }
                    prize = info;
                    break;
                default:
                    throw new InvalidCastException();
            }
        }
        if (buttonA != null && buttonB != null && prize != null)
        {
            datas.Add(new() { ButtonA = buttonA.Value, ButtonB = buttonB.Value, Prize = prize.Value });
            buttonA = buttonB = prize = null;
        }
        else if (buttonA != null || buttonB != null || prize != null)
        {
            Console.Error.WriteLine("Incomplete machine description.");
            throw new InvalidOperationException();
        }
        return [.. datas];
    }

    public override void Part1(Data[] data, RunSettings settings)
    {
        long sum = 0;
        foreach(var (i, machine) in data.Index())
        {
            var best = SolveEquations(machine.ButtonA, machine.ButtonB, machine.Prize);
            if (best.a >= 0 && best.b >= 0)
            {
                var cost = best.a * 3 + best.b;
                if (settings.Verbose)
                    Console.WriteLine("Machine " + (i + 1) + " has A=" + best.a + " B=" + best.b + " => " + cost + " tokens");
                sum += cost;
            }
        }
        Console.WriteLine("Fewest cost is " + sum + " tokens");
    }

    public override void Part2(Data[] data, RunSettings settings)
    {
        long sum = 0;
        var fix = (10000000000000, 10000000000000);
        foreach (var (i, machine) in data.Index())
        {
            var best = SolveEquations(machine.ButtonA, machine.ButtonB, machine.Prize + fix);
            if (best.a >= 0 && best.b >= 0)
            {
                var cost = best.a * 3 + best.b;
                if (settings.Verbose)
                    Console.WriteLine("Machine " + (i + 1) + " has A=" + best.a + " B=" + best.b + " => " + cost + " tokens");
                sum += cost;
            }
        }
        Console.WriteLine("Fewest cost (corrected) is " + sum + " tokens");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day13), settings.Example ? "day13-example.txt" : "day13-input.txt");
    }

    [GeneratedRegex(@"^\s*(?:Button\s+(?<button>[AB])\s*:\s*X\s*\+\s*(?<x>\d+)\s*,\s*Y\s*\+\s*(?<y>\d+)|Prize\s*:\s*X\s*=\s*(?<x>\d+)\s*,\s*Y\s*=\s*(?<y>\d+))\s*$")]
    private static partial Regex LinePattern();

    private static (long a, long b) SolveEquations(Position buttonA, Position buttonB, LongPosition prize)
    {
        var (a, b) = buttonA;
        var (c, d) = buttonB;
        var (e, f) = prize;
        var det = a * d - b *c;
        var d1 = e * d - c * f;
        var d2 = a * f - e * b;
        if (det == 0 || (d1 % det) != 0 || (d2 % det) != 0)
            return (-1, -1);
        return (d1 / det, d2 / det);
    }
}
