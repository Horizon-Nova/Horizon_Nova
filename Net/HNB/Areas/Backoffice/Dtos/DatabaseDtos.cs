using System.ComponentModel.DataAnnotations;

namespace HNB.Areas.Backoffice.Dtos;

/// <summary>
/// 測試連線請求 DTO
/// </summary>
public class TestConnectionRequestDto
{
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// 測試連線回應 DTO
/// </summary>
public class TestConnectionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 資料表詳情請求 DTO
/// </summary>
public class TableDetailsRequestDto
{
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
}

/// <summary>
/// 資料表欄位資訊 DTO
/// </summary>
public class TableColumnDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Length { get; set; }
    public string? DefaultValue { get; set; }
    public string? Constraints { get; set; }
    public string? Description { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }
}


/// <summary>
/// 生成模型請求 DTO
/// </summary>
public class GenerateModelsRequestDto
{
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string ContextName { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
}

/// <summary>
/// 生成模型回應 DTO
/// </summary>
public class GenerateModelsResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

