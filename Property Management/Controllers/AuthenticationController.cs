
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
using Property_Management.Models;

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
            ResponseModel responseModel = new ResponseModel { status=ResponseModel.StatusCodes.fail, message =Configs.defaultErrorMsg};

            var userExist = await userManager.FindByNameAsync(registerModel.username);
            if (userExist != null)
            {
                responseModel.message = Configs.user_exist_msg;
                return StatusCode(StatusCodes.Status500InternalServerError, responseModel);

            }
            
            ApplicationUser user = new ApplicationUser()
            {
                Email = registerModel.email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerModel.username

            };

            var result = await userManager.CreateAsync(user, registerModel.password);

            if (!result.Succeeded)
            {
                responseModel.message = Configs.user_not_created_msg;
                return StatusCode(StatusCodes.Status500InternalServerError, responseModel);
            }

            else
            {
                responseModel.message = Configs.user_created_msg;
                responseModel.status = ResponseModel.StatusCodes.success;
                return Ok(responseModel);
            }
        }

        [HttpPost]
        [Route("login")]

        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            ResponseModel response = new ResponseModel { status = ResponseModel.StatusCodes.fail , message= Configs.defaultErrorMsg};
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
                response.status = ResponseModel.StatusCodes.success;
                response.message = Configs.login_successful_msg;
                response.data = new { token = new JwtSecurityTokenHandler().WriteToken(token) };
                return Ok
                    (
                        response
                    );
            }

            response.message = Configs.login_failed_msg;

            return StatusCode(StatusCodes.Status401Unauthorized, response);

            //return Unauthorized();
        }
        [HttpPost]
        [Route("register_admin")]

        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel registerModel)
        {
            ResponseModel response = new ResponseModel
            {
                status = ResponseModel.StatusCodes.fail,
                message = Configs.defaultErrorMsg
            };

            try
            {
                var userExist = await userManager.FindByNameAsync(registerModel.username);
                if (userExist != null)
                {
                    response.message = Configs.user_exist_msg;
                    return StatusCode(StatusCodes.Status500InternalServerError, response);
                }




                ApplicationUser user = new ApplicationUser()
                {
                    Email = registerModel.email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = registerModel.username

                };

                var result = await userManager.CreateAsync(user, registerModel.password);

                if (!result.Succeeded)
                {
                    response.message = Configs.user_not_created_msg;

                    return StatusCode(StatusCodes.Status500InternalServerError, response);
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

                    response.status = ResponseModel.StatusCodes.success;
                    response.message = Configs.user_created_msg;

                    return Ok(response);
                }
            }
            catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }

            
        }

    }
}
