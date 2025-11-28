using AdventOfCode.Utils;
using System.Text;

namespace AdventOfCode._2015;
internal class Day08 : DayRunner<string[]>
{
    public override string[] Parse(FileReference file)
    {
        return [.. file.GetLines()];
    }

    public override void Part1(string[] data, RunSettings settings)
    {
        var delta = 0;
        foreach (var line in data)
        {
            var evaluated = EvaluateString(line);
            if (settings.Verbose)
                Console.WriteLine($"{line} => {evaluated}");
            delta += (line.Length - evaluated.Length);
        }
        Console.WriteLine($"Difference in memory when evaluated is {delta}");
    }

    public override void Part2(string[] data, RunSettings settings)
    {
        var delta = 0;
        foreach (var line in data)
        {
            var escaped = EscapeString(line);
            if (settings.Verbose)
                Console.WriteLine($"{line} => {escaped}");
            delta += (escaped.Length - line.Length);
        }
        Console.WriteLine($"Difference in memory when escaped is {delta}");
    }

    private static string EvaluateString(string input)
    {
        if (input.Length < 2 || input[0] != '"' || input[^1] != '"')
            throw new InvalidOperationException($"Input is not a string literal: {input}");
        var sb = new StringBuilder();
        for (var i = 1; i < input.Length - 1; ++i)
        {
            switch (input[i])
            {
                case '\\':
                    switch (input[i + 1])
                    {
                        case '"':
                            if (i == input.Length - 2)
                                throw new InvalidOperationException($"String literal is not closed: {input}");
                            sb.Append('"');
                            ++i;
                            break;
                        case '\\':
                            sb.Append('\\');
                            ++i;
                            break;
                        case 'x':
                            if ((i + 3) >= (input.Length - 1))
                                throw new InvalidOperationException($"Hexadecimal escape sequence is not complete: {input}");
                            sb.Append((char)Convert.FromHexString(input.AsSpan(i + 2, 2))[0]);
                            i += 3;
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported escape sequence: \\{input[i + 1]}");
                    }
                    break;
                case '"':
                    throw new InvalidOperationException($"Unescaped double-quote in the middle of the string literal: {input}");
                default:
                    sb.Append(input[i]);
                    break;
            }
        }
        return sb.ToString();
    }

    private static string EscapeString(string input)
    {
        var sb = new StringBuilder();
        sb.Append('"');
        foreach (var ch in input)
        {
            switch (ch)
            {
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }
}
