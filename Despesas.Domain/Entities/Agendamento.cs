namespace Despesas.Domain.Entities;

public class Agendamento
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public int DiaVencimento { get; set; }

    public bool Ativo { get; set; } = true;

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }
}