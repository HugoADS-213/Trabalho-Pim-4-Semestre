using Buillon.Modelos;

namespace Buillon.Api.Dtos
{
    // DTOs de Entrada: Garantem que a API receba apenas os campos exatos necessários (evita injeção de dados)
    public record RegistoUsuarioDto(string Nome, string Email, string Senha, string Role);
    public record LoginDto(string Email, string Senha);
    
    public record SalvarRestauranteDto(string Nome, string Descricao, string EnderecoCompleto, string Bairro, string Cidade, string Telefone);
    public record NovoItemCardapioDto(string NomePrato, string Descricao, decimal Preco);
    public record NovaFotoDto(string UrlFoto, string Legenda);
    public record NovaAvaliacaoDto(int Nota, string Comentario);

    // DTOs de Saída: Ocultam dados sensíveis (como SenhaHash e ProprietarioId) ao devolver os dados para a tela
    public record ItemCardapioDto(int Id, string NomePrato, string Descricao, decimal Preco);
    public record FotoDto(int Id, string UrlFoto, string Legenda);
    public record AvaliacaoDto(int Id, string NomeUsuario, int Nota, string Comentario, DateTime Data);

    public record RestauranteDto(
        int Id, string Nome, string Descricao, string EnderecoCompleto, string Bairro, 
        string Cidade, string Telefone, decimal NotaMedia, int ProprietarioId,
        List<ItemCardapioDto> Cardapio, List<FotoDto> Fotos, List<AvaliacaoDto> Avaliacoes)
    {
        // Converte a classe pesada de Banco de Dados (Restaurante) para a versão limpa de Tela (DTO)
        public static RestauranteDto De(Restaurante r) => new(
            r.Id, r.Nome, r.Descricao, r.EnderecoCompleto, r.Bairro, r.Cidade, r.Telefone, r.NotaMedia, r.ProprietarioId,
            r.Cardapio.Select(c => new ItemCardapioDto(c.Id, c.NomePrato, c.Descricao, c.Preco)).ToList(),
            r.Fotos.Select(f => new FotoDto(f.Id, f.UrlFoto, f.Legenda)).ToList(),
            r.Avaliacoes.Select(a => new AvaliacaoDto(a.Id, a.Usuario?.Nome ?? "Cliente", a.Nota, a.Comentario, a.DataCriacao)).ToList()
        );
    }
}
