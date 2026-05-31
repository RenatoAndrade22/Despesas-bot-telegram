namespace Despesas.Worker.Interfaces;

public interface IRelatorioProcessor
{
    Task ProcessarAsync(string mensagemJson, CancellationToken cancellationToken = default);
}