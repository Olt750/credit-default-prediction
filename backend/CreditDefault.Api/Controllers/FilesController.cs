using CreditDefault.Api.Models;
using CreditDefault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly FileRecordService _service;
        public FilesController(FileRecordService service) => _service = service;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var file = await _service.GetByIdAsync(id);
            return file == null ? NotFound() : Ok(file);
        }

        [HttpPost("metadata")]
        public async Task<IActionResult> CreateMetadata(FileRecord fileRecord)
        {
            fileRecord.Id = fileRecord.Id == Guid.Empty ? Guid.NewGuid() : fileRecord.Id;
            fileRecord.CreatedAt = DateTime.UtcNow;
            fileRecord.UpdatedAt = DateTime.UtcNow;
            await _service.CreateAsync(fileRecord);
            return CreatedAtAction(nameof(Get), new { id = fileRecord.Id }, fileRecord);
        }
    }
}
