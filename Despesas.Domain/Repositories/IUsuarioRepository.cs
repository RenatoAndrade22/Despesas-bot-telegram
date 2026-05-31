using Despesas.Domain.Entities;

namespace Despesas.Domain.Repositories;

public interface IUsuarioRepository
{
    // Apenas assinaturas de métodos (o que deve ser feito, não o "como")
    Task<bool> ExistePorChatIdAsync(long chatId);
    Task AdicionarAsync(Usuario usuario);
}