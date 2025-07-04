using System;
using System.Collections.Generic;

namespace HNB.Models;

public partial class AccessRecord
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string? Roles { get; set; }

    public string RequestPath { get; set; } = null!;

    public string? Ip { get; set; }

    public string Result { get; set; } = null!;

    public string? UserAgent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? LogType { get; set; }

    public string? HttpMethod { get; set; }

    public string? RequestBody { get; set; }

    public string? ResponseBody { get; set; }

    public int? StatusCode { get; set; }

    public double? DurationMs { get; set; }
}
