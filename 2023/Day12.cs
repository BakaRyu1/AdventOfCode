using AdventOfCode.Utils;

namespace AdventOfCode._2023;

internal class Day12 : DayRunner<Day12.RowInfo[]>
{
    public enum State
    {
        Operational,
        Damaged,
        Unknown
    }

    public struct RowInfo
    {
        public State[] States;
        public int[] DamagedGroups;
    }

    public override RowInfo[] Parse(FileReference file)
    {
        var lines = file.GetLines();
        var rows = new List<RowInfo>();
        foreach (var line in lines)
        {
            var parts = line.Split(' ');
            if (parts.Length != 2)
            {
                Console.Error.WriteLine("Couldn't parse line: " + line);
                throw new InvalidOperationException();
            }
            rows.Add(new()
            {
                States = parts[0].Select(ch =>
                {
                    switch (ch)
                    {
                        case '.':
                            return State.Operational;
                        case '#':
                            return State.Damaged;
                        case '?':
                            return State.Unknown;
                        default:
                            Console.Error.WriteLine("Unkown spring state: " + ch);
                            throw new InvalidOperationException();
                    }
                }).ToArray(),
                DamagedGroups = parts[1].AsSpan().SplitInts(',').ToArray()
            });
        }
        return [.. rows];
    }

    public override void Part1(RowInfo[] rows, RunSettings settings)
    {
        var total = 0L;
        foreach (var row in rows)
        {
            if (settings.Verbose)
                Console.WriteLine("Row: " + GetStateString(row.States) + " " + string.Join(',', row.DamagedGroups));
            var possibles = Match(row.States, row.DamagedGroups, visitor: settings.Verbose ? new PrintVisitor() : null);
            if (settings.Verbose)
                Console.WriteLine("\tPossibles: " + possibles);
            total += possibles;
        }
        Console.WriteLine("Total possibles: " + total);
    }

    public override void Part2(RowInfo[] rows, RunSettings settings)
    {
        var total = 0L;
        var cache = new Dictionary<(int, int), long>();
        foreach (var row in rows)
        {
            var unfoldedStates = Enumerable.Range(0, 4)
                .SelectMany(_ => row.States.Concat(Enumerable.Repeat(State.Unknown, 1)))
                .Concat(row.States)
                .ToArray();
            var unfoldedGroups = Enumerable.Range(0, 5)
                .SelectMany(_ => row.DamagedGroups)
                .ToArray();
            if (settings.Verbose)
            {
                Console.WriteLine("Row:\t   " + GetStateString(unfoldedStates));
                Console.WriteLine("\tGroups: " + string.Join(',', unfoldedGroups));
            }

            var possibles = Match(unfoldedStates, unfoldedGroups, cache, settings.Verbose ? new PrintVisitor() : null);

            if (settings.Verbose)
            {
                Console.WriteLine("\tPossibles: " + possibles);
                Console.WriteLine("\tCache size: " + cache.Values.Count);
            }
            cache.Clear();
            total += possibles;
        }
        Console.WriteLine("Total possibles: " + total);
    }

    private static string GetStateString(IEnumerable<State> states)
    {
        return string.Join("", states.Select(state =>
        {
            switch (state)
            {
                case State.Operational: return '.';
                case State.Damaged: return '#';
                case State.Unknown: return '?';
                default:
                    Console.Error.WriteLine("Unkown spring state: " + state);
                    throw new InvalidOperationException();
            }
        }));
    }

    private interface IVisitor
    {
        public void Push(IEnumerable<State> states);
        public void Pop();
        public void Found(IEnumerable<State> states);
        public void CacheHit(int operationalCount, long count);
    }
    private class PrintVisitor : IVisitor
    {
        private readonly Stack<IEnumerable<State>> Stack = [];

        public void Found(IEnumerable<State> states)
        {
            var fullStates = Stack.Aggregate(Enumerable.Empty<State>(), (a, b) => b.Concat(a))
                .Concat(states);
            Console.WriteLine("\t=> " + GetStateString(fullStates));
        }

        public void CacheHit(int operationalCount, long count)
        {
            if (count <= 0)
                return;
            var fullStates = Stack.Aggregate(Enumerable.Empty<State>(), (a, b) => b.Concat(a))
                .Concat(Enumerable.Repeat(State.Operational, operationalCount));
            Console.WriteLine("\t=> " + GetStateString(fullStates) + "--- * " + count);
        }

        public void Pop() => Stack.Pop();
        public void Push(IEnumerable<State> states) => Stack.Push(states);
    }

    private static long Match(State[] states, int[] damagedGroups, Dictionary<(int, int), long>? cache = null, IVisitor? visitor = null)
    {
        if (damagedGroups.Length > 0)
        {
            if (states.Length == 0 || !states.Any(state => state == State.Damaged || state == State.Unknown))
                return 0;
        }
        else
        {
            if (states.All(state => state == State.Operational || state == State.Unknown))
            {
                visitor?.Found(states.Select(_ => State.Operational));
                return 1;
            }
            return 0;
        }
        var operationalCount = states.TakeWhile(state => state == State.Operational).Count();
        if (cache != null && cache.TryGetValue((damagedGroups.Length, states.Length - operationalCount), out var result))
        {
            visitor?.CacheHit(operationalCount, result);
            return result;
        }
        var groupSize = damagedGroups.First();
        var unknownCount = states.Skip(operationalCount)
            .TakeWhile(state => state == State.Unknown).Count();
        var maxShift = states.Skip(operationalCount).Take(unknownCount + groupSize).Count() - groupSize;
        var count = 0L;
        for (var shift = 0; shift <= maxShift; ++shift)
        {
            if (states.Skip(operationalCount + shift).Take(groupSize).All(state => state == State.Damaged || state == State.Unknown))
            {
                var after = states.Skip(operationalCount + shift + groupSize).FirstOrDefault(State.Operational);
                if (after == State.Operational || after == State.Unknown)
                {
                    var eof = !states.Skip(operationalCount + shift + groupSize).Any();
                    visitor?.Push(states.Take(operationalCount)
                        .Concat(Enumerable.Repeat(State.Operational, shift))
                        .Concat(Enumerable.Repeat(State.Damaged, groupSize))
                        .Concat(Enumerable.Repeat(State.Operational, eof ? 0 : 1)));
                    count += Match(states.Skip(operationalCount + shift + groupSize + 1).ToArray(), damagedGroups.Skip(1).ToArray(), cache, visitor);
                    visitor?.Pop();
                }
            }
        }
        if (unknownCount > 0 && states.Skip(operationalCount + unknownCount).FirstOrDefault(State.Operational) == State.Operational)
        {
            visitor?.Push(states.Take(operationalCount)
                .Concat(Enumerable.Repeat(State.Operational, unknownCount + 1)));
            count += Match(states.Skip(operationalCount + unknownCount + 1).ToArray(), damagedGroups, cache, visitor);
            visitor?.Pop();
        }
        cache?.Add((damagedGroups.Length, states.Length - operationalCount), count);
        return count;
    }
}
