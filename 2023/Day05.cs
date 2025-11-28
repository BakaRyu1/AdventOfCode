using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2023;

internal partial class Day05 : DayRunner<Day05.Data>
{
    public struct MappingRange
    {
        public long DestinationStart;
        public long SourceStart;
        public long Length;
    }
    public struct Mapping
    {
        public string ID;
        public MappingRange[] Ranges; // Sorted by SourceStart

        public readonly long Map(long value)
        {
            foreach (var range in Ranges)
            {
                if (value >= range.SourceStart && value < (range.SourceStart + range.Length))
                    return value - range.SourceStart + range.DestinationStart;
            }
            return value;
        }
        private static bool SourceRangeIntersects(LongRange range, MappingRange mappingRange)
        {
            return Math.Max(mappingRange.SourceStart, range.Start) <= Math.Min(mappingRange.SourceStart + mappingRange.Length - 1, range.Start + range.Count - 1);
        }
        public readonly IEnumerable<LongRange> Map(LongRange range)
        {
            var list = new List<LongRange>();
            var curRange = range;
            while (curRange.Count > 0)
            {
                var affectedRanges = Ranges
                    .Where(range => SourceRangeIntersects(curRange, range));
                if (!affectedRanges.Any())
                {
                    list.Add(curRange);
                    break;
                }
                var mappingRange = affectedRanges.First();
                if (curRange.Start < mappingRange.SourceStart)
                {
                    var externalCount = mappingRange.SourceStart - curRange.Start;
                    list.Add(new LongRange()
                    {
                        Start = curRange.Start,
                        Count = externalCount
                    });
                    curRange.Start = mappingRange.SourceStart;
                    curRange.Count -= externalCount;
                }
                var start = Math.Max(curRange.Start, mappingRange.SourceStart);
                var end = Math.Min(curRange.Start + curRange.Count, mappingRange.SourceStart + mappingRange.Length);
                var count = end - start;
                list.Add(new LongRange()
                {
                    Start = mappingRange.DestinationStart + (start - mappingRange.SourceStart),
                    Count = count
                });
                curRange.Start += count;
                curRange.Count -= count;
            }
            return list;
        }
    }
    public struct Data
    {
        public long[] Seeds;
        public Dictionary<string, Mapping> Mappings;
    }
    public struct LongRange
    {
        public long Start;
        public long Count;
    }

    public override Data Parse(FileReference file)
    {
        var data = new Data()
        {
            Mappings = []
        };
        string? currentMappingID = null;
        var currentRanges = new List<MappingRange>();
        var lines = file.GetLines();
        foreach (var line in lines)
        {
            if (line.AsSpan().Trim().IsEmpty)
                continue;
            var seedsMatch = SeedsPattern().Match(line);
            if (seedsMatch.Success)
            {
                data.Seeds = seedsMatch.Groups["nums"].ValueSpan
                    .SplitLongs()
                    .ToArray();
                continue;
            }
            var mappingMatch = MappingPattern().Match(line);
            if (mappingMatch.Success)
            {
                if (currentMappingID is not null)
                {
                    currentRanges.Sort((a, b) => Math.Sign(a.SourceStart - b.SourceStart));
                    var mapping = new Mapping()
                    {
                        ID = currentMappingID,
                        Ranges = [.. currentRanges]
                    };
                    data.Mappings.Add(mapping.ID, mapping);
                }
                currentRanges.Clear();
                currentMappingID = mappingMatch.Groups["id"].Value;
            }
            var rangeMatch = RangePattern().Match(line);
            if (rangeMatch.Success)
            {
                if (currentMappingID == null)
                {
                    Console.Error.WriteLine("Found range outside of a mapping.");
                    throw new InvalidOperationException();
                }
                var range = new MappingRange()
                {
                    DestinationStart = rangeMatch.Groups["dest"].ValueSpan.ToLong(),
                    SourceStart = rangeMatch.Groups["source"].ValueSpan.ToLong(),
                    Length = rangeMatch.Groups["length"].ValueSpan.ToLong()
                };
                currentRanges.Add(range);
            }
        }
        if (currentMappingID is not null)
        {
            var mapping = new Mapping()
            {
                ID = currentMappingID,
                Ranges = [.. currentRanges]
            };
            data.Mappings.Add(mapping.ID, mapping);
        }
        return data;
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var mappings = GetMappingsForPath(data, "seed", "soil", "fertilizer", "water", "light", "temperature", "humidity", "location");
        var locations = data.Seeds
            .Select(seed => MapValue(mappings, seed));
        var lowestLocation = locations.Min();
        Console.WriteLine("Lowest location number is " + lowestLocation);
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var mappings = GetMappingsForPath(data, "seed", "soil", "fertilizer", "water", "light", "temperature", "humidity", "location");
        var seedRanges = data.Seeds.Chunk(2)
            .Select(range => new LongRange() { Start = range[0], Count = range[1] });
        var locations = MapRange(mappings, seedRanges);
        var lowestLocation = locations.Min(range => range.Start);
        Console.WriteLine("Lowest location number (with range) is " + lowestLocation);
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day05), settings.Example ? "day05-example.txt" : "day05-input.txt");
    }

    [GeneratedRegex(@"^\s*seeds\s*:\s*(?<nums>\d+(?:\s+\d+)*)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex SeedsPattern();
    [GeneratedRegex(@"^\s*(?<id>[a-z]+-to-[a-z]+)\s+map\s*:\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex MappingPattern();
    [GeneratedRegex(@"^\s*(?<dest>\d+)\s+(?<source>\d+)\s+(?<length>\d+)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex RangePattern();

    private static long MapValue(Mapping[] mappings, long value)
    {
        foreach (var mapping in mappings)
        {
            value = mapping.Map(value);
        }
        return value;
    }

    private static IEnumerable<LongRange> MapRange(Mapping[] mappings, IEnumerable<LongRange> ranges)
    {
        foreach (var mapping in mappings)
        {
            ranges = ranges.SelectMany(range => mapping.Map(range));
        }
        return ranges;
    }
    private static Mapping[] GetMappingsForPath(Data data, params string[] names)
    {
        var list = new List<Mapping>();
        for (var i = 1; i < names.Length; ++i)
        {
            var mappingID = names[i - 1] + "-to-" + names[i];
            if (!data.Mappings.TryGetValue(mappingID, out var mapping))
            {
                Console.Error.WriteLine("Failed to find mapping " + mappingID);
                throw new InvalidOperationException();
            }
            list.Add(mapping);
        }
        return [.. list];
    }
}
