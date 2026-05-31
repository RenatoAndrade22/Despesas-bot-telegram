using System;

namespace Despesas.Domain.Entities;

public enum StatusMensagem
{
    Processando,
    Concluido,
    Erro
}

public class ControleMensagem
{
    public Guid Id { get; set; }
    public long TelegramUpdateId { get; set; }
    public long TelegramChatId { get; set; }
    public string TextoOriginal { get; set; } = string.Empty;

    public StatusMensagem Status { get; set; } = StatusMensagem.Processando;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}