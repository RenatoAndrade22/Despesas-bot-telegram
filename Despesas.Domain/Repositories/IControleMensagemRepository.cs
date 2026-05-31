using Despesas.Domain.Entities;

namespace Despesas.Domain.Repositories;

public interface IControleMensagemRepository
{
    Task<bool> ExisteUpdateIdAsync(long telegramUpdateId);
    Task AdicionarAsync(ControleMensagem mensagem);
}