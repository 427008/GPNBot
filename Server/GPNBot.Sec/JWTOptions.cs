using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace GPNBot.Sec
{
    public static class JWTOptions
    {
        public const string ISSUER = "GPNBot";
        public const string AUDIENCE = "https://GPNBot/";
        const string KEY = @"";

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }

        public static (string, DateTime?) Token(string userName, string role = null)
        {
            var claims = new List<Claim> { new Claim(ClaimsIdentity.DefaultNameClaimType, userName), };
            if (!string.IsNullOrEmpty(role)) claims.Add(new Claim(ClaimsIdentity.DefaultRoleClaimType, role));

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                "Token",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            var signingCredentials = new SigningCredentials(GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var end = now.Add(TimeSpan.FromDays(1));

            var jwt = new JwtSecurityToken(
                    issuer: ISSUER,
                    audience: AUDIENCE,
                    notBefore: now,
                    claims: claimsIdentity.Claims,
                    expires: end,
                    signingCredentials: signingCredentials);;

            return (new JwtSecurityTokenHandler().WriteToken(jwt), end);
        }

        public static TokenValidationParameters TokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = ISSUER,
                ValidateAudience = true,
                ValidAudience = AUDIENCE,
                ValidateLifetime = true,
                IssuerSigningKey = GetSymmetricSecurityKey(),
                ValidateIssuerSigningKey = true,
            };
        }

        public static long Validate(string code, out string email, out DateTime validTo)
        {
            validTo = DateTime.MinValue;
            email = null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                IPrincipal principal = tokenHandler.ValidateToken(code, TokenValidationParameters(), out SecurityToken validatedToken);
                email = principal.Identity.Name;
                validTo = validatedToken.ValidTo;
            }
            catch (SecurityTokenExpiredException ex)
            {
                email = "TokenExpired";
                return 2;
            }
            catch (Exception ex)
            {
                email = ex.Message;
                return 1;
            }
            return 0;
        }
    }
}
