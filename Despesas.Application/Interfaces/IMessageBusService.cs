namespace Despesas.Application.Interfaces;

public interface IMessageBusService
{
    Task PublicarMensagemAsync(string fila, string mensagem);
}