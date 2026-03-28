using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly AppDbContext _context;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions, AppDbContext context)
    {
        _jwtOptions = jwtOptions.Value;
        _context = context;
    }

    public async Task<(string AccessToken, string RefreshToken, DateTime ExpiraEm)> GerarTokensAsync(
        string identityUserId,
        string email,
        string cargo,
        Guid usuarioId,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiracaoMinutos);

        var claims = new List<Claim>
        {
            new("sub", identityUserId),
            new("email", email),
            new("role", cargo),
            new("usuario_id", usuarioId.ToString()),
            new("tenant_id", tenantId?.ToString() ?? ""),
            new("is_super_admin", (cargo == "SuperAdmin").ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshTokenString = GerarRefreshTokenString();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            IdentityUserId = identityUserId,
            Token = HashToken(refreshTokenString),
            ExpiraEm = DateTime.UtcNow.AddDays(_jwtOptions.RefreshExpiracaoDias),
            CriadoEm = DateTime.UtcNow
        };

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return (accessToken, refreshTokenString, expiraEm);
    }

    public Task<(string IdentityUserId, string Email)?> ValidarAccessTokenExpiradoAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));

        try
        {
            tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // Aceita tokens expirados para refresh
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var identityUserId = jwtToken.Claims.First(c => c.Type == "sub").Value;
            var email = jwtToken.Claims.First(c => c.Type == "email").Value;

            return Task.FromResult<(string, string)?>(( identityUserId, email));
        }
        catch
        {
            return Task.FromResult<(string, string)?>(null);
        }
    }

    public async Task<bool> ValidarRefreshTokenAsync(
        string identityUserId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var hash = HashToken(refreshToken);

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                t => t.IdentityUserId == identityUserId && t.Token == hash,
                cancellationToken);

        return token is not null && token.EstaAtivo;
    }

    public async Task RevogarRefreshTokenAsync(
        string identityUserId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var hash = HashToken(refreshToken);

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                t => t.IdentityUserId == identityUserId && t.Token == hash,
                cancellationToken);

        if (token is not null)
        {
            token.RevogadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static string GerarRefreshTokenString()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
