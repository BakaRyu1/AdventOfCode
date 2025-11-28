using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day07 : DayRunner<Day07.Equation[]>
{
    public struct Equation
    {
        public long TestValue;
        public long[] Numbers;
    }
    public override Equation[] Parse(FileReference file)
    {
        var lines = file.GetLines();
        var equations = new List<Equation>();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var parts = line.Split(":");
            if (parts.Length != 2)
            {
                Console.Error.WriteLine("Couldn't parse equation: " + line);
                continue;
            }
            var testValue = parts[0].AsSpan().ToLong();
            var numbers = parts[1].AsSpan().Trim().SplitLongs();
            equations.Add(new Equation
            {
                TestValue = testValue,
                Numbers = numbers.ToArray()
            });
        }
        return [.. equations];
    }

    public override void Part1(Equation[] equations, RunSettings settings)
    {
        long sum = 0;
        foreach (var equation in equations)
        {
            if (IsEquationValid(equation.TestValue, equation.Numbers.First(), equation.Numbers.Skip(1)))
                sum += equation.TestValue;
            else
            {
                if (settings.Verbose)
                    Console.WriteLine("Rejected: " + equation.TestValue + ": " + string.Join(' ', equation.Numbers));
            }
        }
        if (settings.Verbose)
            Console.WriteLine();
        Console.WriteLine("Sum of possible test values is " + sum);
    }

    public override void Part2(Equation[] equations, RunSettings settings)
    {
        long sum = 0;
        foreach (var equation in equations)
        {
            if (IsEquationValidWithConcatenate(equation.TestValue, equation.Numbers.First(), equation.Numbers.Skip(1)))
                sum += equation.TestValue;
            else
            {
                if (settings.Verbose)
                    Console.WriteLine("Rejected: " + equation.TestValue + ": " + string.Join(' ', equation.Numbers));
            }
        }
        if (settings.Verbose)
            Console.WriteLine();
        Console.WriteLine("Sum of possible test values (with concatenation) is " + sum);
    }

    private static bool IsEquationValid(long testValue, long currentValue, IEnumerable<long> numbers)
    {
        if (!numbers.Any())
            return testValue == currentValue;
        var first = numbers.First();
        var remaining = numbers.Skip(1);
        return IsEquationValid(testValue, first + currentValue, remaining)
            || IsEquationValid(testValue, first * currentValue, remaining);
    }
    private static long Concatenate(long first, long second)
    {
        return (first.ToString() + second.ToString()).AsSpan().ToLong();
        //return first * (long)Math.Pow(10, Math.Floor(Math.Log10(second)) + 1) + second;
    }
    private static bool IsEquationValidWithConcatenate(long testValue, long currentValue, IEnumerable<long> numbers)
    {
        if (!numbers.Any())
            return testValue == currentValue;
        var first = numbers.First();
        var remaining = numbers.Skip(1);
        return IsEquationValidWithConcatenate(testValue, currentValue + first, remaining)
            || IsEquationValidWithConcatenate(testValue, currentValue * first, remaining)
            || IsEquationValidWithConcatenate(testValue, Concatenate(currentValue, first), remaining);
    }
}
