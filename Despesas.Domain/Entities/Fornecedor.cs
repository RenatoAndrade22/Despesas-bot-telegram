namespace Despesas.Domain.Entities;

public class Fornecedor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
}