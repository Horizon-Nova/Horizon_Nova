using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

[Table("blocked_ips", Schema = "dbo")]
public partial class blocked_ip
{
    [Key]
    public Guid id { get; set; }

    public string ip { get; set; } = null!;

    public string? reason { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? expires_at { get; set; }
}
