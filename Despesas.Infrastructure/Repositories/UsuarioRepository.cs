using Despesas.Domain.Entities;
using Despesas.Domain.Repositories;
using Despesas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Despesas.Infrastructure.Repositories;

// A classe assina o contrato com os dois pontos: IUsuarioRepository
public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistePorChatIdAsync(long chatId)
    {
        // Aqui usamos o poder do Entity Framework
        return await _context.Usuarios.AnyAsync(u => u.TelegramChatId == chatId);
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        // Salva de fato no banco de dados
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }
}