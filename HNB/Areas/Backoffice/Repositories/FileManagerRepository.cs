using Models.HnbHnbBackoffice;

namespace HNB.Areas.Backoffice.Repositories;

public class FileManagerRepository(HnbHnbBackofficeDbContext db)
{
    #region 統一的查詢來源

    private IQueryable<vw_file_manager> ValidFiles
        => db.vw_file_managers.Where(f => f.is_deleted == false);

    private IQueryable<file_manager> ValidFileManagers
        => db.file_managers.Where(f => f.is_deleted == false);

    #endregion

    #region 專用查詢方法

    public List<vw_file_manager> QueryFileList(string? username = null, string? parentCode = null)
        => ValidFiles
            .Where(f => (username == null 
                      || f.owner_username == username 
                      || f.shared_users!.Contains("ALL")
                      || f.shared_users!.Contains(username))
                     && f.parent_code == parentCode)
            .OrderByDescending(f => f.item_type)
            .ThenBy(f => f.file_name)
            .ToList();

    public List<vw_file_manager> QueryAllFolders(string? username = null)
        => ValidFiles
            .Where(f => f.item_type == "folder"
                     && (username == null 
                      || f.owner_username == username 
                      || f.shared_users!.Contains("ALL")
                      || f.shared_users!.Contains(username)))
            .OrderBy(f => f.parent_code ?? "")
            .ThenBy(f => f.file_name)
            .ToList();

    public Dictionary<string, string> QueryAllFilePaths()
        => ValidFiles
            .Where(f => !string.IsNullOrEmpty(f.file_path))
            .ToDictionary(f => f.file_path!, f => f.code!);

    public vw_file_manager? QueryFile(long? id = null, string? code = null)
    {
        if (id.HasValue)
            return ValidFiles.FirstOrDefault(f => f.id == id.Value);

        if (!string.IsNullOrEmpty(code))
            return ValidFiles.FirstOrDefault(f => f.code == code);

        return null;
    }

    public List<file_manager> QueryFileManagementList(string? username = null, string? itemType = null)
        => ValidFileManagers
            .Where(f => (username == null || f.owner_username == username)
                     && (itemType == null || f.item_type == itemType))
            .ToList();

    public file_manager? QueryFileManagement(long? id = null, string? code = null)
    {
        if (id.HasValue)
            return ValidFileManagers.FirstOrDefault(f => f.id == id.Value);

        if (!string.IsNullOrEmpty(code))
            return ValidFileManagers.FirstOrDefault(f => f.code == code);

        return null;
    }

    #endregion

    #region 基本 CRUD 操作

    public file_manager? InsertFile(file_manager data)
    {
#if DEBUG
        return null;
#else
        var existingEntity = db.file_managers.Find(data.id);

        if (existingEntity == null)
        {
            data.created_at = DateTime.Now;
            db.file_managers.Add(data);
            db.SaveChanges();
            return data;
        }

        existingEntity.file_name = data.file_name;
        existingEntity.file_path = data.file_path;
        existingEntity.shared_users = data.shared_users;
        existingEntity.file_size = data.file_size;
        existingEntity.parent_code = data.parent_code;
        existingEntity.mime_type = data.mime_type;
        existingEntity.updated_at = DateTime.Now;

        db.SaveChanges();
        return existingEntity;
#endif
    }

    public bool DeleteFile(long id)
    {
        var entity = db.file_managers.Find(id);
        if (entity != null)
        {
            entity.is_deleted = true;
            entity.deleted_at = DateTime.Now;
            db.SaveChanges();
            return true;
        }
        return false;
    }

    #endregion
}

