using System.Text;
using System.Text.Json;
using Despesas.Worker.Interfaces;
namespace Despesas.Worker.Services;

public class TelegramService : ITelegramService
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;

    public TelegramService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _botToken = configuration["TelegramSettings:BotToken"] ?? throw new ArgumentNullException("BotToken ausente");

        _httpClient.BaseAddress = new Uri($"https://api.telegram.org/bot{_botToken}/");
    }

    public async Task EnviarMensagemAsync(long chatId, string texto)
    {
        var payload = new
        {
            chat_id = chatId,
            text = texto,
            parse_mode = "HTML" 
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("sendMessage", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            throw new Exception($"Erro ao enviar mensagem pro Telegram: {erro}");
        }
    }
}