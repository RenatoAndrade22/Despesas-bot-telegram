using Despesas.Application.DTOs.Telegram;

namespace Despesas.Application.Interfaces;

public interface ITelegramWebhookService
{
    Task ProcessarMensagemAsync(TelegramUpdateDto update);
}