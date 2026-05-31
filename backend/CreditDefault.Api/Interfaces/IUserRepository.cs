using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}