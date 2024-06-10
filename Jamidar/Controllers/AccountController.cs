//using Jamidar.Data;
//using Jamidar.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Jamidar.Controllers
//{
//    public class AccountController : ControllerBase
//    {

//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly RoleManager<IdentityRole> _roleManager;
//        private readonly ApplicationDbContext _context;
//        private readonly ITokenService _tokenService;
//        private readonly ILogger<AccountController> _logger;

//        public AccountController(
//            UserManager<ApplicationUser> userManager,
//            RoleManager<IdentityRole> roleManager,
//            ApplicationDbContext context,
//            ITokenService tokenService,
//            ILogger<AccountController> logger
//        )
//        {
//            _userManager = userManager;
//            _roleManager = roleManager;
//            _context = context;
//            _tokenService = tokenService;
//            _logger = logger;
//        }

//        [HttpPost("register")]
//        public async Task<IActionResult> Register(RegistrationRequest request)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            foreach (var role in request.Role)
//            {
//                if (!await _roleManager.RoleExistsAsync(role))
//                {
//                    await _roleManager.CreateAsync(new IdentityRole(role));
//                }
//            }

//            var user = new ApplicationUser { UserName = request.Username, Email = request.Email, Role = request.Role };

//            var result = await _userManager.CreateAsync(user, request.Password!);

//            if (result.Succeeded)
//            {
//                await _userManager.AddToRolesAsync(user, request.Role);
//                request.Password = "";
//                return CreatedAtAction(nameof(Register), new { email = request.Email, role = request.Role }, request);
//            }

//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(error.Code, error.Description);
//            }

//            return BadRequest(ModelState);
//        }

//        [HttpPost("login")]
//        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            var managedUser = await _userManager.FindByEmailAsync(request.Email);

//            if (managedUser == null)
//            {
//                return BadRequest("Bad credentials");
//            }

//            var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);

//            if (!isPasswordValid)
//            {
//                return BadRequest("Bad credentials");
//            }

//            var userInDb = _context.Users.FirstOrDefault(u => u.Email == request.Email);

//            if (userInDb is null)
//            {
//                return Unauthorized(request);
//            }

//            var accessToken = _tokenService.CreateToken(userInDb);
//            await _context.SaveChangesAsync();

//            return Ok(new AuthResponse
//            {
//                Username = userInDb.UserName,
//                Email = userInDb.Email,
//                Token = accessToken,
//                Roles = userInDb.Role.ToArray()
//            });
//        }

//        [HttpGet("GetUsers")]
//        public async Task<IActionResult> UserIndex()
//        {
//            return Ok(await _userManager.Users.ToListAsync());
//        }

//        [HttpGet("GetUser/{id}")]
//        public async Task<IActionResult> UserIndex(string id)
//        {
//            return Ok(await _userManager.FindByIdAsync(id));
//        }

//        [HttpGet("GetRoles")]
//        [Authorize]
//        public async Task<IActionResult> RoleIndex()
//        {
//            return Ok(await _roleManager.Roles.ToListAsync());
//        }

//        [HttpPost("create-role")]
//        [Authorize(Roles = "Admin")]
//        public async Task<ActionResult> CreateRole([FromBody] UserRoleDto request)
//        {
//            IdentityRole role = new IdentityRole
//            {
//                Name = request.Name,
//            };

//            var result = await _roleManager.CreateAsync(role);

//            if (result.Succeeded)
//            {
//                return Ok(request);
//            }

//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(error.Code, error.Description);
//            }

//            return BadRequest(ModelState);
//        }

//        [HttpPut("edit-role")]
//        [Authorize(Roles = "Admin")]
//        public async Task<ActionResult> EditRole([FromBody] UserRoleDto request)
//        {
//            var role = await _roleManager.FindByIdAsync(request.Id);

//            if (role == null)
//                return BadRequest();

//            role.Name = request.Name;

//            var result = await _roleManager.UpdateAsync(role);

//            if (result.Succeeded)
//            {
//                return Ok(request);
//            }

//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(error.Code, error.Description);
//            }

//            return BadRequest(ModelState);
//        }

//        [HttpDelete("delete-role/{id}")]
//        [Authorize(Roles = "Admin")]
//        public async Task<ActionResult> DeleteRole(string id)
//        {
//            IdentityRole? role = await _roleManager.FindByIdAsync(id);

//            if (role == null)
//                return BadRequest();

//            var users = await _userManager.GetUsersInRoleAsync(role.Name);

//            if (users.Count > 0)
//            {
//                return BadRequest("User exists in this role");
//            }

//            var result = await _roleManager.DeleteAsync(role);

//            if (result.Succeeded)
//            {
//                return Ok();
//            }

//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(error.Code, error.Description);
//            }

//            return BadRequest(ModelState);
//        }

//        [HttpPost("AssignRole")]
//        [Authorize(Roles = "Admin, Operator")]
//        public async Task<IActionResult> AssignRole(AssignRoleDto model)
//        {
//            try
//            {
//                var user = await _userManager.FindByIdAsync(model.Id);
//                if (user is null) return BadRequest($"User not found by id: {model.Id}");

//                var userRoles = await _userManager.GetRolesAsync(user);

//                List<string?> dbRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

//                foreach (var role in dbRoles)
//                {
//                    if (model.Role.Contains(role))
//                    {
//                        if (!userRoles.Contains(role))
//                        {
//                            await _userManager.AddToRoleAsync(user, role);
//                        }
//                    }
//                    else
//                    {
//                        if (userRoles.Contains(role))
//                        {
//                            await _userManager.RemoveFromRoleAsync(user, role);
//                        }
//                    }
//                }

//                user.Role = model.Role;
//                await _userManager.UpdateAsync(user);

//                return Ok();
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(ex);
//            }
//        }
//    }
//}


using Jamidar.Data;
using Jamidar.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;

namespace Jamidar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AccountController> _logger;
        private readonly object roles;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ITokenService tokenService,
            ILogger<AccountController> logger
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var role in request.Role)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var user = new ApplicationUser { UserName = request.Username, Email = request.Email };

            var result = await _userManager.CreateAsync(user, request.Password!);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, request.Role);
                request.Password = "";
                return CreatedAtAction(nameof(Register), new { email = request.Email, role = request.Role }, request);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var managedUser = await _userManager.FindByEmailAsync(request.Email);

            if (managedUser == null)
            {
                return BadRequest("Bad credentials");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(managedUser, request.Password);

            if (!isPasswordValid)
            {
                return BadRequest("Bad credentials");
            }

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (userInDb == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            var accessToken = _tokenService.CreateToken(userInDb);
            await _context.SaveChangesAsync();

            return Ok(new AuthResponse
            {
                Username = userInDb.UserName,
                Email = userInDb.Email,
                Token = accessToken,
                Roles = userInDb.Role.ToArray()
            });
        }

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(await _userManager.Users.ToListAsync());
        }

        [HttpGet("GetUser/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }

        [HttpGet("GetRoles")]
        [Authorize]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _roleManager.Roles.ToListAsync());
        }

        [HttpPost("create-role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateRole([FromBody] UserRoleDto request)
        {
            IdentityRole role = new IdentityRole
            {
                Name = request.Name,
            };

            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return Ok(request);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPut("edit-role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditRole([FromBody] UserRoleDto request)
        {
            var role = await _roleManager.FindByIdAsync(request.Id);

            if (role == null)
            {
                return BadRequest("Role not found.");
            }

            role.Name = request.Name;

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return Ok(request);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("delete-role/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            IdentityRole? role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return BadRequest("Role not found.");
            }

            var users = await _userManager.GetUsersInRoleAsync(role.Name);

            if (users.Count > 0)
            {
                return BadRequest("Users exist in this role.");
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return Ok();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("AssignRole")]
        [Authorize(Roles = "Admin, Operator")]
        public async Task<IActionResult> AssignRole(AssignRoleDto model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return BadRequest($"User not found by id: {model.Id}");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var dbRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

                foreach (var role in dbRoles)
                {
                    if (model.Role.Contains(role))
                    {
                        if (!userRoles.Contains(role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                    }
                    else
                    {
                        if (userRoles.Contains(role))
                        {
                            await _userManager.RemoveFromRoleAsync(user, role);
                        }
                    }
                }

                await _userManager.UpdateAsync(user);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles");
                return BadRequest(ex.Message);
            }
        }
    }
}
