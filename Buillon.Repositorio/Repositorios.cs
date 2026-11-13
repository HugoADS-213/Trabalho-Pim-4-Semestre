using Buillon.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Buillon.Repositorio
{
    // Contexto principal do Entity Framework Core que faz a ponte com o SQL Server
    public class BuillonContext : DbContext
    {
        public BuillonContext(DbContextOptions<BuillonContext> options) : base(options) { }

        // Representam as tabelas no banco de dados
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Restaurante> Restaurantes { get; set; }
        public DbSet<ItemCardapio> ItensCardapio { get; set; }
        public DbSet<FotoRestaurante> FotosRestaurante { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Garante que não existam dois usuários com o mesmo e-mail no banco
            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
            
            // Mapeia as tabelas para acionarem as Triggers e Procedures criadas no SQL
            modelBuilder.Entity<Avaliacao>().ToTable("Avaliacoes", tb => tb.HasTrigger("TR_AtualizarNotaMediaRestaurante"));
            modelBuilder.Entity<Restaurante>().ToTable("Restaurantes", tb => tb.HasTrigger("TR_AuditoriaRestaurante"));
        }
    }

    // Interface para injeção de dependência
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObterPorEmailAsync(string email);
        Task<Usuario?> ObterPorIdAsync(int id);
        Task AdicionarAsync(Usuario usuario);
    }

    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly BuillonContext _context;
        public UsuarioRepositorio(BuillonContext context) => _context = context;
        public async Task<Usuario?> ObterPorEmailAsync(string email) => await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        public async Task<Usuario?> ObterPorIdAsync(int id) => await _context.Usuarios.FindAsync(id);
        public async Task AdicionarAsync(Usuario usuario) { _context.Usuarios.Add(usuario); await _context.SaveChangesAsync(); }
    }

    public interface IRestauranteRepositorio
    {
        Task<List<Restaurante>> ListarPorAreaAsync(string? cidade, string? bairro, int pagina, int tamanho);
        Task<List<Restaurante>> BuscarDestaquesPorProcedureAsync(string cidade, decimal notaMinima);
        Task<Restaurante?> ObterCompletoPorIdAsync(int id);
        Task<Restaurante?> ObterPorProprietarioIdAsync(int proprietarioId);
        Task AdicionarAsync(Restaurante restaurante);
        Task AtualizarAsync(Restaurante restaurante);
    }

    public class RestauranteRepositorio : IRestauranteRepositorio
    {
        private readonly BuillonContext _context;
        public RestauranteRepositorio(BuillonContext context) => _context = context;

        // Traz os restaurantes com fotos, aplicando paginação para não sobrecarregar a memória
        public async Task<List<Restaurante>> ListarPorAreaAsync(string? cidade, string? bairro, int pagina, int tamanho)
        {
            var query = _context.Restaurantes.Include(r => r.Fotos).AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(cidade)) query = query.Where(r => r.Cidade.Contains(cidade));
            return await query.OrderByDescending(r => r.NotaMedia).Skip((pagina - 1) * tamanho).Take(tamanho).ToListAsync();
        }

        // Executa uma Stored Procedure diretamente no SQL Server para buscar destaques
        public async Task<List<Restaurante>> BuscarDestaquesPorProcedureAsync(string cidade, decimal notaMinima)
        {
            return await _context.Restaurantes.FromSql($"EXEC sp_BuscarRestaurantesDestaque @Cidade = {cidade}, @NotaMinima = {notaMinima}").ToListAsync();
        }

        public async Task<Restaurante?> ObterCompletoPorIdAsync(int id) => await _context.Restaurantes.Include(r => r.Fotos).FirstOrDefaultAsync(r => r.Id == id);
        public async Task<Restaurante?> ObterPorProprietarioIdAsync(int proprietarioId) => await _context.Restaurantes.FirstOrDefaultAsync(r => r.ProprietarioId == proprietarioId);
        public async Task AdicionarAsync(Restaurante restaurante) { _context.Restaurantes.Add(restaurante); await _context.SaveChangesAsync(); }
        public async Task AtualizarAsync(Restaurante restaurante) { _context.Restaurantes.Update(restaurante); await _context.SaveChangesAsync(); }
    }
}
