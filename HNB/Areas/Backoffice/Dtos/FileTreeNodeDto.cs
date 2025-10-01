using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HNB.Areas.Backoffice.Dtos;

public sealed class FileTreeNodeDto
{
    public string Name { get; init; } = "";
    public string VirtualPath { get; init; } = "/";
    public int Depth { get; init; }
}
