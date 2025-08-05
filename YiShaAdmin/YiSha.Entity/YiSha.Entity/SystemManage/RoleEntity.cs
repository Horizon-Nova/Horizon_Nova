using System.ComponentModel.DataAnnotations.Schema;

namespace YiSha.Entity.SystemManage
{
    [Table("SysRole")]
    public class RoleEntity : BaseExtensionEntity
    {
        public string RoleName { get; set; }
        public int? RoleSort { get; set; }
        public int? RoleStatus { get; set; }
        public string Remark { get; set; }

        /// 角色對應的選單，頁面和按鈕
        /// </summary>
        [NotMapped]
        public string MenuIds { get; set; }

    }
}
