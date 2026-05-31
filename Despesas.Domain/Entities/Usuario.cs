namespace Despesas.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public long TelegramChatId { get; set; } 
}