namespace FrinkyEngine.Core.Assets;

public class FAssetEntry
{
    public string RelativePath { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public ulong DataOffset { get; set; }
    public ulong DataSize { get; set; }
}

public static class FAssetArchive
{
    private static readonly byte[] Magic = "FARC"u8.ToArray();
    private const uint Version = 1;

    public static void Write(string outputPath, IReadOnlyList<FAssetEntry> entries)
    {
        using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(fs);

        // Header
        writer.Write(Magic);
        writer.Write(Version);
        writer.Write((uint)entries.Count);

        // We need to compute offsets after writing the table, so first calculate table size
        long tableSize = 0;
        foreach (var entry in entries)
        {
            var pathBytes = System.Text.Encoding.UTF8.GetBytes(entry.RelativePath);
            tableSize += 4 + pathBytes.Length + 8 + 8; // pathLen + path + offset + size
        }

        long headerSize = 4 + 4 + 4; // magic + version + fileCount
        long dataStart = headerSize + tableSize;

        // Build offset table
        var offsets = new ulong[entries.Count];
        ulong currentOffset = (ulong)dataStart;
        for (int i = 0; i < entries.Count; i++)
        {
            var fileInfo = new FileInfo(entries[i].SourcePath);
            offsets[i] = currentOffset;
            entries[i].DataOffset = currentOffset;
            entries[i].DataSize = (ulong)fileInfo.Length;
            currentOffset += (ulong)fileInfo.Length;
        }

        // Write table
        for (int i = 0; i < entries.Count; i++)
        {
            var pathBytes = System.Text.Encoding.UTF8.GetBytes(entries[i].RelativePath);
            writer.Write((uint)pathBytes.Length);
            writer.Write(pathBytes);
            writer.Write(offsets[i]);
            writer.Write(entries[i].DataSize);
        }

        // Write data
        var buffer = new byte[81920];
        foreach (var entry in entries)
        {
            using var input = File.OpenRead(entry.SourcePath);
            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                writer.Write(buffer, 0, bytesRead);
        }
    }

    public static void ExtractAll(string archivePath, string outputDirectory)
    {
        using var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fs);

        // Read header
        var magic = reader.ReadBytes(4);
        if (magic[0] != Magic[0] || magic[1] != Magic[1] || magic[2] != Magic[2] || magic[3] != Magic[3])
            throw new InvalidDataException("Not a valid .fasset archive (bad magic).");

        var version = reader.ReadUInt32();
        if (version != Version)
            throw new InvalidDataException($"Unsupported .fasset version: {version}");

        var fileCount = reader.ReadUInt32();

        // Read table
        var entries = new FAssetEntry[fileCount];
        for (uint i = 0; i < fileCount; i++)
        {
            var pathLen = reader.ReadUInt32();
            var pathBytes = reader.ReadBytes((int)pathLen);
            var relativePath = System.Text.Encoding.UTF8.GetString(pathBytes);
            var offset = reader.ReadUInt64();
            var size = reader.ReadUInt64();

            entries[i] = new FAssetEntry
            {
                RelativePath = relativePath,
                DataOffset = offset,
                DataSize = size
            };
        }

        // Extract files
        var buffer = new byte[81920];
        foreach (var entry in entries)
        {
            var outputPath = Path.Combine(outputDirectory, entry.RelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            fs.Position = (long)entry.DataOffset;
            using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            var remaining = (long)entry.DataSize;
            while (remaining > 0)
            {
                var toRead = (int)Math.Min(remaining, buffer.Length);
                var bytesRead = fs.Read(buffer, 0, toRead);
                if (bytesRead == 0) break;
                output.Write(buffer, 0, bytesRead);
                remaining -= bytesRead;
            }
        }
    }
}
