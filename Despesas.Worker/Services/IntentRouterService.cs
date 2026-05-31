using System.Text.Json;
using Despesas.Worker.Interfaces;

namespace Despesas.Worker.Services;

public class IntentRouterService : IIntentRouterService
{
    private readonly IGroqService _groqService;
    private readonly ILogger<IntentRouterService> _logger;

    public IntentRouterService(IGroqService groqService, ILogger<IntentRouterService> logger)
    {
        _groqService = groqService;
        _logger = logger;
    }

    public async Task<string?> DeterminarFilaDestinoAsync(string mensagemJson)
    {
        using var document = JsonDocument.Parse(mensagemJson);
        var root = document.RootElement;

        // Extração isolada da estrutura do Telegram
        var textoUsuario = root.GetProperty("message").TryGetProperty("text", out var textElement)
            ? textElement.GetString()
            : string.Empty;

        if (string.IsNullOrWhiteSpace(textoUsuario))
        {
            _logger.LogWarning("Mensagem sem texto recebida. Descartando.");
            return null;
        }

        // Delegação para a IA
        var intencao = await _groqService.DescobrirIntencaoAsync(textoUsuario);
        _logger.LogInformation("Intenção identificada: {intencao}", intencao);

        return intencao switch
        {
            "Despesa" => "processar_despesas",
            "Relatorio" => "processar_relatorios",
            _ => null
        };
    }
}