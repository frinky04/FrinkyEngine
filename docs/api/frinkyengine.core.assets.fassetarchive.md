# FAssetArchive

Namespace: FrinkyEngine.Core.Assets

Packs and extracts binary asset archives in the `.fasset` format.
 The archive stores a header, a file table, and concatenated file data.

```csharp
public static class FAssetArchive
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FAssetArchive](./frinkyengine.core.assets.fassetarchive)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Methods

### **Write(String, IReadOnlyList&lt;FAssetEntry&gt;)**

Writes a set of asset entries into a single archive file.

```csharp
public static void Write(string outputPath, IReadOnlyList<FAssetEntry> entries)
```

#### Parameters

`outputPath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Destination path for the archive.

`entries` [IReadOnlyList&lt;FAssetEntry&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1)<br>
The files to pack. Each entry's [FAssetEntry.SourcePath](./frinkyengine.core.assets.fassetentry#sourcepath) must point to an existing file.

### **ExtractAll(String, String)**

Extracts all files from an archive to the specified output directory, recreating subdirectories.

```csharp
public static void ExtractAll(string archivePath, string outputDirectory)
```

#### Parameters

`archivePath` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Path to the `.fasset` archive.

`outputDirectory` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>
Directory to extract files into.
