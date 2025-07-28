using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}





//public class AccountController
//{

//    private readonly UserManager<IdentityUser> _userManager;
//    private readonly RoleManager<IdentityRole> _roleManager;

//    public AccountController(UserManager<IdentityUser> userManager,
//                             RoleManager<IdentityRole> roleManager)
//    {
//        _userManager = userManager;
//        _roleManager = roleManager;
//    }

//}
//}
//You can then call:


//await _userManager.AddToRoleAsync(user, "Admin");
//await _userManager.AddClaimAsync(user, new Claim("Permission", "CanView"));

//3.Can I extend Identity tables?
//Yes. To add custom attributes (e.g., FirstName, DateOfBirth, etc.), you need to create a custom user class:

//csharp
//Copy
//Edit
//public class ApplicationUser : IdentityUser
//{
//    public string FirstName { get; set; }
//    public DateTime? DateOfBirth { get; set; }
//}
//Then update your context:
//csharp
//Copy
//Edit
//public class NorthwindContext : IdentityDbContext<ApplicationUser>
//{
//    public NorthwindContext(DbContextOptions<NorthwindContext> options)
//        : base(options) { }
//}
//And update your Program.cs:

//csharp
//Copy
//Edit
//builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//    .AddEntityFrameworkStores<NorthwindContext>()
//    .AddDefaultTokenProviders()
//    .AddDefaultUI();