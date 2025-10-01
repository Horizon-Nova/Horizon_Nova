using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_permission_organization
{
    public int? id { get; set; }

    [StringLength(50)]
    public string? type { get; set; }

    [StringLength(100)]
    public string? name { get; set; }

    [StringLength(500)]
    public string? description { get; set; }

    public int? level { get; set; }

    public int? sort_order { get; set; }

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

    public int? created_by { get; set; }

    public int? updated_by { get; set; }

    public string? internal_notes { get; set; }

    [StringLength(100)]
    public string? parent_organization_name { get; set; }

    [StringLength(100)]
    public string? parent_name { get; set; }

    public long? user_count { get; set; }

    public long? role_count { get; set; }

    public long? child_organization_count { get; set; }

    public long? child_count { get; set; }

    public List<string>? user_names { get; set; }

    public List<string>? role_names { get; set; }

    public int? hierarchy_level { get; set; }

    public List<int>? path { get; set; }

    public string? full_path_name { get; set; }
}
