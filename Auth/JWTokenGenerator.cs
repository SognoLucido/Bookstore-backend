
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using System.Text;

namespace Auth
{
    public class JWTokenGenerator : ITokenService
    {
        private readonly IConfiguration config;
        public JWTokenGenerator(IConfiguration _config) { config = _config; }
       


        public async Task<string> GenerateToken(string UserId , string role)
        {
            var key = Encoding.UTF8.GetBytes(config["Authentication:SecretKey"]!);

            var tokenHandler = new JwtSecurityTokenHandler();

         

            List<Claim> claims = 
                [
                new("UserID", UserId), 
                new("ruoli", role)
                ];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = config["Authentication:Issuer"],
                Audience = config["Authentication:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwt = tokenHandler.WriteToken(token);

            return jwt;
            

          

        }


    }
}
