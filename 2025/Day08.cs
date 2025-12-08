using AdventOfCode.Utils;

namespace AdventOfCode._2025;
internal class Day08 : DayRunner<Position3D[]>
{
    public override Position3D[] Parse(FileReference file)
    {
        var points = new List<Position3D>();
        foreach (var line in file.GetLines())
        {
            var numbers = line.AsSpan().SplitInts(',').ToList();
            if (numbers.Count != 3)
                throw new InvalidOperationException($"Expected 3 coordinates, found {numbers.Count}: {line}");
            points.Add(new(numbers[0], numbers[1], numbers[2]));
        }
        return [.. points];
    }

    public override void Part1(Position3D[] data, RunSettings settings)
    {
        var edges = GetAllPossibleConnections(data).OrderBy(tuple => tuple.distanceSqr).ToList();
        var connections = new Dictionary<Position3D, HashSet<Position3D>>();
        var circuits = new List<HashSet<Position3D>>();
        var count = settings.Example ? 10 : 1000;
        foreach (var (p1, p2, d) in edges.Take(count))
        {
            ConnectJunctions(p1, p2, connections, circuits);
            if (settings.Verbose)
                Console.WriteLine($"Connecting {p1}-{p2} ({d}) in circuit of size {connections[p1].Count}");
        }
        if (settings.Verbose)
        {
            Console.WriteLine($"There are {circuits.Count} actual circuits.");
            Console.WriteLine($"The circuits are {string.Join(", ", circuits.Select(set => set.Count))}");
        }
        var largests = circuits.OrderBy(set => set.Count).TakeLast(3);
        if (settings.Verbose)
            Console.WriteLine($"The 3 largests are {string.Join(", ", largests.Select(set => set.Count))}");
        var product = largests.Aggregate(1, (num, set) => num * set.Count);
        Console.WriteLine($"The product is {product}");
    }

    public override void Part2(Position3D[] data, RunSettings settings)
    {
        var edges = GetAllPossibleConnections(data).OrderBy(tuple => tuple.distanceSqr).ToList();
        var connections = new Dictionary<Position3D, HashSet<Position3D>>();
        var circuits = new List<HashSet<Position3D>>();
        var notConnected = data.ToHashSet();
        var i = 0;
        Position3D p1 = Position3D.Zero;
        Position3D p2 = Position3D.Zero;
        while (notConnected.Count > 0 && i < edges.Count)
        {
            (p1, p2, _) = edges[i++];
            ConnectJunctions(p1, p2, connections, circuits);
            notConnected.Remove(p1);
            notConnected.Remove(p2);
        }
        if (settings.Verbose)
            Console.WriteLine($"Last connection required is {p1}-{p2}");
        Console.WriteLine($"The multiplication is {p1.X} * {p2.X} = {p1.X * p2.X}");
    }

    private static IEnumerable<(Position3D p1, Position3D p2, double distanceSqr)> GetAllPossibleConnections(IEnumerable<Position3D> points)
    {
        foreach (var (i, p1) in points.Index())
        {
            foreach (var p2 in points.Skip(i + 1))
            {
                yield return (p1, p2, p1.DistanceSquared(p2));
            }
        }
    }

    private static void ConnectJunctions(Position3D p1, Position3D p2, Dictionary<Position3D, HashSet<Position3D>> connections, List<HashSet<Position3D>> circuits)
    {
        var circuit = connections.GetValueOrDefault(p1);
        var circuitB = connections.GetValueOrDefault(p2);
        if (circuit != null)
        {
            if (circuitB != null)
            {
                if (circuit == circuitB)
                    return;
                if (circuit.Count < circuitB.Count)
                    (circuit, circuitB) = (circuitB, circuit);
                foreach (var p in circuitB)
                {
                    circuit.Add(p);
                    connections[p] = circuit;
                }
                circuits.Remove(circuitB);
            }
        }
        else
        {
            if (circuitB != null)
                circuit = circuitB;
            else
            {
                circuit = [];
                circuits.Add(circuit);
            }
        }
        circuit.Add(p1);
        circuit.Add(p2);
        connections[p1] = connections[p2] = circuit;
    }
}
