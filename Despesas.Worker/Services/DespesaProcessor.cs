using Despesas.Domain.Entities;
using Despesas.Infrastructure.Data;
using Despesas.Worker.DTOs;
using Despesas.Worker.Interfaces;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class DespesaProcessor : IDespesaProcessor
{
    private readonly AppDbContext _dbContext;
    private readonly IGroqService _groqService;
    private readonly ITelegramService _telegramService;

    public DespesaProcessor(AppDbContext dbContext, IGroqService groqService, ITelegramService telegramService)
    {
        _dbContext = dbContext;
        _groqService = groqService;
        _telegramService = telegramService;
    }

    public async Task ProcessarAsync(string mensagemJson)
    {
        using var document = JsonDocument.Parse(mensagemJson);
        var root = document.RootElement;
        var chatId = root.GetProperty("message").GetProperty("chat").GetProperty("id").GetInt64();
        var texto = root.GetProperty("message").GetProperty("text").GetString()!;

        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.TelegramChatId == chatId);
        if (usuario == null)
        {
            await _telegramService.EnviarMensagemAsync(chatId, "⚠️ Usuário não cadastrado.");
            return;
        }

        var jsonDaIA = await _groqService.ExtrairDespesaAsync(texto, await ObterContextoFornecedores());
        var despesaExtraida = JsonSerializer.Deserialize<DespesaGroqDto>(jsonDaIA, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (despesaExtraida?.Valor > 0)
        {
            ValidarFornecedor(despesaExtraida);
            await SalvarDespesaAsync(despesaExtraida, usuario.Id);
            await _telegramService.EnviarMensagemAsync(chatId, $"✅ Despesa de R$ {despesaExtraida.Valor} registrada!");
        }
    }

    private async Task<string> ObterContextoFornecedores() =>
        string.Join("\n", await _dbContext.Fornecedores.Select(f => $"ID: {f.Id} - Nome: {f.Nome}").ToListAsync());

    private void ValidarFornecedor(DespesaGroqDto dto)
    {
        if (dto.IdFornecedor.HasValue && !_dbContext.Fornecedores.Any(f => f.Id == dto.IdFornecedor))
            dto.IdFornecedor = null;
    }

    private async Task SalvarDespesaAsync(DespesaGroqDto dto, int usuarioId)
    {
        _dbContext.Despesas.Add(new Despesa { Valor = dto.Valor, FornecedorId = dto.IdFornecedor, UsuarioId = usuarioId, DataCriacao = DateTime.UtcNow });
        await _dbContext.SaveChangesAsync();
    }
}