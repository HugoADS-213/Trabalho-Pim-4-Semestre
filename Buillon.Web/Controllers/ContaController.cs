using System.Security.Claims;
using Buillon.Negocio;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Buillon.Web.Controllers
{
    // Controlador voltado para telas (retorna Views e redirecionamentos, não JSON)
    public class ContaController : Controller
    {
        private readonly AutenticacaoNegocio _authNegocio;
        public ContaController(AutenticacaoNegocio authNegocio) => _authNegocio = authNegocio;

        [HttpPost]
        public async Task<IActionResult> Login(string email, string senha)
        {
            try
            {
                var (usuario, _) = await _authNegocio.ValidarLoginAsync(email, senha);
                
                // Cria os dados de identificação para guardar no Cookie
                var claims = new List<Claim> { 
                    new(ClaimTypes.NameIdentifier, usuario.Id.ToString()), 
                    new(ClaimTypes.Role, usuario.Role) 
                };
                
                var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Grava o Cookie no navegador do usuário
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidade));

                // Se for dono de restaurante, joga para a Dashboard dele
                if (usuario.Role == "Proprietario")
                    return RedirectToAction("MinhaPagina", "Restaurantes");

                // Se for cliente, joga para a Home page
                return RedirectToAction("Index", "Home");
            }
            catch { return View(); } // Se a senha errar, recarrega a página de login
        }
    }
}
