namespace ManagementSystem.Utilities
{
    public class DirectoryManagerUtilities
    {
        // 目錄管理相關的工具方法
        public bool CreateDirectoryIfNotExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public long GetDirectorySize(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return 0;

                return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                    .Sum(file => new FileInfo(file).Length);
            }
            catch
            {
                return 0;
            }
        }
    }
}
