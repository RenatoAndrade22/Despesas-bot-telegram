namespace Despesas.Worker.Interfaces;

public interface ITelegramService
{
    Task EnviarMensagemAsync(long chatId, string texto);
}