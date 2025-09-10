using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.HnbHnbBackoffice;

[Keyless]
public partial class person_relation_v
{
    public long? person_id { get; set; }

    [StringLength(100)]
    public string? person_name { get; set; }

    [StringLength(100)]
    public string? person_dept_id { get; set; }

    public long? related_person_id { get; set; }

    [StringLength(100)]
    public string? related_person_name { get; set; }

    [StringLength(100)]
    public string? related_dept_id { get; set; }

    public int? distance { get; set; }

    public string? relation { get; set; }
}
