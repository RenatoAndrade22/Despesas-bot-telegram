using System.Text.Json;
using Despesas.Application.DTOs.Telegram;
using Despesas.Application.Interfaces;
using Despesas.Domain.Entities;
using Despesas.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Despesas.Application.Services;

public class TelegramWebhookService : ITelegramWebhookService
{
    private readonly IControleMensagemRepository _mensagemRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMessageBusService _messageBus;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TelegramWebhookService> _logger;
    private readonly string _senhaConfigurada;

    public TelegramWebhookService(
        IControleMensagemRepository mensagemRepository,
        IUsuarioRepository usuarioRepository,
        IMessageBusService messageBus,
        ITelegramBotClient telegramBotClient,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<TelegramWebhookService> logger)
    {
        _mensagemRepository = mensagemRepository;
        _usuarioRepository = usuarioRepository;
        _messageBus = messageBus;
        _telegramBotClient = telegramBotClient;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
        _senhaConfigurada = configuration["TelegramSettings:SenhaRegistro"]
                            ?? throw new ArgumentNullException("SenhaRegistro não configurada.");
    }

    public async Task ProcessarMensagemAsync(TelegramUpdateDto update)
    {
        if (string.IsNullOrWhiteSpace(update?.Message?.Text))
            return;

        // Idempotência de Sucesso (Garante que mensagens processadas corretamente não dupliquem)
        if (await _mensagemRepository.ExisteUpdateIdAsync(update.UpdateId))
            return;

        // Autorização e Registro de Usuários
        var isAutorizado = await ValidarETratarUsuarioAsync(update.Message);

        // Não autorizado
        if (!isAutorizado)
            return;

        // Tenta processar com Idempotência de Erro
        await SalvarEEncaminharMensagemAsync(update);
    }

    private async Task<bool> ValidarETratarUsuarioAsync(TelegramMessageDto message)
    {
        var chatId = message.Chat.Id;

        if (await _usuarioRepository.ExistePorChatIdAsync(chatId))
            return true; // Usuário existe, pode prosseguir.

        // Usuário não existe, verificar se é tentativa de registro.
        var texto = message.Text.Trim();
        var comandoEsperado = $"/registrar {_senhaConfigurada}";

        if (texto == comandoEsperado)
        {
            var nome = message.From?.FirstName ?? "Usuário";

            var novoUsuario = new Usuario
            {
                Nome = nome,
                TelegramChatId = chatId
            };

            await _usuarioRepository.AdicionarAsync(novoUsuario);
            await _telegramBotClient.EnviarMensagemAsync(chatId, $"✅ *Cadastro realizado com sucesso!*\nOlá {nome}, agora você está autorizado a lançar despesas e pedir relatórios.");
        }
        else
        {
            await _telegramBotClient.EnviarMensagemAsync(chatId, "❌ Acesso Negado. Você não tem permissão para usar este sistema. Por favor, envie o comando de registro.");
        }

        return false;
    }

    private async Task SalvarEEncaminharMensagemAsync(TelegramUpdateDto update)
    {
        var controleMensagem = new ControleMensagem
        {
            TelegramUpdateId = update.UpdateId,
            TelegramChatId = update.Message.Chat.Id,
            TextoOriginal = update.Message.Text,
            Status = StatusMensagem.Processando,
            DataCriacao = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _mensagemRepository.AdicionarAsync(controleMensagem);

            var mensagemJson = JsonSerializer.Serialize(update);
            await _messageBus.PublicarMensagemAsync("telegram_messages", mensagemJson);

            await _unitOfWork.CommitAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();

            // Cria uma chave única para esse erro usando o ID da mensagem do Telegram
            var cacheKey = $"error_notified_{update.UpdateId}";

            // Verifica no cache se o usuário JÁ FOI avisado sobre a falha nesta requisição específica
            if (!_cache.TryGetValue(cacheKey, out _))
            {
                await _telegramBotClient.EnviarMensagemAsync(
                    update.Message.Chat.Id,
                    "❌ Erro ao processar. Tente novamente em alguns minutos.");

                // Salva na memória por 30 minutos que esse aviso já foi dado
                _cache.Set(cacheKey, true, TimeSpan.FromMinutes(30));
            }

            _logger.LogError(ex, "Falha crítica ao processar o webhook do Telegram. UpdateId: {UpdateId}", update.UpdateId);
        }
    }
}