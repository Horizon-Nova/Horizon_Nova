using System;
using System.Collections.Generic;

namespace HNB.Models;

public partial class ErrorLog
{
    public int Id { get; set; }

    public string Function { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string? StackTrace { get; set; }

    public DateTime? CreatedAt { get; set; }
}
