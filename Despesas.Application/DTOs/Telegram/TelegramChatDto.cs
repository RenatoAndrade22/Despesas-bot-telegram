using System.Text.Json.Serialization;

namespace Despesas.Application.DTOs.Telegram;

public class TelegramChatDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}