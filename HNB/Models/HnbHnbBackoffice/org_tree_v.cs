using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class org_tree_v
{
    [StringLength(36)]
    public string? ancestor_id { get; set; }

    [StringLength(50)]
    public string? ancestor_name { get; set; }

    [StringLength(36)]
    public string? descendant_id { get; set; }

    [StringLength(50)]
    public string? descendant_name { get; set; }

    public int? depth { get; set; }

    [StringLength(36)]
    public string? parent_id { get; set; }

    public int? display_order { get; set; }

    [StringLength(50)]
    public string? manager_person_id { get; set; }
}
