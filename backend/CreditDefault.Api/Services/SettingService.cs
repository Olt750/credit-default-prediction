using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Services
{
    public class SettingService
    {
        private readonly IRepository<Setting> _repository;
        public SettingService(IRepository<Setting> repository) => _repository = repository;

        public Task<List<Setting>> GetAllAsync() => _repository.GetAllAsync();
        public Task<Setting?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);
        public Task CreateAsync(Setting setting) => _repository.AddAsync(setting);
        public Task UpdateAsync(Setting setting) => _repository.UpdateAsync(setting);
    }
}
