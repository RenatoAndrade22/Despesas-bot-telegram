using Microsoft.EntityFrameworkCore;
using Despesas.Domain.Entities;
using Despesas.Domain.Repositories;
using Despesas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
namespace Despesas.Infrastructure.Repositories;

public class ControleMensagemRepository : IControleMensagemRepository
{
    private readonly AppDbContext _context;

    public ControleMensagemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExisteUpdateIdAsync(long telegramUpdateId)
    {
        return await _context.Set<ControleMensagem>()
                             .AnyAsync(x => x.TelegramUpdateId == telegramUpdateId);
    }

    public async Task AdicionarAsync(ControleMensagem mensagem)
    {
        await _context.Set<ControleMensagem>().AddAsync(mensagem);
    }

    public async Task<IDbContextTransaction> IniciarTransacaoAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }
}