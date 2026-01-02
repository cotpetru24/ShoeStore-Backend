using Microsoft.AspNetCore.Identity;

namespace ShoeStore.DataContext.PostgreSQL.Models
{
    public partial class UserDetail
    {
        public string AspNetUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsHidden { get; set; }
        public bool IsBlocked { get; set; }

        public virtual IdentityUser AspNetUser { get; set; }
    }
}
