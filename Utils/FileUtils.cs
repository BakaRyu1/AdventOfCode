using System.Text;

namespace AdventOfCode.Utils;

public struct FileReference
{
    public enum FileType
    {
        Normal,
        Resource
    }

    public string Name;
    public FileType Type;
    public Type? RelatedType;
    public readonly Stream GetStream()
    {
        switch (Type)
        {
            case FileType.Normal:
                return File.Open(Name, FileMode.Open);
            case FileType.Resource:
                var stream = RelatedType?.Assembly.GetManifestResourceStream($"{RelatedType?.Namespace}.{Name}");
                return stream
                    ?? throw new FileNotFoundException($"Couldn't find resource {Name} for type {RelatedType?.FullName}");
            default:
                throw new InvalidOperationException();
        }
    }
    public readonly string GetText()
    {
        using var stream = GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    public readonly IEnumerable<string> GetLines()
    {
        using var stream = GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string? line;
        while ((line = reader.ReadLine()) != null)
            yield return line;
    }
    public readonly IEnumerable<string> GetRectangle()
    {
        var lines = GetLines().Select(line => line.Trim()).SkipWhile(line => line.Length == 0);
        var width = -1;
        var eof = false;
        foreach (var line in lines)
        {
            if (line.Length == 0)
            {
                eof = true;
                continue;
            }
            else if (eof)
                throw new InvalidOperationException("Rectangle has an empty line.");
            if (width < 0)
                width = line.Length;
            else if (line.Length != width)
                throw new InvalidOperationException("Line length doesn't match");
            yield return line;
        }
    }
    public static FileReference Normal(string name)
        => new() { Name = name, Type = FileType.Normal };
    public static FileReference Resource(Type type, string name)
        => new() { Name = name, Type = FileType.Resource, RelatedType = type };
}

internal static class FileUtils
{

}
