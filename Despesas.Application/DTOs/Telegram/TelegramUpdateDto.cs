using System.Text.Json.Serialization;

namespace Despesas.Application.DTOs.Telegram;

public class TelegramUpdateDto
{
    [JsonPropertyName("update_id")]
    public long UpdateId { get; set; }

    [JsonPropertyName("message")]
    public TelegramMessageDto? Message { get; set; }
}