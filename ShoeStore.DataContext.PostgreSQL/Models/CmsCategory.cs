using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class CmsCategory
{
    public int Id { get; set; }

    public int ProfileId { get; set; }

    public int SortOrder { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ItemTagline { get; set; }

    public string? ImageBase64 { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CmsProfile Profile { get; set; } = null!;
}
