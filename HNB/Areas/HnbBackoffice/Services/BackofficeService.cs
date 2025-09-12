using HNB.Areas.HnbBackoffice.Dtos;
using HNB.Areas.HnbBackoffice.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Runtime.Intrinsics.Arm;
using static DirectoryManagerUtilities;

namespace HNB.Areas.HnbBackoffice.Services;

public class BackofficeService(DirectoryManagerUtilities dm)
{
    #region File Manager (檔案總管)
    /// <summary> 查詢目錄樹（由指定虛擬路徑起算）</summary>
    public List<FileTreeNodeDto> BuildTree(string startVirtualPath = "/")
        => dm.BuildTree(startVirtualPath);

    /// <summary>建立檔案清單 VM（不丟例外）</summary>
    public FileListVm BuildFileListVm(string path)
    {
        var v = dm.NormalizePath(path);
        var folders = dm.ListFolders(v).Select(x => new FileEntryDto
        {
            Kind = EntryKind.Folder,
            Name = x.Name,
            VirtualPath = v,
            LastWriteUtc = x.LastWriteUtc
        });
        var files = dm.ListFiles(v).Select(x => new FileEntryDto
        {
            Kind = EntryKind.File,
            Name = x.Name,
            VirtualPath = v,
            Size = x.Size,
            LastWriteUtc = x.LastWriteUtc
        });

        var items = folders.Concat(files)
                           .OrderBy(i => i.Kind == EntryKind.File)
                           .ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                           .ToList();

        return new FileListVm { Path = v, Items = items };
    }

    public string SaveUploadedFile(string destVirtualPath, string relKey, IFormFile file)
        => dm.SaveFormFileAt(destVirtualPath, relKey, file);

    /// <summary> 建立資料夾 </summary>
    public string CreateFolder(string virtualPath, string folderName) => dm.CreateDirectoryAt(virtualPath, folderName);

    /// <summary> 建立空白檔 </summary>
    public string CreateEmptyFile(string virtualPath, string fileName) => dm.CreateFileAt(virtualPath, fileName);

    /// <summary> 刪除檔案 </summary>
    public string DeleteFile(string virtualPath, string nameOrRel) => dm.DeleteFileAt(virtualPath, nameOrRel);

    /// <summary> 刪除資料夾（含內容） </summary>
    public string DeleteFolder(string virtualPath, string nameOrRel) => dm.DeleteDirectoryAt(virtualPath, nameOrRel, true);

    /// <summary> 檔案重新命名 </summary>
    public string RenameFile(string virtualPath, string oldName, string newName) => dm.RenameFileAt(virtualPath, oldName, newName);
    /// <summary> 資料夾重新命名 </summary>
    public string RenameFolder(string virtualPath, string oldName, string newName) => dm.RenameDirectoryAt(virtualPath, oldName, newName);

    /// <summary> 下載檔案 </summary>
    public (string? absPath, string? err, string contentType) ResolveFileForDownload(string path, string name)
    {
        var (abs, err) = dm.GetSafeFilePath(path, name);
        var ct = dm.GetContentType(name);
        return (abs, err, ct);
    }

    /// <summary> 下載資料夾 </summary>
    public (string? zipAbs, string? err, string downloadName) BuildFolderZip(string path, string folderName)
        => dm.CreateZipOfDirectory(path, folderName);

    /// <summary> 讀取文字檔內容 </summary>
    public (string? text, string? err) ReadText(string vpath, string file) => dm.ReadTextAt(vpath, file);
    /// <summary> 寫入文字檔內容（覆寫） </summary>
    public string WriteText(string vpath, string file, string content) => dm.WriteTextAt(vpath, file, content);


    #endregion
}

