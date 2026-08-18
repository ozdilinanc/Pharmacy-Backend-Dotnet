using Microsoft.AspNetCore.Mvc;
using PharmacyProject.Application.DTOs.Auth;
using PharmacyProject.Application.Interfaces.Services;

namespace PharmacyProject.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {   
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var token = await _authService.RegisterAsync(dto);
            _logger.LogInformation("Yeni kullanıcı kayıt oldu: {Email}", dto.Email);
            return StatusCode(201, token);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _authService.LoginAsync(dto);
                _logger.LogInformation("Kullanıcı başarıyla giriş yaptı: {Email}", dto.Email);
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Başarısız giriş denemesi: {Email}. Sebep: {Message}", dto.Email, ex.Message);
                throw; 
            }
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(new { 
                identityName = User.Identity?.Name, 
                authenticationType = User.Identity?.AuthenticationType,
                isInSuperAdmin = User.IsInRole("SuperAdmin"),
                isInAdmin = User.IsInRole("Admin"),
                claims 
            });
        }
    }
}