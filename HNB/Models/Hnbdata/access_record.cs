using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

[Table("access_records", Schema = "dbo")]
public partial class access_record
{
    [Key]
    public Guid id { get; set; }

    public string user_name { get; set; } = null!;

    public string? roles { get; set; }

    public string request_path { get; set; } = null!;

    public string? ip { get; set; }

    public string result { get; set; } = null!;

    public string? user_agent { get; set; }

    public DateTime? created_at { get; set; }

    public string? log_type { get; set; }

    public string? http_method { get; set; }

    public string? request_body { get; set; }

    public string? response_body { get; set; }

    public int? status_code { get; set; }

    public double? duration_ms { get; set; }
}
