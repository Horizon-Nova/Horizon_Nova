using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Text;

namespace ManagementSystem.Areas.Backoffice.Services
{
    public class FileManagerServices
    {
        private readonly string _rootPath;
        
        public FileManagerServices(IWebHostEnvironment env)
        {
            _rootPath = Path.Combine(env.WebRootPath, "uploads");
            
            // 確保根目錄存在
            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
        }

        public FileListViewModel BuildFileListVm(string path)
        {
            var fullPath = Path.Combine(_rootPath, path.TrimStart('/'));
            var vm = new FileListViewModel
            {
                CurrentPath = path,
                ParentPath = LoadParentPath(path),
                Items = new List<FileItemViewModel>()
            };

            if (!Directory.Exists(fullPath))
            {
                return vm;
            }

            try
            {
                var dirs = Directory.GetDirectories(fullPath)
                    .Select(d => new FileItemViewModel
                    {
                        Name = Path.GetFileName(d),
                        Type = "folder",
                        Size = 0,
                        Modified = Directory.GetLastWriteTime(d),
                        Path = path.TrimEnd('/') + "/" + Path.GetFileName(d)
                    })
                    .OrderBy(x => x.Name);

                var files = Directory.GetFiles(fullPath)
                    .Select(f => new FileInfo(f))
                    .Select(f => new FileItemViewModel
                    {
                        Name = f.Name,
                        Type = LoadFileType(f.Extension),
                        Size = f.Length,
                        Modified = f.LastWriteTime,
                        Path = path.TrimEnd('/') + "/" + f.Name
                    })
                    .OrderBy(x => x.Name);

                vm.Items = dirs.Concat(files).ToList();
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不拋出異常
                vm.ErrorMessage = ex.Message;
            }

            return vm;
        }

        public List<TreeNodeViewModel> BuildTree(string path)
        {
            var nodes = new List<TreeNodeViewModel>();
            var fullPath = Path.Combine(_rootPath, path.TrimStart('/'));

            if (!Directory.Exists(fullPath))
            {
                return nodes;
            }

            try
            {
                var dirs = Directory.GetDirectories(fullPath);
                foreach (var dir in dirs)
                {
                    var dirName = Path.GetFileName(dir);
                    var node = new TreeNodeViewModel
                    {
                        Id = path.TrimEnd('/') + "/" + dirName,
                        Text = dirName,
                        Type = "folder",
                        Children = BuildTree(path.TrimEnd('/') + "/" + dirName)
                    };
                    nodes.Add(node);
                }
            }
            catch (Exception)
            {
                // 忽略錯誤
            }

            return nodes;
        }

        public string CreateFolder(string path, string name)
        {
            try
            {
                var fullPath = Path.Combine(_rootPath, path.TrimStart('/'), name);
                
                if (Directory.Exists(fullPath))
                {
                    return "資料夾已存在";
                }

                Directory.CreateDirectory(fullPath);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string CreateEmptyFile(string path, string name)
        {
            try
            {
                var fullPath = Path.Combine(_rootPath, path.TrimStart('/'), name);
                
                if (File.Exists(fullPath))
                {
                    return "檔案已存在";
                }

                File.Create(fullPath).Close();
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string SaveUploadedFile(string path, string fileName, IFormFile file)
        {
            try
            {
                var fullPath = Path.Combine(_rootPath, path.TrimStart('/'));
                
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                var filePath = Path.Combine(fullPath, fileName);
                
                using var stream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(stream);
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteFile(string path, string name)
        {
            try
            {
                var fullPath = Path.Combine(_rootPath, path.TrimStart('/'), name);
                
                if (!File.Exists(fullPath))
                {
                    return "檔案不存在";
                }

                File.Delete(fullPath);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteFolder(string path, string name)
        {
            try
            {
                var fullPath = Path.Combine(_rootPath, path.TrimStart('/'), name);
                
                if (!Directory.Exists(fullPath))
                {
                    return "資料夾不存在";
                }

                Directory.Delete(fullPath, true);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string RenameFile(string path, string oldName, string newName)
        {
            try
            {
                var oldPath = Path.Combine(_rootPath, path.TrimStart('/'), oldName);
                var newPath = Path.Combine(_rootPath, path.TrimStart('/'), newName);
                
                if (!File.Exists(oldPath))
                {
                    return "檔案不存在";
                }

                if (File.Exists(newPath))
                {
                    return "目標檔案已存在";
                }

                File.Move(oldPath, newPath);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string RenameFolder(string path, string oldName, string newName)
        {
            try
            {
                var oldPath = Path.Combine(_rootPath, path.TrimStart('/'), oldName);
                var newPath = Path.Combine(_rootPath, path.TrimStart('/'), newName);
                
                if (!Directory.Exists(oldPath))
                {
                    return "資料夾不存在";
                }

                if (Directory.Exists(newPath))
                {
                    return "目標資料夾已存在";
                }

                Directory.Move(oldPath, newPath);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public (string? absolutePath, string? error, string contentType) ResolveFileForDownload(string path, string name)
        {
            try
            {
                var fullPath = Path.Combine(_rootPath, path.TrimStart('/'), name);
                
                if (!File.Exists(fullPath))
                {
                    return (null, "檔案不存在", "application/octet-stream");
                }

                var contentType = LoadContentType(Path.GetExtension(name));
                return (fullPath, null, contentType);
            }
            catch (Exception ex)
            {
                return (null, ex.Message, "application/octet-stream");
            }
        }

        public (string? zipPath, string? error, string downloadName) BuildFolderZip(string path, string name)
        {
            try
            {
                var folderPath = Path.Combine(_rootPath, path.TrimStart('/'), name);
                
                if (!Directory.Exists(folderPath))
                {
                    return (null, "資料夾不存在", "");
                }

                var tempZipPath = Path.GetTempFileName();
                var downloadName = $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";

                ZipFile.CreateFromDirectory(folderPath, tempZipPath);
                
                return (tempZipPath, null, downloadName);
            }
            catch (Exception ex)
            {
                return (null, ex.Message, "");
            }
        }

        public (string? content, string? error) ReadText(string path, string name)
        {
            try
            {
                var fullPath = Path.Combine(_rootPath, path.TrimStart('/'), name);
                
                if (!File.Exists(fullPath))
                {
                    return (null, "檔案不存在");
                }

                var content = File.ReadAllText(fullPath, Encoding.UTF8);
                return (content, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public string WriteText(string path, string name, string content)
        {
            try
            {
                var fullPath = Path.Combine(_rootPath, path.TrimStart('/'), name);
                
                File.WriteAllText(fullPath, content, Encoding.UTF8);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private string LoadParentPath(string path)
        {
            if (path == "/" || string.IsNullOrEmpty(path))
            {
                return "/";
            }

            var parts = path.TrimEnd('/').Split('/');
            if (parts.Length <= 1)
            {
                return "/";
            }

            return string.Join("/", parts.Take(parts.Length - 1));
        }

        private string LoadFileType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "image",
                ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" => "video",
                ".mp3" or ".wav" or ".flac" or ".aac" => "audio",
                ".pdf" => "pdf",
                ".doc" or ".docx" => "document",
                ".xls" or ".xlsx" => "spreadsheet",
                ".txt" or ".log" => "text",
                ".zip" or ".rar" or ".7z" => "archive",
                _ => "file"
            };
        }

        private string LoadContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }

    // View Models
    public class FileListViewModel
    {
        public string CurrentPath { get; set; } = string.Empty;
        public string ParentPath { get; set; } = string.Empty;
        public List<FileItemViewModel> Items { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class FileItemViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public string Path { get; set; } = string.Empty;
    }

    public class TreeNodeViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<TreeNodeViewModel> Children { get; set; } = new();
    }
}
