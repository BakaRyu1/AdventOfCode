using AdventOfCode.Utils;
using System.Text.Json;

namespace AdventOfCode._2015;
internal class Day12 : DayRunner<JsonDocument>
{
    public override JsonDocument Parse(FileReference file)
    {
        return JsonDocument.Parse(file.GetText());
    }

    public override void Part1(JsonDocument data, RunSettings settings)
    {
        var sum = SumOf(data.RootElement);
        Console.WriteLine($"Sum of numbers is {sum}");
    }

    public override void Part2(JsonDocument data, RunSettings settings)
    {
        var sum = SumOfNotRed(data.RootElement);
        Console.WriteLine($"Sum of not-red numbers is {sum}");
    }

    private static long SumOf(JsonElement element)
    {
        var sum = 0L;
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                    sum += SumOf(prop.Value);
                break;
            case JsonValueKind.Array:
                foreach (var child in element.EnumerateArray())
                    sum += SumOf(child);
                break;
            case JsonValueKind.Number:
                sum += element.GetInt64();
                break;
        }
        return sum;
    }

    private static long SumOfNotRed(JsonElement element)
    {
        var sum = 0L;
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var sumObj = 0L;
                var hasRed = false;
                foreach (var prop in element.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String && prop.Value.GetString() == "red")
                    {
                        hasRed = true;
                        break;
                    }
                    sumObj += SumOfNotRed(prop.Value);
                }
                if (!hasRed)
                    sum += sumObj;
                break;
            case JsonValueKind.Array:
                foreach (var child in element.EnumerateArray())
                    sum += SumOfNotRed(child);
                break;
            case JsonValueKind.Number:
                sum += element.GetInt64();
                break;
        }
        return sum;
    }
}
