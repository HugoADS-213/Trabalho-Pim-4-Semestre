using System.Security.Cryptography;
using System.Text;
using Buillon.Modelos;
using Buillon.Repositorio;

namespace Buillon.Negocio
{
    // Cérebro da gestão de contas e acessos
    public class AutenticacaoNegocio
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IRestauranteRepositorio _restauranteRepositorio;

        public AutenticacaoNegocio(IUsuarioRepositorio usuarioRepositorio, IRestauranteRepositorio restauranteRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _restauranteRepositorio = restauranteRepositorio;
        }

        public async Task<Usuario> RegistarUsuarioAsync(string nome, string email, string senhaPlana, string role)
        {
            // Regra: Impede registro de e-mails duplicados
            var existente = await _usuarioRepositorio.ObterPorEmailAsync(email);
            if (existente != null) throw new ArgumentException("E-mail já registado.");

            // Gera o Hash antes de salvar no banco
            var novoUsuario = new Usuario { Nome = nome, Email = email, Role = role, SenhaHash = GerarHashSenha(senhaPlana) };
            novoUsuario.Validar();
            await _usuarioRepositorio.AdicionarAsync(novoUsuario);
            return novoUsuario;
        }

        public async Task<(Usuario Usuario, int? RestauranteDoProprietarioId)> ValidarLoginAsync(string email, string senhaPlana)
        {
            // Regra: Busca usuário e verifica se o Hash da senha digitada bate com o do banco
            var usuario = await _usuarioRepositorio.ObterPorEmailAsync(email);
            if (usuario == null || usuario.SenhaHash != GerarHashSenha(senhaPlana)) throw new UnauthorizedAccessException("Credenciais inválidas.");

            int? restauranteId = null;
            // Se for proprietário, já busca o ID do restaurante dele para injetar no token/cookie
            if (usuario.Role == "Proprietario")
            {
                var meuRestaurante = await _restauranteRepositorio.ObterPorProprietarioIdAsync(usuario.Id);
                restauranteId = meuRestaurante?.Id;
            }
            return (usuario, restauranteId);
        }

        // Método de criptografia usando SHA256 com um "Salt" (tempero extra de segurança)
        private static string GerarHashSenha(string senha) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("BuillonSalt2026_" + senha)));
    }

    // Cérebro da gestão de restaurantes
    public class RestauranteNegocio
    {
        private readonly IRestauranteRepositorio _repositorio;
        public RestauranteNegocio(IRestauranteRepositorio repositorio) => _repositorio = repositorio;

        public async Task<List<Restaurante>> ListarNaAreaAsync(string? cidade, string? bairro, int pagina, int tamanho) 
        {
            // Regra: Trava paginações negativas ou downloads gigantescos para proteger o servidor
            if (pagina < 1) pagina = 1;
            if (tamanho < 1 || tamanho > 50) tamanho = 10;
            return await _repositorio.ListarPorAreaAsync(cidade, bairro, pagina, tamanho);
        }

        public async Task<Restaurante?> ObterPorIdAsync(int id) => await _repositorio.ObterCompletoPorIdAsync(id);
        
        public async Task EditarPaginaDoEstabelecimentoAsync(int restauranteId, int proprietarioLogadoId, string nome, string descricao, string enderecoCompleto, string bairro, string cidade, string telefone)
        {
            var restaurante = await _repositorio.ObterCompletoPorIdAsync(restauranteId) ?? throw new KeyNotFoundException("Não encontrado.");
            
            // Regra crítica de segurança: Garante que o usuário logado é realmente o dono do restaurante
            if (restaurante.ProprietarioId != proprietarioLogadoId) throw new UnauthorizedAccessException("Sem permissão.");
            
            restaurante.AtualizarPagina(nome, descricao, enderecoCompleto, bairro, cidade, telefone);
            await _repositorio.AtualizarAsync(restaurante);
        }
    }
}
