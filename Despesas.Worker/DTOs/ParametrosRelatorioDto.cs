namespace Despesas.Worker.DTOs;

public class ParametrosRelatorioDto
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int? IdFornecedor { get; set; }
}