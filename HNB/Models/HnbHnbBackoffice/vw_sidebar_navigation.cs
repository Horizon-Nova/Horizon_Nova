using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class vw_sidebar_navigation
{
    public int? id { get; set; }

    [StringLength(50)]
    public string? code { get; set; }

    [StringLength(100)]
    public string? title { get; set; }

    [StringLength(500)]
    public string? url { get; set; }

    [StringLength(50)]
    public string? icon { get; set; }

    [StringLength(50)]
    public string? parent_code { get; set; }

    public int? sort_order { get; set; }

    public bool? is_active { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }
}
