using Models.HnbWeb;

namespace HNB.Areas.HNB_WEB.Repositories;

/// <summary>
/// 首頁資料存取層，僅負責資料查詢與轉換
/// </summary>
public class NovaHomeRepository
{
    #region 統一的查詢來源

    private static IReadOnlyList<NovaHomeShowcaseServiceItem> ValidShowcaseServices => BuildShowcaseServices();

    #endregion

    #region 專用查詢方法

    /// <summary>
    /// 查詢首頁展示區服務列表
    /// </summary>
    public List<NovaHomeShowcaseServiceItem> QueryShowcaseServiceList() => ValidShowcaseServices.ToList();

    #endregion

    private static IReadOnlyList<NovaHomeShowcaseServiceItem> BuildShowcaseServices() =>
    [
        new NovaHomeShowcaseServiceItem
        {
            Key = "ai",
            Accent = "#6366F1",
            Icon = "sparkles",
            Eyebrow = "AI / 資料智能",
            Title = "讓 AI 變成可維運的功能",
            Desc = "把物件辨識、智慧擷取、向量搜尋與監控告警整合進系統，重點放在可部署、可追蹤、可長期迭代。",
            UseCases = ["智慧查表：物件偵測與數字辨識，自動擷取並回填資料", "AI 叢集/伺服器監控：效能追蹤、資源分析、異常預警", "向量搜尋整合：資料向量化與相似度查詢（依需求）"],
            Deliverables = ["可部署服務（API/後台）與權限控管、操作日誌", "儀表板與告警：指標、事件留存、追溯能力", "模型/資料管線整合：版本管理、監控與迭代建議"],
            Tags = ["物件辨識", "向量搜尋", "監控告警", "儀表板", "模型整合"],
            Cta = "我想導入 AI"
        },
        new NovaHomeShowcaseServiceItem
        {
            Key = "tag",
            Accent = "#10B981",
            Icon = "scan-line",
            Eyebrow = "AprilTag / IoT",
            Title = "用 AprilTag 讓現場更快更準",
            Desc = "從快速識別、盤點與定位，到後台串接與追溯，將現場作業流程做成可管理、可量化的系統。",
            UseCases = ["TAGV：AprilTag 快速識別與動態資料申請", "倉儲盤點：盤點/定位/地圖可視化與作業流程整合", "與後台串接：事件、報表、權限與追溯"],
            Deliverables = ["識別/盤點流程設計與串接（APP/系統）", "後台管理：標籤/任務/報表與權限（依需求）", "可視化與追溯：地圖、紀錄、事件留存"],
            Tags = ["AprilTag", "盤點", "定位", "可視化", "串接"],
            Cta = "我想導入 AprilTag"
        },
        new NovaHomeShowcaseServiceItem
        {
            Key = "app",
            Accent = "#D946EF",
            Icon = "smartphone",
            Eyebrow = "行動 APP",
            Title = "把流程帶到現場，順利上線",
            Desc = "支援跨平台與裝置整合，將掃描/辨識/資料回填等流程做成順手的行動工具，並規劃穩定迭代與上架流程。",
            UseCases = ["識別與掃描：AprilTag 快速掃描、資料申請與顯示", "AI 智慧擷取：儀表讀值、自動擷取與檢核", "與後台整合：登入/權限、資料同步、操作紀錄"],
            Deliverables = ["APP 與後端串接：權限、版本、資料流程", "上線與維運：錯誤追蹤、效能觀測、迭代規劃", "部署/上架支援（依專案需求）"],
            Tags = ["Flutter", "React Native", "OpenCV", "TensorFlow", "上架"],
            Cta = "我想做行動 APP"
        },
        new NovaHomeShowcaseServiceItem
        {
            Key = "web",
            Accent = "#0EA5E9",
            Icon = "layout-dashboard",
            Eyebrow = "Web / 企業系統",
            Title = "把業務做成可管理的後台",
            Desc = "從簽核與文書、權限管理、檔案管理到儀表板與報表匯出，建立可交接、可維運的企業系統。",
            UseCases = ["簽核與文書：表單、簽核流程、公告與權限", "企業後台：使用者/角色/組織、導航與檔案管理", "報表與匯出：Excel/Word 產出與通知整合"],
            Deliverables = ["系統架構與權限模型、文件與交付說明", "後台 UI / API / 資料庫設計與維護策略", "匯出與通知：範本、Email、記錄與審計"],
            Tags = [".NET", "Razor", "PostgreSQL", "權限", "報表"],
            Cta = "我想做企業系統"
        },
        new NovaHomeShowcaseServiceItem
        {
            Key = "ops",
            Accent = "#F59E0B",
            Icon = "server",
            Eyebrow = "部署 / 維運",
            Title = "讓系統上線後穩定可控",
            Desc = "把部署流程、站台管理、健康檢查與資源監控做成一套可視化機制，讓維運可追蹤、可回溯、可交接。",
            UseCases = ["站台管理：建立/啟動/停止/刪除與狀態查詢", "持續部署：部署狀態追蹤、健康測試與重試機制", "監控與日誌：CPU/記憶體/網路/磁碟與錯誤統計"],
            Deliverables = ["部署流程與健康檢查：步驟、結果回報與記錄", "監控面板：趨勢圖表、統計與事件追溯", "錯誤日誌：多維度查詢、匯出與維運建議"],
            Tags = ["CD", "監控", "日誌", "健康檢查", "可觀測性"],
            Cta = "我想強化部署維運"
        },
        new NovaHomeShowcaseServiceItem
        {
            Key = "inspect",
            Accent = "#64748B",
            Icon = "ruler",
            Eyebrow = "量測 / 檢測整合",
            Title = "把檢測結果串成可交付流程",
            Desc = "串接外部量測/檢測軟體與設備，整理結果輸出、授權管理與日誌追蹤，讓流程可交接、可維護。",
            UseCases = ["結果串接：監聽輸入檔案並搬運到目標結果檔", "授權工具：機台指紋與金鑰管理、一鍵複製", "設備維護：韌體更新工具與操作流程文件"],
            Deliverables = ["工具程式與安裝/操作手冊（提供客戶）", "串接規格：輸入/輸出格式、錯誤處理與日誌", "維護機制：授權、更新流程與問題排查指引"],
            Tags = ["串接", "授權", "日誌", "工具", "交接文件"],
            Cta = "我想整合檢測流程"
        }
    ];
}
