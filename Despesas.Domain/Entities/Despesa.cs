using System;

namespace Despesas.Domain.Entities;

public class Despesa
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }
}