using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
[Table("vw_permission_organization", Schema = "dbo")]
public partial class vw_permission_organization
{
    [Column("id")]
    public int? id { get; set; }

    [Column("name")]
    public string? name { get; set; }

    [Column("description")]
    public string? description { get; set; }

    [Column("level")]
    public int? level { get; set; }

    [Column("sort_order")]
    public int? sort_order { get; set; }

    [Column("is_active")]
    public bool? is_active { get; set; }

    [Column("status")]
    public string? status { get; set; }

    [Column("created_at")]
    public DateTime? created_at { get; set; }

    [Column("updated_at")]
    public DateTime? updated_at { get; set; }

    [Column("tags")]
    public string[]? tags { get; set; }

    [Column("notes")]
    public string? notes { get; set; }

    [Column("parent_organization_name")]
    public string? parent_organization_name { get; set; }

    [Column("parent_name")]
    public string? parent_name { get; set; }

    [Column("parent_id")]
    public int? parent_id { get; set; }

    [Column("path")]
    public int[]? path { get; set; }

    [Column("hierarchy_level")]
    public int? hierarchy_level { get; set; }

    [Column("full_path_name")]
    public string? full_path_name { get; set; }

    [Column("user_count")]
    public int? user_count { get; set; }

    [Column("child_count")]
    public int? child_count { get; set; }

    [Column("role_count")]
    public int? role_count { get; set; }

    [Column("child_organization_count")]
    public int? child_organization_count { get; set; }

    [Column("user_names")]
    public string[]? user_names { get; set; }

    [Column("role_names")]
    public string[]? role_names { get; set; }
}
