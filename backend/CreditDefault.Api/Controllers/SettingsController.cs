using CreditDefault.Api.Models;
using CreditDefault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Route("api/settings")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : ControllerBase
    {
        private readonly SettingService _service;
        public SettingsController(SettingService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpPost]
        public async Task<IActionResult> Create(Setting setting)
        {
            setting.Id = setting.Id == Guid.Empty ? Guid.NewGuid() : setting.Id;
            setting.CreatedAt = DateTime.UtcNow;
            setting.UpdatedAt = DateTime.UtcNow;
            await _service.CreateAsync(setting);
            return CreatedAtAction(nameof(GetAll), new { id = setting.Id }, setting);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, Setting dto)
        {
            var setting = await _service.GetByIdAsync(id);
            if (setting == null) return NotFound();

            setting.Key = dto.Key;
            setting.Value = dto.Value;
            setting.Description = dto.Description;
            setting.IsSensitive = dto.IsSensitive;
            setting.UpdatedAt = DateTime.UtcNow;
            await _service.UpdateAsync(setting);
            return Ok(setting);
        }
    }
}
