using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Dto;

namespace Backend.Controllers
{
    [ApiController]
    [Route("Auth")]
    public class AuthController : ControllerBase
    {
        private readonly AccessService _accessService;
        private const string AdminUser = "admin";
        private const string AdminPass = "supersecret";

        public AuthController(AccessService accessService)
        {
            _accessService = accessService;
        }
 
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            if (dto.Username == AdminUser && dto.Password == AdminPass)
            {
                var token = _accessService.CreateSession("admin");
                return Ok(new { role = "admin", token });
            }
            return Unauthorized();
        }

        [HttpGet("validate")]
        public IActionResult Validate([FromHeader(Name = "Authorization")] string bearer)
        {
            if (string.IsNullOrEmpty(bearer) || !bearer.StartsWith("Bearer "))
                return Unauthorized();
            var token = bearer["Bearer ".Length..];
            if (_accessService.ValidateSession(token, out var role))
                return Ok(new { role });
            return Unauthorized();
        }

        [HttpPost("generate-codes")]
        public IActionResult GenerateCodes([FromBody] GenerateDto dto)
        {
            var result = _accessService.GenerateCodes(dto.ExaminersCount);
            return Ok(result);
        }

        [HttpPost("code-login")]
        public IActionResult CodeLogin([FromBody] CodeDto dto)
        {
            if (_accessService.TryValidateCode(dto.Code, out var role))
            {
                var token = _accessService.CreateSession(role);
                return Ok(new { role, token });
            }
            return Unauthorized("Nieprawidłowy kod");
        }

    }
}
