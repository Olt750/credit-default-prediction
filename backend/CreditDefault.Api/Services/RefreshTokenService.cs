using System.Security.Cryptography;
using System.Text;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Services
{
    public class RefreshTokenService
    {
        private readonly IRefreshTokenRepository _repository;

        public RefreshTokenService(IRefreshTokenRepository repository)
        {
            _repository = repository;
        }

        public async Task<(string PlainToken, RefreshToken Entity)> CreateAsync(Guid userId, string? ipAddress)
        {
            var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = HashToken(plainToken),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            await _repository.AddAsync(entity);
            return (plainToken, entity);
        }

        public async Task<(RefreshToken OldToken, string NewPlainToken)> RotateAsync(string plainToken, string? ipAddress)
        {
            var oldHash = HashToken(plainToken);
            var oldToken = await _repository.GetByHashAsync(oldHash);
            if (oldToken == null || !oldToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            var newPlainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newHash = HashToken(newPlainToken);
            oldToken.RevokedAt = DateTime.UtcNow;
            oldToken.RevokedByIp = ipAddress;
            oldToken.ReplacedByTokenHash = newHash;

            await _repository.UpdateAsync(oldToken);
            await _repository.AddAsync(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = oldToken.UserId,
                TokenHash = newHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            });

            return (oldToken, newPlainToken);
        }

        public async Task<bool> RevokeAsync(string plainToken, string? ipAddress)
        {
            var token = await _repository.GetByHashAsync(HashToken(plainToken));
            if (token == null || token.RevokedAt != null) return false;

            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            await _repository.UpdateAsync(token);
            return true;
        }

        public static string HashToken(string plainToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
            return Convert.ToHexString(bytes);
        }
    }
}
