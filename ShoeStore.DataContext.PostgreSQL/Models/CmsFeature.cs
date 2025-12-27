using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class CmsFeature
{
    public int Id { get; set; }

    public int ProfileId { get; set; }

    public int SortOrder { get; set; }

    public string? IconClass { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CmsProfile Profile { get; set; } = null!;
}
