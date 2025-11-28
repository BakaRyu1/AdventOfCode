using AdventOfCode.Utils;
using System.Text.RegularExpressions;

namespace AdventOfCode._2023;

internal partial class Day15 : DayRunner<string[], Day15.StepInfo[]>
{
    public override string[] Parse(FileReference file)
    {
        var text = file.GetText();
        var textSpan = text.AsSpan();
        var steps = new List<string>();
        foreach (var elem in textSpan.Split(','))
        {
            steps.Add(textSpan[elem].Trim().ToString().Replace("\n", ""));
        }
        return [.. steps];
    }

    public override void Part1(string[] data, RunSettings settings)
    {
        var sum = 0;
        foreach (var step in data)
        {
            var hash = GetHash(step);
            sum += hash;
        }
        Console.WriteLine("Sum of step hash is " + sum);
    }
    public struct StepInfo
    {
        public string Label;
        public char Op;
        public int FocalLength;
    }
    public override StepInfo[] Parse2(FileReference file)
    {
        var text = file.GetText();
        var steps = new List<StepInfo>();
        foreach (var elem in text.Split(','))
        {
            var match = StepPattern().Match(elem);
            if (!match.Success)
            {
                Console.Error.WriteLine("Failed to parse step: " + elem);
                throw new InvalidOperationException();
            }
            steps.Add(new()
            {
                Label = match.Groups["label"].Value,
                Op = match.Groups["op"].Value[0],
                FocalLength = match.Groups["op"].Value[0] == '=' ? match.Groups["focal"].ValueSpan.ToInt() : 0
            });
        }
        return [.. steps];
    }
    public override void Part2(StepInfo[] data, RunSettings settings)
    {
        var boxes = new List<(string, int)>[256];
        for (var i = 0; i < boxes.Length; ++i)
            boxes[i] = new();
        foreach (var step in data)
        {
            var boxIndex = GetHash(step.Label);
            var box = boxes[boxIndex];
            var lensIndex = box.FindIndex(lens => lens.Item1 == step.Label);
            switch (step.Op)
            {
                case '-':
                    if (lensIndex >= 0)
                        box.RemoveAt(lensIndex);
                    break;
                case '=':
                    if (lensIndex >= 0)
                        box[lensIndex] = (step.Label, step.FocalLength);
                    else
                        box.Add((step.Label, step.FocalLength));
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
        var focusingPower = boxes.SelectMany((box, boxIndex) => 
            box.Select((lens, lensIndex) => 
                (boxIndex + 1) * (lensIndex + 1) * lens.Item2)).Sum();
        Console.WriteLine("Focusing power is " + focusingPower);
    }

    private static int GetHash(string str)
    {
        var hash = 0;
        foreach (var ch in str)
        {
            hash = (hash + ch) * 17 % 256;
        }
        return hash;
    }

    [GeneratedRegex(@"^(?<label>[a-z]+)(?<op>[=-])(?<focal>\d*)$")]
    private static partial Regex StepPattern();
}
