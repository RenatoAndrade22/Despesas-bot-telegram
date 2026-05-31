namespace Despesas.Worker.Interfaces;

public interface IIntentRouterService
{
    Task<string?> DeterminarFilaDestinoAsync(string mensagemJson);
}