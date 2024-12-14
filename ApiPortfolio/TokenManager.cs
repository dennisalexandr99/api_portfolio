using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace ApiPortfolio
{
    public class TokenManager
    {
        public static string GenerateJwtToken(string secretKey, string issuer, string audience, int expiryInMinutes, string username, string roleLevel)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Typ, roleLevel),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                signingCredentials: signingCredentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public string[] ValidateJwtToken(string token, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);

            string sub = "";

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out var validatedToken);

                if (validatedToken is not null)
                {
                    var jsonToken = validatedToken as JwtSecurityToken;
                    var subClaim = jsonToken?.Claims?.FirstOrDefault(c => c.Type == "sub")?.Value;
                    var typsClaim = jsonToken?.Claims?.FirstOrDefault(c => c.Type == "typ")?.Value;

                    return new[] { subClaim, typsClaim };
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
