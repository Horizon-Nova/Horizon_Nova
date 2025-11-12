using HNB.Areas.Backoffice.Dtos;
using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class DatabaseController(DatabaseService databaseService) : BaseController
{
    public IActionResult Index()
        => View();

    [HttpPost]
    public IActionResult TestConnection(TestConnectionRequestDto request)
    {
        var result = databaseService.TestConnection(request.Provider, request.ConnectionString);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public IActionResult LoadDatabaseTables(TestConnectionRequestDto request)
    {
        var result = databaseService.LoadDatabaseTables(request.Provider, request.ConnectionString);
        return Json(new { success = result.Success, tables = result.Tables, message = result.Message });
    }

    [HttpPost]
    public IActionResult SubmitBackup(GenerateModelsRequestDto request)
    {
        var result = databaseService.BackupDatabaseTables(request);
        return Json(new { success = result.Success, message = result.Message });
    }

    public IActionResult LoadDetail(string? tableName = null, string? provider = null, string? connectionString = null)
    {
        var currentTableName = tableName ?? string.Empty;
        var currentProvider = provider ?? string.Empty;
        var currentConnectionString = connectionString ?? string.Empty;

        var result = databaseService.LoadTableDetails(currentProvider, currentConnectionString, currentTableName);

        ViewBag.TableName = currentTableName;
        ViewBag.TableColumns = result.Columns ?? new List<TableColumnDto>();
        ViewBag.Success = result.Success;

        return PartialView("Partials/Database/Modal/_TableDetail");
    }

}
