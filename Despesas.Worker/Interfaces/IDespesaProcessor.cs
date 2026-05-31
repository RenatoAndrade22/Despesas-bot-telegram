
namespace Despesas.Worker.Interfaces;
public interface IDespesaProcessor
{
    Task ProcessarAsync(string mensagemJson);
}