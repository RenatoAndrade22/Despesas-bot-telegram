using System.Text;
using System.Text.Json;
using Despesas.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Despesas.Infrastructure.ExternalServices;

public class TelegramBotClient : ITelegramBotClient
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;

    public TelegramBotClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _botToken = configuration["TelegramSettings:BotToken"];
    }

    public async Task EnviarMensagemAsync(long chatId, string mensagem)
    {
        var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";

        // Montamos o corpo da requisição exatamente como a API do Telegram espera
        var payload = new
        {
            chat_id = chatId,
            text = mensagem,
            parse_mode = "Markdown" // Permite usar negritos e itálicos nas respostas
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);

        // Se o Telegram retornar algum erro (400, 401, etc), isso vai disparar uma exceção
        response.EnsureSuccessStatusCode();
    }
}