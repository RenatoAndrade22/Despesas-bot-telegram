using System.Text.Json.Serialization;

namespace Despesas.Application.DTOs.Telegram;

public class TelegramUserDto
{
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }
}