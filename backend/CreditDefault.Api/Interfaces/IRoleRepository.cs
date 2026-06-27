using CreditDefault.Api.Models;

namespace CreditDefault.Api.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name);
        Task<List<string>> GetUserRoleNamesAsync(Guid userId);
        Task<List<string>> GetUserPermissionsAsync(Guid userId);
        Task AddUserToRoleAsync(Guid userId, string roleName);
    }
}
