using System;
using System.Collections.Generic;

namespace HNB.Models;

/// <summary>
/// 黑名單
/// </summary>
public partial class BlockedIp
{
    public Guid Id { get; set; }

    public string Ip { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
