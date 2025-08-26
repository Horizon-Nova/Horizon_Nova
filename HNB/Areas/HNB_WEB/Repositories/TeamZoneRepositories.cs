using Microsoft.EntityFrameworkCore;
using Models.Hnbdata;

namespace HNB.Areas.HNB_WEB.Repositories;

public class TeamZoneRepositories(HnbdataDbContext hb)
{
    //#region 統一的查詢來源（僅取上架且發布）
    //private IQueryable<project> ProjectsValid
    //    => hb.Set<project>().Where(p => p.is_active == true && p.status == "published").OrderBy(p => p.sort_order);
    //private IQueryable<project_section> ProjectSectionsValid
    //    => hb.Set<project_section>().Where(s => s.is_active == true);
    //private IQueryable<project_asset> ProjectAssetsValid
    //    => hb.Set<project_asset>().Where(a => a.is_active == true);
    //private IQueryable<project_link> ProjectLinksValid
    //    => hb.Set<project_link>().Where(l => l.is_active == true);
    //#endregion

    //#region Portfolio Page
    //public List<project> Project1Mapping()
    //    => ProjectsValid.ToList();

    //public List<(string category, string category_label)> Project1CategoryLabelsMapping()
    //{
    //    return ProjectsValid
    //        .GroupBy(p => new { p.category, p.category_label })
    //        .AsEnumerable()
    //        .Select(g => (g.Key.category!, g.Key.category_label!))
    //        .ToList();
    //}

    ///// <summary>projects + ListByCategoryMapping：依分類取專案清單（原始模型、已排序）</summary>
    //public List<project> Project1ListByCategoryMapping(string category)
    //    => ProjectsValid
    //        .Where(p => p.category == category)
    //        .OrderBy(p => p.sort_order)
    //        .ThenByDescending(p => p.updated_at)
    //        .ToList();

    //public project? ProjectMapping(string slug)
    //    => ProjectsValid.FirstOrDefault(p => p.slug == slug);

    //public List<project> ProjectsRelatedByCategory(project current, int take = 4)
    //=> ProjectsValid
    //    .Where(p => p.project_id != current.project_id && p.category == current.category)
    //    .OrderBy(p => p.sort_order)
    //    .ThenByDescending(p => p.updated_at)
    //    .Take(take)
    //    .ToList();

    ///// <summary>project_section + List：取得該專案的章節列表（依 sort_order 排序）</summary>

    //public List<project_section> ProjectSectionsList(long project_id)
    //=> ProjectSectionsValid
    //    .Where(s => s.project_id == project_id)
    //    .OrderBy(s => s.sort_order)
    //    .ThenBy(s => s.section_id)
    //    .ToList();

    ///// <summary>取得該專案的章節列表（依 sort_order 排序）</summary>
    //public List<project_asset> ProjectAssetsList(long project_id)
    //=> ProjectAssetsValid
    //    .Where(a => a.project_id == project_id)
    //    .OrderBy(a => a.sort_order)
    //    .ThenBy(a => a.asset_id)
    //    .ToList();

    ///// <summary>project_asset + List：取得該專案的資產（圖片/附件等）列表</summary>
    //public List<project_link> ProjectLinksList(long project_id)
    //=> ProjectLinksValid
    //    .Where(l => l.project_id == project_id)
    //    .OrderBy(l => l.sort_order)
    //    .ThenBy(l => l.link_id)
    //    .ToList();

    //#endregion


}
