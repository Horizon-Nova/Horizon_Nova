using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class department_closure_v
{
    [StringLength(36)]
    public string? ancestor_id { get; set; }

    [StringLength(36)]
    public string? descendant_id { get; set; }

    public int? depth { get; set; }
}
