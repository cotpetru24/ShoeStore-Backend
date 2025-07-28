using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.Auth;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly AuthService _service;


        public AuthController(AuthService injectedService)
        {
            _service = injectedService;
        }



        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var token = await _service.LoginAsync(request);

                if (token == null) return NotFound("Invalid credentials");
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error:{ex.Message}");
            }
        }


        [HttpPost("register")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                var token = await _service.RegisterAsync(request);

                if (token == null) return BadRequest("Failed to create user.");

                return Ok(new { token });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

    }
}




//public class RolesController : Controller
//{
//    private string _adminRole = "Administrators";

//    private string _userEmail = "admin@admin.com";

//    private readonly RoleManager<IdentityRole> _roleManager;

//    private readonly UserManager<IdentityUser> _userManager;


//    public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
//    {
//        this._roleManager = roleManager;
//        this._userManager = userManager;
//    }

//    public async Task<IActionResult> Index()
//    {
//        if (!(await _roleManager.RoleExistsAsync(_adminRole)))
//        {
//            await _roleManager.CreateAsync(new IdentityRole(_adminRole));
//        }

//        IdentityUser user = await _userManager.FindByEmailAsync(_userEmail);

//        if (user == null)
//        {
//            user = new IdentityUser()
//            {
//                UserName = _userEmail,
//                Email = _userEmail,
//                //EmailConfirmed = true
//            };

//            IdentityResult result = await _userManager.CreateAsync(user, "Pa$$w0rd");

//            if (result.Succeeded)
//            {
//                Console.WriteLine($"User {user.UserName} created successfully.");
//            }

//            else
//            {
//                foreach (IdentityError error in result.Errors)
//                {
//                    Console.WriteLine(error.Description);
//                }
//            }
//        }

//        if (!user.EmailConfirmed)
//        {
//            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

//            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

//            if (result.Succeeded)
//            {
//                Console.WriteLine($"Email for user {user.UserName} confirmed successfully.");
//            }
//            else
//            {
//                foreach (IdentityError error in result.Errors)
//                {
//                    Console.WriteLine(error.Description);
//                }
//            }
//        }

//        if (!(await _userManager.IsInRoleAsync(user, _adminRole)))
//        {
//            IdentityResult result = await _userManager.AddToRoleAsync(user, _adminRole);

//            if (result.Succeeded)
//            {
//                Console.WriteLine($"User {user.UserName} added to role {_adminRole} successfully.");
//            }
//            else
//            {
//                foreach (IdentityError error in result.Errors)
//                {
//                    Console.WriteLine(error.Description);
//                }
//            }
//        }


//        return Redirect("/");
//    }
//}

