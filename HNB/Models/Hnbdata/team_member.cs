using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Hnbdata;

[Table("team_member", Schema = "dbo")]
public partial class team_member
{
    [Key]
    public int id { get; set; }

    [StringLength(100)]
    public string name { get; set; } = null!;

    [StringLength(100)]
    public string role { get; set; } = null!;

    [StringLength(255)]
    public string? avatar_url { get; set; }

    [StringLength(4)]
    public string? avatar_text { get; set; }

    public string? bio { get; set; }

    public List<string>? skills { get; set; }

    [StringLength(7)]
    public string? color { get; set; }

    [StringLength(255)]
    public string? github { get; set; }

    [StringLength(255)]
    public string? linkedin { get; set; }

    [StringLength(255)]
    public string? twitter { get; set; }

    public bool? is_active { get; set; }
}
