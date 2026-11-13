using System.Security.Claims;
using Buillon.Api.Dtos;
using Buillon.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buillon.Api.Controllers
{
    [ApiController]
    [Route("api/restaurantes")]
    [Authorize] // Bloqueia acesso, exigindo o Token JWT
    public class RestaurantesController : ControllerBase
    {
        private readonly RestauranteNegocio _negocio;
        public RestaurantesController(RestauranteNegocio negocio) => _negocio = negocio;

        [HttpGet]
        [AllowAnonymous] // Lista pode ser vista por quem não está logado
        public async Task<IActionResult> Listar([FromQuery] string? cidade, [FromQuery] string? bairro, int pagina = 1)
        {
            var lista = await _negocio.ListarNaAreaAsync(cidade, bairro, pagina, 10);
            return Ok(lista.Select(RestauranteDto.De)); // Devolve a lista formatada no DTO
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Proprietario")] // Exige Token e que a Role seja "Proprietario"
        public async Task<IActionResult> EditarPagina(int id, [FromBody] SalvarRestauranteDto dto)
        {
            try
            {
                // Extrai o ID do proprietário direto do token que veio na requisição
                int proprietarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                
                await _negocio.EditarPaginaDoEstabelecimentoAsync(
                    id, proprietarioId, dto.Nome, dto.Descricao,
                    dto.EnderecoCompleto, dto.Bairro, dto.Cidade, dto.Telefone);

                return NoContent(); // Retorna 204 (Sucesso sem corpo de resposta)
            }
            catch (KeyNotFoundException) { return NotFound(); } // 404
            catch (UnauthorizedAccessException) { return Forbid(); } // 403 (Proibido)
            catch (ArgumentException ex) { return Problem(detail: ex.Message, statusCode: 400); }
        }
    }
}
