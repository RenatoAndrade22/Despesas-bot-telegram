using Microsoft.AspNetCore.Mvc;
using Despesas.Application.DTOs.Telegram;
using Despesas.Application.Interfaces;

namespace Despesas.Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramWebhookService _webhookService;

    public TelegramController(ITelegramWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceberMensagem([FromBody] TelegramUpdateDto update)
    {
        try
        {
            await _webhookService.ProcessarMensagemAsync(update);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro crítico: {ex.Message}");
            return StatusCode(500);
        }
    }
}