using AdventOfCode.Utils;

namespace AdventOfCode._2024;

internal class Day09 : DayRunner<Day09.Data>
{
    public struct FileData
    {
        public int ID;
        public int Offset;
        public int Length;
    }
    public struct FreeSpaceData
    {
        public int Offset;
        public int Length;
    }
    public struct Data
    {
        public FileData[] Files;
        public FreeSpaceData[] FreeSpaces;
        public int TotalSpace;
    }
    public override Data Parse(FileReference file)
    {
        var text = file.GetText();
        var files = new List<FileData>();
        var freeSpaces = new List<FreeSpaceData>();
        var totalSpace = 0;
        var id = 0;
        var isFile = true;
        foreach (var ch in text.Trim())
        {
            if (ch < '0' || ch > '9')
            {
                Console.Error.WriteLine("Invalid disk map character: " + ch);
                throw new InvalidOperationException();
            }
            var length = ch - '0';
            if (isFile)
            {
                files.Add(new()
                {
                    ID = id++,
                    Offset = totalSpace,
                    Length = length
                });
            }
            else if (length > 0)
            {
                freeSpaces.Add(new()
                {
                    Offset = totalSpace,
                    Length = length
                });
            }
            isFile = !isFile;
            totalSpace += length;
        }
        return new()
        {
            Files = files.ToArray(),
            FreeSpaces = freeSpaces.ToArray(),
            TotalSpace = totalSpace
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var disk = InitDisk(data);
        var readPos = disk.Length - 1;
        var writePos = 0;
        while (readPos > 0 && disk[readPos] == -1)
            --readPos;
        foreach (var freeSpace in data.FreeSpaces)
        {
            if (freeSpace.Offset > readPos)
            {
                while (writePos < disk.Length && disk[writePos] != -1)
                    ++writePos;
                break;
            }
            writePos = freeSpace.Offset;
            for (int i = 0; i < freeSpace.Length; ++i)
            {
                disk[writePos] = disk[readPos];
                disk[readPos] = -1;
                ++writePos;
                do
                {
                    --readPos;
                } while (readPos > writePos &&  disk[readPos] == -1);
                if (readPos <= writePos)
                    break;
            }
            if (readPos <= writePos)
                break;
        }
        while (readPos > writePos)
        {
            disk[writePos] = disk[readPos];
            disk[readPos] = -1;
            do
            {
                --readPos;
            } while (readPos > writePos && disk[readPos] == -1);
            ++writePos;
        }
        var checksum = CalculateDiskChecksum(disk);
        if (settings.Verbose)
            Console.WriteLine("Disk: " + string.Join(' ', disk.Take(disk.AsSpan().LastIndexOfAnyExcept(-1) + 1)));
        Console.WriteLine("Filesystem checksum: " + checksum);
        
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var disk = InitDisk(data);
        FileData[] files = [.. data.Files];
        var freeSpaces = data.FreeSpaces.ToList();
        for (var j = files.Length - 1; j >= 0; --j)
        {
            var file = files[j];
            for (int i = 0; i < freeSpaces.Count; ++i)
            {
                var freeSpace = freeSpaces[i];
                if (file.Offset < freeSpace.Offset)
                    break;
                if (freeSpace.Length >= file.Length)
                {
                    Array.Fill(disk, -1, file.Offset, file.Length);
                    Array.Fill(disk, file.ID, freeSpace.Offset, file.Length);
                    var adjacentFreeSpaceIndex = freeSpaces.FindIndex(freeSpace => (freeSpace.Offset + freeSpace.Length) == file.Offset);
                    if (adjacentFreeSpaceIndex >= 0)
                    {
                        var freeSpaceToExpand = freeSpaces[adjacentFreeSpaceIndex];
                        freeSpaceToExpand.Length += file.Length;
                        freeSpaces[adjacentFreeSpaceIndex] = freeSpaceToExpand;
                    }
                    else
                    {
                        freeSpaces.Add(new()
                        {
                            Offset = file.Offset,
                            Length = file.Length
                        });
                    }
                    file.Offset = freeSpace.Offset;
                    files[j] = file;
                    freeSpace.Offset += file.Length;
                    freeSpace.Length -= file.Length;
                    if (freeSpace.Length > 0)
                        freeSpaces[i] = freeSpace;
                    else
                        freeSpaces.RemoveAt(i);
                    break;
                }
            }
        }
        var checksum = CalculateDiskChecksum(disk);
        if (settings.Verbose)
            Console.WriteLine("Disk: " + string.Join(' ', disk.Take(disk.AsSpan().LastIndexOfAnyExcept(-1) + 1)));
        Console.WriteLine("Filesystem checksum: " + checksum);
    }

    private static int[] InitDisk(Data data)
    {
        var disk = new int[data.TotalSpace];
        foreach (var file in data.Files)
            Array.Fill(disk, file.ID, file.Offset, file.Length);
        foreach (var freeSpace in data.FreeSpaces)
            Array.Fill(disk, -1, freeSpace.Offset, freeSpace.Length);
        return disk;
    }
    private static long CalculateDiskChecksum(int[] disk)
        => disk.Select((id, index) => id >= 0 ? (long)(id * index) : 0L).Sum();
}
