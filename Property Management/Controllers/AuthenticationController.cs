
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Property_Management.Authentication;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Property_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

        public BinaryReader JwtRegisteredClaims { get; private set; }

        public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._configuration = configuration;
        }

        [HttpPost]
        [Route("register")]

        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var userExist = await userManager.FindByNameAsync(registerModel.username);
            if (userExist != null)

            return StatusCode(StatusCodes.Status500InternalServerError, new Response { status = "Failed", message = "User Already Exists" });

            
            
            ApplicationUser user = new ApplicationUser()
            {
                Email = registerModel.email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerModel.username

            };

            var result = await userManager.CreateAsync(user, registerModel.password);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { status = "Failed", message = "User Not Created" });
            }

            else
            {
                return Ok(new Response { status = "Success", message = "User Created Successfully" });
            }
        }

        [HttpPost]
        [Route("login")]

        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var user = await userManager.FindByNameAsync(loginModel.username);
            if(user != null && await userManager.CheckPasswordAsync(user, loginModel.password))
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach(var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var authSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secrete"]));
                var token = new JwtSecurityToken
                    (
                        issuer:_configuration["JWT:ValidUser"],
                        audience:_configuration["JWT:ValidAudience"],
                        expires:DateTime.Now.AddHours(3),
                        claims:authClaims,
                        signingCredentials:new SigningCredentials(authSignInKey,SecurityAlgorithms.HmacSha256)

                    );
                return Ok
                    (
                        new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token)
                            
                        }
                    );
            }
            return Unauthorized();
        }
        [HttpPost]
        [Route("register_admin")]

        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel registerModel)
        {
            var userExist = await userManager.FindByNameAsync(registerModel.username);
            if (userExist != null)

                return StatusCode(StatusCodes.Status500InternalServerError, new Response { status = "Failed", message = "User Already Exists" });



            ApplicationUser user = new ApplicationUser()
            {
                Email = registerModel.email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerModel.username

            };

            var result = await userManager.CreateAsync(user, registerModel.password);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { status = "Failed", message = "User Not Created" });
            }

            else
            {
                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                {
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                }
                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                {
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                }
                if (await roleManager.RoleExistsAsync(UserRoles.Admin))
                {
                    await userManager.AddToRoleAsync(user, UserRoles.User);
                }
                return Ok(new Response { status = "Success", message = "User Created Successfully" });
            }
        }

    }
}
