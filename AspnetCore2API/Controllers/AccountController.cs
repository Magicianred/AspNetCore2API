using FormPdfEasy.Entities;
using FormPdfEasy.Interfaces;
using FormPdfEasy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FormPdfEasy.Controllers
{
    [Produces("application/json")]
    [Route("api/auth")]
    public class AccountController : Controller
    {
        private IConfiguration _configuration { get; }
        private UserManager<CustomUser> _userManager;
        private IPasswordHasher<CustomUser> _passwordHasher;
      

        private ILogger<UserController> _logger;
        private IUserRepository _userRepository;

        public AccountController(
            UserManager<CustomUser> userManager, 
            IPasswordHasher<CustomUser> passwordHasher, 
            IConfiguration configuration,
            ILogger<UserController> logger,
            IUserRepository userRepository)
        {
            _configuration = configuration;
            _userManager = userManager;
            _passwordHasher = passwordHasher;

            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Issue([FromBody] UserCredentialDto userCredential) {

            var user = await _userManager.FindByNameAsync(userCredential.UserName);

            if (user == null)
                return NotFound();

            if (!user.EmailConfirmed)
                return NotFound("User email not confirmed"); //TODO: inform user the email need to verify before can use the api

;            var verifiedUser = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userCredential.Password) == PasswordVerificationResult.Success;

            if (!verifiedUser)
                return BadRequest("User verification failed");

            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }.Union(userClaims);

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["IssuerSigningKey"]));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToInt32(_configuration["TokenExpirationNoDays"]));

            var securityToken = new JwtSecurityToken(
                _configuration["Issuer"],
                _configuration["Audience"],
                claims,
                expires: expires,
                signingCredentials: credentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            var tokenInformation = new TokenDto()
            {
                token = token,
                expiration = expires
            };

            return Ok(tokenInformation);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            try
            {
                var user = new CustomUser
                {
                    UserName = registerUserDto.UserName,
                    FirstName = registerUserDto.FirstName,
                    LastName = registerUserDto.LastName,
                    Gender = registerUserDto.Gender,
                    Email = registerUserDto.Email,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var userResult = await _userManager.CreateAsync(user, registerUserDto.Password);
                var claimResult = await _userManager.AddClaimAsync(user, new Claim("SuperUser", "True"));

                var emailTokenVerification = _userManager.GenerateEmailConfirmationTokenAsync(user);

                //TODO:send emailToken to mail. Create email service

                return StatusCode((int)HttpStatusCode.Created);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

        }

        public async Task<IActionResult> ConfirmEmail(string userId, string emailVerificationToken)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogError("ConfirmEmail: User not found on");
                return NotFound();
            }

            var emailConfirmationResult = await _userManager.ConfirmEmailAsync(user, emailVerificationToken);

            if (!emailConfirmationResult.Succeeded)
            {
                _logger.LogError("ConfirmEmail: " + emailConfirmationResult
                    .Errors.Select(error => error.Description)
                    .Aggregate((allErrors, error) => allErrors += ", " + error));

                return BadRequest();
            }
            return Ok();
        }
    }
}
