using Despesas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Despesas.Infrastructure.Data;

public class AppDbContext : DbContext
{
    // O construtor recebe as configurações (como a string de conexão) e repassa para a classe base do EF Core
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Os DbSets representam as tabelas que serão criadas no banco de dados
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<Despesa> Despesas { get; set; }
    public DbSet<ControleMensagem> ControleMensagens { get; set; }
    public DbSet<Agendamento> Agendamentos { get; set; }
}