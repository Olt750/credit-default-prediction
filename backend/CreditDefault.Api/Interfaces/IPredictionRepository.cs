using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Interfaces
{
    public interface IPredictionRepository
    {
        Task<Prediction> GetByIdAsync(Guid id);
        Task<IEnumerable<Prediction>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Prediction>> GetAllAsync();
        Task AddAsync(Prediction prediction);
    }
}