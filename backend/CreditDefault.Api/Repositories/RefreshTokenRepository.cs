using CreditDefault.Api.Data;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        public RefreshTokenRepository(AppDbContext context) => _context = context;

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
            await _context.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        public async Task AddAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}
