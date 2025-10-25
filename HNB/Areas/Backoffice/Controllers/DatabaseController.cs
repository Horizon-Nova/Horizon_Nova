using HNB.Areas.Backoffice.Dtos;
using HNB.Areas.Backoffice.Filters;
using HNB.Areas.Backoffice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNB.Areas.Backoffice.Controllers;

[Area("Backoffice")]
public class DatabaseController(DatabaseService databaseService) : BaseController
{
    public IActionResult DatabaseManagement()
    {
        return View("DatabaseManagement");
    }

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

    [HttpGet]
    public IActionResult LoadDetail(string? tableName = null, string? provider = null, string? connectionString = null)
    {
        ViewBag.TableName = tableName;
        
        if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(provider) && !string.IsNullOrEmpty(connectionString))
        {
            var result = databaseService.LoadTableDetails(provider, connectionString, tableName);
            ViewBag.TableColumns = result.Columns ?? new List<TableColumnDto>();
            ViewBag.Success = result.Success;
        }
        else
        {
            ViewBag.TableColumns = new List<TableColumnDto>();
            ViewBag.Success = false;
        }
        
        return PartialView("_DatabaseManagementModal");
    }

}
