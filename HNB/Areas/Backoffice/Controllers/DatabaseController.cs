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
        return View();
    }

    [HttpPost]
    public IActionResult TestConnection(TestConnectionRequestDto request)
    {
        var result = databaseService.TestConnection(request.Provider, request.ConnectionString);
        var response = new TestConnectionResponseDto
        {
            Success = result.Success,
            Message = result.Message
        };

        return Json(response);
    }

    [HttpPost]
    public IActionResult LoadDatabaseTables(TestConnectionRequestDto request)
    {
        var result = databaseService.LoadDatabaseTables(request.Provider, request.ConnectionString);
        return Json(new { success = result.Success, tables = result.Tables, message = result.Message });
    }


    [HttpPost]
    public IActionResult BackupTables(GenerateModelsRequestDto request)
    {
        var result = databaseService.BackupDatabaseTables(request);

        return Json(new GenerateModelsResponseDto
        {
            Success = result.Success,
            Message = result.Message
        });
    }




    /// <summary>
    /// 獲取資料表詳情部分視圖
    /// </summary>
    [HttpPost]
    public IActionResult LoadTableDetailsPartial([FromBody] TableDetailsRequestDto request)
    {
        if (request == null || string.IsNullOrEmpty(request.Provider) ||
            string.IsNullOrEmpty(request.ConnectionString) || string.IsNullOrEmpty(request.TableName))
        {
            return PartialView("_TableDetailsModal", new List<TableColumnDto>());
        }

        var result = databaseService.LoadTableDetails(request.Provider, request.ConnectionString, request.TableName);

        if (result.Success && result.Columns != null && result.Columns.Any())
        {
            return PartialView("_TableDetailsModal", result.Columns);
        }
        else
        {
            return PartialView("_TableDetailsModal", new List<TableColumnDto>());
        }
    }

}
