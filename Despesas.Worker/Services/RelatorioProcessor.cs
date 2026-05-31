using System.Text.Json;
using Despesas.Domain.Entities;
using Despesas.Infrastructure.Data;
using Despesas.Worker.Interfaces;
using Despesas.Worker.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Despesas.Worker.Services;

public class RelatorioProcessor : IRelatorioProcessor
{
    private readonly AppDbContext _dbContext;
    private readonly IGroqService _groqService;
    private readonly ITelegramService _telegramService;
    private readonly ILogger<RelatorioProcessor> _logger;

    public RelatorioProcessor(
        AppDbContext dbContext,
        IGroqService groqService,
        ITelegramService telegramService,
        ILogger<RelatorioProcessor> logger)
    {
        _dbContext = dbContext;
        _groqService = groqService;
        _telegramService = telegramService;
        _logger = logger;
    }

    public async Task ProcessarAsync(string mensagemJson, CancellationToken cancellationToken = default)
    {
        // 1. Parse inicial da mensagem do Telegram
        using var document = JsonDocument.Parse(mensagemJson);
        var root = document.RootElement;

        var textoUsuario = root.GetProperty("message").TryGetProperty("text", out var textElement)
            ? textElement.GetString() ?? string.Empty
            : string.Empty;

        var chatId = root.GetProperty("message").GetProperty("chat").GetProperty("id").GetInt64();

        // 2. Validação de Usuário
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.TelegramChatId == chatId, cancellationToken);
        if (usuario == null)
        {
            await _telegramService.EnviarMensagemAsync(chatId, "⚠️ Ops! Não encontrei o seu cadastro no sistema para gerar o relatório.");
            return;
        }

        // 3. Extração via IA (Groq)
        var contextoFornecedores = await ObterContextoFornecedoresAsync(cancellationToken);
        var dataAtual = DateTime.Now.ToString("dd/MM/yyyy");

        var jsonDaIA = await _groqService.ExtrairParametrosRelatorioAsync(textoUsuario, dataAtual, contextoFornecedores);
        _logger.LogInformation("Parâmetros do relatório (Groq): {json}", jsonDaIA);

        var parametros = JsonSerializer.Deserialize<ParametrosRelatorioDto>(jsonDaIA, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // 4. Execução da Regra de Negócio
        if (parametros != null)
        {
            await ProcessarEEnviarRelatorioAsync(parametros, usuario.Id, chatId, cancellationToken);
        }
    }

    // --- Métodos Privados (SRP) ---

    private async Task<string> ObterContextoFornecedoresAsync(CancellationToken cancellationToken)
    {
        var lista = await _dbContext.Fornecedores
            .Select(f => $"ID: {f.Id} - Nome: {f.Nome}")
            .ToListAsync(cancellationToken);

        return string.Join("\n", lista);
    }

    private async Task ProcessarEEnviarRelatorioAsync(ParametrosRelatorioDto parametros, int usuarioId, long chatId, CancellationToken cancellationToken)
    {
        // Proteção contra alucinação de fornecedor
        if (parametros.IdFornecedor.HasValue)
        {
            var fornecedorExiste = await _dbContext.Fornecedores.AnyAsync(f => f.Id == parametros.IdFornecedor.Value, cancellationToken);
            if (!fornecedorExiste)
            {
                parametros.IdFornecedor = null;
            }
        }

        // Ajuste de Fuso Horário (PostgreSQL UTC)
        var dataInicioUtc = DateTime.SpecifyKind(parametros.DataInicio, DateTimeKind.Utc);
        var dataFimUtc = DateTime.SpecifyKind(parametros.DataFim.AddHours(23).AddMinutes(59).AddSeconds(59), DateTimeKind.Utc);

        // Consulta Segura
        var query = _dbContext.Despesas.AsQueryable()
            .Where(d => d.UsuarioId == usuarioId &&
                        d.DataCriacao >= dataInicioUtc &&
                        d.DataCriacao <= dataFimUtc);

        if (parametros.IdFornecedor.HasValue)
        {
            query = query.Where(d => d.FornecedorId == parametros.IdFornecedor.Value);
        }

        var totalGasto = await query.SumAsync(d => d.Valor, cancellationToken);
        var quantidadeItens = await query.CountAsync(cancellationToken);

        // Formatação e Envio
        var msgRelatorio = $"📊 <b>Relatório Financeiro</b>\n\n" +
                           $"Período: {parametros.DataInicio:dd/MM/yyyy} até {parametros.DataFim:dd/MM/yyyy}\n" +
                           $"Lançamentos: {quantidadeItens}\n" +
                           $"<b>Total Gasto: R$ {totalGasto:F2}</b>";

        await _telegramService.EnviarMensagemAsync(chatId, msgRelatorio);
    }
}
