using System.Text.Json.Serialization;

namespace Despesas.Application.DTOs.Telegram;

public class TelegramMessageDto
{
    [JsonPropertyName("from")]
    public TelegramUserDto? From { get; set; }

    [JsonPropertyName("chat")]
    public TelegramChatDto? Chat { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}