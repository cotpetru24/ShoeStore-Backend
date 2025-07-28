using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<IdentityUser> injectedUserManager, RoleManager<IdentityRole> inctedRoleManager)
        {
            _roleManager = inctedRoleManager;
            _userManager = injectedUserManager;
        }

        [HttpGet("user")]
        public async Task GetUserAsync()
        {

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task GetAllUsersAsync()
        {

        }

        [HttpPut]
        public async Task UpdateUserAsync()
        {

        }
    }
}
