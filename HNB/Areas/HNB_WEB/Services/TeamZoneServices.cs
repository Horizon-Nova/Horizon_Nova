using HNB.Areas.HNB_WEB.Repositories;
using System.Text.Json;
using Models.Hnbdata;

namespace HNB.Areas.HNB_WEB.Services;

public class TeamZoneServices(TeamZoneRepositories res)
{

    //#region Portfolio

    ///// <summary>作品清單頁</summary>
    //public void ViewBagModelPortfolio(dynamic viewBag)
    //{
    //    var categories = res.Project1CategoryLabelsMapping();
    //    viewBag.ProjectCategories = categories; // List<(category, category_label)>

    //    var sections = new Dictionary<string, List<project>>(StringComparer.OrdinalIgnoreCase);
    //    foreach (var (category, _) in categories)
    //        sections[category] = res.Project1ListByCategoryMapping(category);
    //    viewBag.PortfolioSections = sections;

    //    var techMap = new Dictionary<long, List<string>>();
    //    var featMap = new Dictionary<long, List<string>>();
    //    var highlightMap = new Dictionary<long, List<string>>();

    //    foreach (var list in sections.Values)
    //    {
    //        foreach (var p in list)
    //        {
    //            if (!techMap.ContainsKey(p.project_id))
    //                techMap[p.project_id] = ToList(p.tech_stack_json);

    //            if (!featMap.ContainsKey(p.project_id))
    //                featMap[p.project_id] = ToList(p.features_json);

    //            // 如果有亮點欄位：highlight_json
    //            // if (!highlightMap.ContainsKey(p.project_id))
    //            //     highlightMap[p.project_id] = ToList(p.highlights_json);
    //        }
    //    }

    //    viewBag.ProjectTechListMap = techMap;       // IDictionary<long, List<string>>
    //    viewBag.ProjectFeatListMap = featMap;       // IDictionary<long, List<string>>
    //    viewBag.ProjectHighlightListMap = highlightMap; // 可選
    //}

    //#endregion

    //#region ProjectDetail 

    ///// <summary>ProjectDetail ViewBag 控管</summary>
    ///// <summary>專案詳細頁 ViewBag</summary>
    //public void ViewBagModelProjectDetail(dynamic viewBag, string slug)
    //{
    //    var project = res.ProjectMapping(slug);
    //    if (project == null)
    //    {
    //        viewBag.NotFound = true;
    //        return;
    //    }

    //    viewBag.Project = project;
    //    viewBag.ProjectCategoryLabel = project.category_label;

    //    // sections
    //    viewBag.ProjectSections = res.ProjectSectionsList(project.project_id);

    //    // assets (圖片 / 附件)
    //    viewBag.ProjectAssets = res.ProjectAssetsList(project.project_id);

    //    // links (外部連結)
    //    viewBag.ProjectLinks = res.ProjectLinksList(project.project_id);

    //    viewBag.ProjectRelated = res.ProjectsRelatedByCategory(project);

    //    viewBag.ProjectTechList = ToList(project.tech_stack_json);
    //    viewBag.ProjectFeatureList = ToList(project.features_json);
    //    // viewBag.ProjectHighlightList = ToList(project.highlights_json);
    //}

    //#endregion

    //#region 私有工具
    //private static List<string> ToList(string? json)
    //{
    //    if (string.IsNullOrWhiteSpace(json)) return new();
    //    try { return JsonSerializer.Deserialize<List<string>>(json!) ?? new(); }
    //    catch { return new(); }
    //}
    //#endregion

}
