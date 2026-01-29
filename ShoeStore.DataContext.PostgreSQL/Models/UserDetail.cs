using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class UserDetail
{
    public string AspNetUserId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsHidden { get; set; }

    public bool IsBlocked { get; set; }

    public virtual AspNetUser AspNetUser { get; set; } = null!;
}
