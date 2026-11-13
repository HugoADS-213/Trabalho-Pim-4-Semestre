using Buillon.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buillon.Web.Controllers
{
    public class RestaurantesController : Controller
    {
        private readonly RestauranteNegocio _negocio;
        public RestaurantesController(RestauranteNegocio negocio) => _negocio = negocio;

        [HttpGet]
        public async Task<IActionResult> Index(string cidade)
        {
            // Busca os dados da camada de Negócio e injeta diretamente no HTML da View
            var restaurantes = await _negocio.ListarNaAreaAsync(cidade, null, 1, 50);
            return View(restaurantes);
        }

        [Authorize(Roles = "Proprietario")]
        public IActionResult MinhaPagina()
        {
            // A View de edição só abre se o Cookie do navegador confirmar que é Proprietário
            return View();
        }
    }
}
