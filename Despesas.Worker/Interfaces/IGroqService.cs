namespace Despesas.Worker.Interfaces;

public interface IGroqService
{
    Task<string> DescobrirIntencaoAsync(string textoUsuario);
    Task<string> ExtrairDespesaAsync(string textoUsuario, string fornecedoresContexto);
    Task<string> ExtrairParametrosRelatorioAsync(string textoUsuario, string dataAtual, string fornecedoresContexto);
}