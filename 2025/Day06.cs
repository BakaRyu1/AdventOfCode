using AdventOfCode.Utils;

namespace AdventOfCode._2025;
internal class Day06 : DayRunner<Day06.Data, Day06.Data>
{
    public struct Data
    {
        public long[][] Numbers;
        public char[] Operators;
    }
    public override Data Parse(FileReference file)
    {
        var ended = false;
        var numbers = new List<long[]>();
        char[]? operators = null;
        foreach (var line in file.GetLines())
        {
            if (ended)
                throw new InvalidOperationException($"Unexpected trailing line after operators: {line}"); ;
            if (line.Any(ch => !char.IsAsciiDigit(ch) && !char.IsWhiteSpace(ch))) {
                if (numbers.Count == 0)
                    throw new InvalidOperationException("No numbers found");
                operators = [.. line.AsSpan().SplitAsStrings().Select(str =>
                {
                    if (str.Length != 1 || !OPERATORS.ContainsKey(str[0]))
                        throw new InvalidOperationException($"Expected operator, found: {str}");
                    return str[0];
                })];
                if (numbers[0].Length != operators.Length)
                    throw new InvalidOperationException($"Operator count mismatch: expected {numbers[0].Length}, found {operators.Length}");
                ended = true;
            }
            else
            {
                long[] row = [.. line.AsSpan().SplitLongs()];
                if (numbers.Count > 0 && numbers[0].Length != row.Length)
                    throw new InvalidOperationException($"Number count mismatch: expected {numbers[0].Length}, found {row.Length}");
                numbers.Add(row);
            }
        }
        if (operators == null)
            throw new InvalidOperationException("Missing operators");
        return new()
        {
            Numbers = Array2D<long>.From(numbers).AsTransposed().To2DArray(),
            Operators = [.. operators]
        };
    }
    public override Data Parse2(FileReference file)
    {
        var operators = new List<char>();
        var numbers = new List<long[]>();
        var lines = file.GetLines().ToList();
        var operatorsLine = lines[^1];
        var numberLines = lines[..^1];
        var row = new List<long>();
        for (var i = operatorsLine.Length - 1; i >= 0; --i)
        {
            var number = string.Join("", numberLines.Select(line => line[i])).Trim();
            if (number.Length > 0)
                row.Add(number.AsSpan().ToLong());
            if (OPERATORS.ContainsKey(operatorsLine[i]))
            {
                operators.Add(operatorsLine[i]);
                numbers.Add([.. row]);
                row.Clear();
            }
            else if (!char.IsWhiteSpace(operatorsLine[i]))
                throw new InvalidOperationException($"Expected operator, found: {operatorsLine[i]}");
        }
        return new()
        {
            Numbers = [.. numbers],
            Operators = [.. operators]
        };
    }
    public override void Part1(Data data, RunSettings settings)
    {
        var total = SumOfAnswers(data, settings.Verbose);
        Console.WriteLine($"Total sum of answers is {total}");
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var total = SumOfAnswers(data, settings.Verbose);
        Console.WriteLine($"Total sum of answers is {total}");
    }

    private static readonly Dictionary<char, Func<long, long, long>> OPERATORS = new() {
        { '+', (a, b) => a + b },
        { '*', (a, b) => a * b }
    };

    private static long SumOfAnswers(Data data, bool verbose)
    {
        var total = 0L;
        for (var i = 0; i < data.Operators.Length; ++i)
        {
            var aggregate = data.Numbers[i].Aggregate(OPERATORS[data.Operators[i]]);
            if (verbose)
               Console.WriteLine($"{string.Join($" {data.Operators[i]} ", data.Numbers[i])} = {aggregate}");
            total += aggregate;
        }
        return total;
    }
}
