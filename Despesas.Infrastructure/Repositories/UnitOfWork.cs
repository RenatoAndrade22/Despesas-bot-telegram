using Microsoft.EntityFrameworkCore.Storage;
using Despesas.Domain.Repositories;
using Despesas.Infrastructure.Data;

namespace Despesas.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            // Dispara o INSERT para o banco e commita a transação
            await _context.SaveChangesAsync();
            if (_transaction != null) await _transaction.CommitAsync();
        }
        finally
        {
            if (_transaction != null) await _transaction.DisposeAsync();
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            if (_transaction != null) await _transaction.RollbackAsync();
        }
        finally
        {
            if (_transaction != null) await _transaction.DisposeAsync();
        }
    }
}