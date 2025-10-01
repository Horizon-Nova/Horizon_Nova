using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_permission_role
{
    public int? id { get; set; }

    [StringLength(50)]
    public string? type { get; set; }

    [StringLength(100)]
    public string? name { get; set; }

    [StringLength(500)]
    public string? description { get; set; }

    public List<string>? permissions { get; set; }

    public bool? is_active { get; set; }

    [StringLength(50)]
    public string? status { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? created_at { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? updated_at { get; set; }

    public List<string>? tags { get; set; }

    public string? notes { get; set; }

    public int? parent_id { get; set; }

    public int? sort_order { get; set; }

    public int? level { get; set; }

    public int? created_by { get; set; }

    public int? updated_by { get; set; }

    public string? internal_notes { get; set; }

    [StringLength(100)]
    public string? organization_name { get; set; }

    public long? user_count { get; set; }

    public long? permission_count { get; set; }

    public List<string>? user_names { get; set; }
}
