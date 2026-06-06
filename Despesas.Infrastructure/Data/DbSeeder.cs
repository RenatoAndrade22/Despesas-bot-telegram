using Despesas.Domain.Entities;
using Despesas.Infrastructure.Data;

namespace Despesas.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext dbContext)
    {
        // Garante que o banco exista e as tabelas estejam criadas
        dbContext.Database.EnsureCreated();

        // Só insere se não houver fornecedores cadastrados
        if (!dbContext.Fornecedores.Any())
        {
            var fornecedoresIniciais = new List<Fornecedor>
            {
                new Fornecedor { Nome = "Imposto" },
                new Fornecedor { Nome = "Dentista" },
                new Fornecedor { Nome = "Dental" },
                new Fornecedor { Nome = "Protese | Laboratorio" },
                new Fornecedor { Nome = "Scretária" },
                new Fornecedor { Nome = "Supermercado | Mercado" },
                new Fornecedor { Nome = "gasolina | alcool | posto" },
                new Fornecedor { Nome = "Farmácia" },
                new Fornecedor { Nome = "Padaria" },
                new Fornecedor { Nome = "Restaurante" },
                new Fornecedor { Nome = "Ifood" },
                new Fornecedor { Nome = "Uber" },
                new Fornecedor { Nome = "Energia Elétrica | Luz | Energia" },
                new Fornecedor { Nome = "Água" },
                new Fornecedor { Nome = "Internet" }
            };

            dbContext.Fornecedores.AddRange(fornecedoresIniciais);
            dbContext.SaveChanges();
        }
    }
}