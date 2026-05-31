using Despesas.Application.DTOs.Telegram;
using Despesas.Application.Interfaces;

namespace Despesas.Application.Interfaces;
public interface ITelegramService
{
    Task ProcessarMensagemAsync(TelegramUpdateDto update);
}