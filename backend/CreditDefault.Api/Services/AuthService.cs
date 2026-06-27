using CreditDefault.Api.DTOs;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;
        private readonly RefreshTokenService _refreshTokenService;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            PasswordService passwordService,
            JwtService jwtService,
            RefreshTokenService refreshTokenService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<object> RegisterAsync(RegisterDto dto, string? ipAddress)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new InvalidOperationException("Passwords do not match.");

            if (await _userRepository.GetByEmailAsync(dto.Email) != null)
                throw new InvalidOperationException("Email already registered.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _roleRepository.AddUserToRoleAsync(user.Id, "User");
            user = await _userRepository.GetByIdAsync(user.Id) ?? user;

            return await BuildAuthResponseAsync(user, ipAddress, "Registration successful.");
        }

        public async Task<object> LoginAsync(LoginDto dto, string? ipAddress)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            if (user.UserRoles.Count == 0)
            {
                await _roleRepository.AddUserToRoleAsync(user.Id, user.Role);
                user = await _userRepository.GetByIdAsync(user.Id) ?? user;
            }

            return await BuildAuthResponseAsync(user, ipAddress);
        }

        public async Task<object> RefreshAsync(string refreshToken, string? ipAddress)
        {
            var (oldToken, newPlainToken) = await _refreshTokenService.RotateAsync(refreshToken, ipAddress);
            var user = await _userRepository.GetByIdAsync(oldToken.UserId) ?? throw new UnauthorizedAccessException("Invalid refresh token.");
            var token = _jwtService.GenerateToken(user);

            return new
            {
                accessToken = token.AccessToken,
                token = token.AccessToken,
                refreshToken = newPlainToken,
                expiresAt = token.ExpiresAt,
                user = ToUserPayload(user)
            };
        }

        public Task<bool> LogoutAsync(string refreshToken, string? ipAddress) =>
            _refreshTokenService.RevokeAsync(refreshToken, ipAddress);

        private async Task<object> BuildAuthResponseAsync(User user, string? ipAddress, string? message = null)
        {
            var token = _jwtService.GenerateToken(user);
            var refresh = await _refreshTokenService.CreateAsync(user.Id, ipAddress);
            return new
            {
                accessToken = token.AccessToken,
                token = token.AccessToken,
                refreshToken = refresh.PlainToken,
                expiresAt = token.ExpiresAt,
                user = ToUserPayload(user),
                message
            };
        }

        private static object ToUserPayload(User user)
        {
            var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
            var role = roles.FirstOrDefault() ?? user.Role;
            return new { user.Id, user.FullName, user.Email, Role = role, Roles = roles };
        }
    }
}
