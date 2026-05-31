namespace Despesas.Application.Interfaces;

public interface ITelegramBotClient
{
    Task EnviarMensagemAsync(long chatId, string mensagem);
}