using System.ComponentModel.DataAnnotations;

namespace Buillon.Modelos
{
    // Classe base que todas as entidades herdam para ter um Id e Data de Criação padronizados
    public abstract class EntidadeBase
    {
        public int Id { get; set; }
        public DateTime DataCriacao { get; protected set; } = DateTime.UtcNow;
        public abstract void Validar(); // Obriga todas as classes filhas a terem regras de validação
    }

    public class Usuario : EntidadeBase
    {
        [Required, MaxLength(100)] public string Nome { get; set; } = string.Empty;
        [Required, EmailAddress, MaxLength(150)] public string Email { get; set; } = string.Empty;
        [Required] public string SenhaHash { get; set; } = string.Empty; // Nunca guardamos a senha em texto limpo
        [Required, MaxLength(30)] public string Role { get; set; } = "Cliente"; // "Cliente" ou "Proprietario"

        public override void Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome)) throw new ArgumentException("Nome obrigatório.");
            if (Role != "Cliente" && Role != "Proprietario") throw new ArgumentException("Role inválida.");
        }
    }

    public class Restaurante : EntidadeBase
    {
        // Os 'private set' garantem que os dados só podem ser alterados através de métodos controlados
        [Required, MaxLength(120)] public string Nome { get; private set; } = string.Empty;
        [Required, MaxLength(1000)] public string Descricao { get; private set; } = string.Empty;
        [Required, MaxLength(250)] public string EnderecoCompleto { get; private set; } = string.Empty;
        [Required, MaxLength(100)] public string Bairro { get; private set; } = string.Empty;
        [Required, MaxLength(100)] public string Cidade { get; private set; } = string.Empty;
        [MaxLength(20)] public string Telefone { get; private set; } = string.Empty;
        
        public decimal NotaMedia { get; private set; } // Calculada automaticamente via Trigger no Banco
        
        // Relacionamento com o dono do restaurante
        public int ProprietarioId { get; private set; }
        public Usuario? Proprietario { get; private set; }

        // Relacionamentos 1 para N (Um restaurante tem vários pratos, fotos e avaliações)
        public ICollection<ItemCardapio> Cardapio { get; private set; } = new List<ItemCardapio>();
        public ICollection<FotoRestaurante> Fotos { get; private set; } = new List<FotoRestaurante>();
        public ICollection<Avaliacao> Avaliacoes { get; private set; } = new List<Avaliacao>();

        protected Restaurante() { } // Construtor vazio exigido pelo Entity Framework

        // Construtor usado na criação de um novo restaurante
        public Restaurante(string nome, string desc, string endereco, string bairro, string cidade, string tel, int proprietarioId)
        {
            ProprietarioId = proprietarioId;
            AtualizarPagina(nome, desc, endereco, bairro, cidade, tel);
        }

        // Método que concentra a regra de atualização dos dados
        public void AtualizarPagina(string nome, string desc, string endereco, string bairro, string cidade, string tel)
        {
            Nome = nome; Descricao = desc; EnderecoCompleto = endereco; Bairro = bairro; Cidade = cidade; Telefone = tel;
            Validar();
        }

        public override void Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome) || Nome.Length < 3) throw new ArgumentException("Nome inválido.");
        }
    }

    public class ItemCardapio : EntidadeBase
    {
        public int RestauranteId { get; set; }
        [Required, MaxLength(100)] public string NomePrato { get; set; } = string.Empty;
        [MaxLength(300)] public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }

        public override void Validar() { if (Preco <= 0) throw new ArgumentException("Preço inválido."); }
    }

    public class FotoRestaurante : EntidadeBase
    {
        public int RestauranteId { get; set; }
        [Required, MaxLength(500)] public string UrlFoto { get; set; } = string.Empty;
        [MaxLength(150)] public string Legenda { get; set; } = string.Empty;

        public override void Validar() { if (string.IsNullOrWhiteSpace(UrlFoto)) throw new ArgumentException("URL obrigatória."); }
    }

    public class Avaliacao : EntidadeBase
    {
        public int RestauranteId { get; set; }
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public int Nota { get; set; }
        [MaxLength(500)] public string Comentario { get; set; } = string.Empty;

        public override void Validar() { if (Nota < 1 || Nota > 5) throw new ArgumentException("Nota de 1 a 5."); }
    }
}
