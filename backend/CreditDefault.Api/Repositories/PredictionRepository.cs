using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CreditDefault.Api.Models;
using CreditDefault.Api.Data;
using CreditDefault.Api.Interfaces;

namespace CreditDefault.Api.Repositories
{
    public class PredictionRepository : IPredictionRepository
    {
        private readonly AppDbContext _context;
        public PredictionRepository(AppDbContext context) => _context = context;

        public async Task<Prediction> GetByIdAsync(Guid id) => await _context.Predictions.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
        public async Task<IEnumerable<Prediction>> GetByUserIdAsync(Guid userId) => await _context.Predictions.Where(p => p.UserId == userId).ToListAsync();
        public async Task<IEnumerable<Prediction>> GetAllAsync() => await _context.Predictions.Include(p => p.User).ToListAsync();
        public async Task AddAsync(Prediction prediction) { _context.Predictions.Add(prediction); await _context.SaveChangesAsync(); }
    }
}