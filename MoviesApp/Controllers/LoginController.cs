using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoviesApp.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MoviesApp.Controllers
{
    public class LoginController : Controller
    {
        private IConfiguration _config;
        private UserManager<ApplicationUser> userManager;
        public LoginController(IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _config = config;
            this.userManager = userManager;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index([FromServices]UserManager<ApplicationUser> userManager,[FromServices] MovieContext context, [FromBody]UserModel login)
        {
            if (ModelState.IsValid)
            {
                IActionResult response = Unauthorized();
                var user = await AuthenticateUser(login);

                if (user != null)
                {
                    var tokenString = await GenerateJSONWebToken(user);
                    var refreshToken = generateRefreshToken(user);
                    context.RefreshTokens.Add(refreshToken);
                    context.SaveChanges();
                    setTokenCookie(refreshToken.Token);
                    response = Ok(new { token = tokenString, roles = await userManager.GetRolesAsync(user)});
                }

                return response;
            }
            return BadRequest();
        } 

        public async Task<IActionResult> RefreshToken([FromServices] MovieContext context)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var refreshTokenEntity = context.RefreshTokens.Include(x => x.ApplicationUser).First(x => x.Token == refreshToken);
            var newRefreshToken = generateRefreshToken(refreshTokenEntity.ApplicationUser);
            context.RefreshTokens.Add(newRefreshToken);
            refreshTokenEntity.Revoked = DateTime.UtcNow;
            refreshTokenEntity.ReplacedByToken = newRefreshToken.Token;
            context.SaveChanges();
            setTokenCookie(newRefreshToken.Token);
            var tokenString = await GenerateJSONWebToken(refreshTokenEntity.ApplicationUser);
            return Ok(new { token = tokenString, roles = await userManager.GetRolesAsync(refreshTokenEntity.ApplicationUser) });
        }

        private async Task<string> GenerateJSONWebToken(ApplicationUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
            var roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials) ;

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<ApplicationUser> AuthenticateUser(UserModel login)
        {
            var user = await userManager.FindByEmailAsync(login.EmailAddress); ;
            if (await userManager.CheckPasswordAsync(user, login.Password))
            {
                return user;
            }
            return null;
        }

        [HttpPost]
        public IActionResult Logout([FromServices] MovieContext context)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken != null)
            {
                var refreshTokenEntity = context.RefreshTokens.FirstOrDefault(x => x.Token == refreshToken);
                if (refreshTokenEntity != null)
                {
                    refreshTokenEntity.Revoked = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
            Response.Cookies.Delete("refreshToken");
            return Ok();
        }

        private RefreshToken generateRefreshToken(ApplicationUser applicationUser)
        {
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    UserId = applicationUser.Id
                };
            }
        }

        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}
