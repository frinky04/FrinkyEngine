# FAssetEntry

Namespace: FrinkyEngine.Core.Assets

Represents a single file entry within an [FAssetArchive](./frinkyengine.core.assets.fassetarchive).

```csharp
public class FAssetEntry
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [FAssetEntry](./frinkyengine.core.assets.fassetentry)<br>
Attributes [NullableContextAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullablecontextattribute), [NullableAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.nullableattribute)

## Properties

### **RelativePath**

Path of the file relative to the assets root, using forward slashes.

```csharp
public string RelativePath { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **SourcePath**

Absolute path to the source file on disk (used during archive creation).

```csharp
public string SourcePath { get; set; }
```

#### Property Value

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **DataOffset**

Byte offset of this file's data within the archive.

```csharp
public ulong DataOffset { get; set; }
```

#### Property Value

[UInt64](https://docs.microsoft.com/en-us/dotnet/api/system.uint64)<br>

### **DataSize**

Size of this file's data in bytes.

```csharp
public ulong DataSize { get; set; }
```

#### Property Value

[UInt64](https://docs.microsoft.com/en-us/dotnet/api/system.uint64)<br>

## Constructors

### **FAssetEntry()**

```csharp
public FAssetEntry()
```
