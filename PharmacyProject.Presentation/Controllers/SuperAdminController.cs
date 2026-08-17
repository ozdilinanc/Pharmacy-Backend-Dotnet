using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyProject.Application.DTOs.Admin;
using PharmacyProject.Infrastructure.Persistence.Context;

namespace PharmacyProject.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(AppDbContext context, ILogger<SuperAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPut("change-role")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeRole([FromBody] ChangeRoleDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var oldRole = user.Role;
            user.Role = dto.NewRole;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kullanıcı rolü değiştirildi: {Email}. Eski Rol: {OldRole} -> Yeni Rol: {NewRole}", user.Email, oldRole, user.Role);

            return Ok(new { Message = $"'{user.Email}' adlı kullanıcının yetkisi {user.Role} olarak güncellendi." });
        }
    }
}
