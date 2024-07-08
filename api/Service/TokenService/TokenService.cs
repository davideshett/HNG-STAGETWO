using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using api.Data;
using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace api.Service.TokenService
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration config;
        private readonly UserManager<User> userManager;
        private readonly DataContext dataContext;
         private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config, UserManager<User> userManager, DataContext dataContext)
        {
            this.config = config;
            this.userManager = userManager;
            this.dataContext = dataContext;
             _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("JwtSettings:Key").Value));
        }
        public async Task<string> CreateToken(User user)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]));

            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var userClaims = await userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.NormalizedEmail.ToLower()),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new (JwtRegisteredClaimNames.Email, user.NormalizedEmail.ToLower()),
                new ("userId", user.UserId.ToString()),
                new ("fullname", $"{user.FirstName} {user.LastName}"),
            }
            .Union(userClaims);

            var token = new JwtSecurityToken(
                issuer: config["JwtSettings:Issuer"],
                audience: config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(config["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}