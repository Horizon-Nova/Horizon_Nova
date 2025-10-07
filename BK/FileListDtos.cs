namespace ManagementSystem.Areas.Backoffice.Dtos;

public enum EntryKind { Folder, File }

public sealed class FileEntryDto
{
    public EntryKind Kind { get; init; }                 // Folder / File
    public string Name { get; init; } = "";
    public string VirtualPath { get; init; } = "/";      // 所在虛擬路徑
    public long? Size { get; init; }                     // File 用
    public DateTime? LastWriteUtc { get; init; }
}

public sealed class FileListVm
{
    public string Path { get; init; } = "/";
    public IReadOnlyList<FileEntryDto> Items { get; init; } = Array.Empty<FileEntryDto>();
}
