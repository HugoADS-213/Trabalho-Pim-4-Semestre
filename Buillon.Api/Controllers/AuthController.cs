using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Buillon.Api.Dtos;
using Buillon.Modelos;
using Buillon.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Buillon.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous] // Permite acesso sem token (pois o usuário ainda está tentando fazer login)
    public class AuthController : ControllerBase
    {
        private readonly AutenticacaoNegocio _authNegocio;
        private readonly IConfiguration _config;

        public AuthController(AutenticacaoNegocio authNegocio, IConfiguration config)
        {
            _authNegocio = authNegocio;
            _config = config;
        }

        [HttpPost("registar")]
        public async Task<IActionResult> Registar([FromBody] RegistoUsuarioDto dto)
        {
            try
            {
                var usuario = await _authNegocio.RegistarUsuarioAsync(dto.Nome, dto.Email, dto.Senha, dto.Role);
                return StatusCode(201, new { usuario.Id, usuario.Nome, usuario.Email, usuario.Role });
            }
            catch (ArgumentException ex) { return Problem(detail: ex.Message, statusCode: 400); }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var (usuario, restauranteId) = await _authNegocio.ValidarLoginAsync(dto.Email, dto.Senha);
                var token = GerarTokenJwt(usuario); // Gera o Token se a senha estiver correta
                return Ok(new { token, usuario.Id, usuario.Nome, usuario.Role, restauranteId });
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); } // Erro 401
        }

        // Método que cria o "Crachá Virtual" para o Flutter
        private string GerarTokenJwt(Usuario usuario)
        {
            var chave = _config["Jwt:Key"] ?? "ChaveSecretaBuillonDesenvolvimentoComMaisDe32Caracteres!";
            var credenciais = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)), SecurityAlgorithms.HmacSha256);

            // Grava o ID e o Papel do usuário dentro do token para uso nas telas
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: credenciais);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
