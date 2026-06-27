using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Services
{
    public class FileRecordService
    {
        private readonly IRepository<FileRecord> _repository;
        public FileRecordService(IRepository<FileRecord> repository) => _repository = repository;

        public Task<List<FileRecord>> GetAllAsync() => _repository.GetAllAsync();
        public Task<FileRecord?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);
        public Task CreateAsync(FileRecord fileRecord) => _repository.AddAsync(fileRecord);
    }
}
