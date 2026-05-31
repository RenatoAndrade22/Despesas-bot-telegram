using Despesas.Worker.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Configuration; // Necessário para acessar o IConfiguration

namespace Despesas.Worker.Workers;

public class TelegramRouterWorker : BackgroundService
{
    private readonly ILogger<TelegramRouterWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private const string FilaTelegram = "telegram_messages";
    private const string FilaDespesas = "processar_despesas";
    private const string FilaRelatorios = "processar_relatorios";

    public TelegramRouterWorker(
        ILogger<TelegramRouterWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration) // Injeção adicionada
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Roteador de Mensagens...");

        // Lendo as credenciais diretamente do arquivo de configuração
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"] ?? "localhost",
            UserName = _configuration["RabbitMq:User"] ?? "guest",
            Password = _configuration["RabbitMq:Password"] ?? "guest"
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // Declaração de Filas
        await DeclararFilasAsync(channel);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var mensagemJson = Encoding.UTF8.GetString(body);

            try
            {
                using var scope = _serviceProvider.CreateScope();

                var routerService = scope.ServiceProvider.GetRequiredService<IIntentRouterService>();

                var routingKey = await routerService.DeterminarFilaDestinoAsync(mensagemJson);

                if (routingKey != null)
                {
                    await channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: routingKey,
                        body: body);

                    _logger.LogInformation("Mensagem encaminhada com sucesso para a fila: {fila}", routingKey);
                }
                else
                {
                    _logger.LogWarning("Fluxo interrompido: Intenção não reconhecida ou mensagem inválida.");
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha crítica ao processar a rota da mensagem.");
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(queue: "telegram_messages", autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private static async Task DeclararFilasAsync(IChannel channel)
    {

        await channel.ExchangeDeclareAsync("dead_letter_exchange", ExchangeType.Direct, durable: true);

        var argsTelegram = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" },
            { "x-dead-letter-routing-key", "telegram_messages_dlq" }
        };

        await channel.QueueDeclareAsync(FilaTelegram, durable: true, exclusive: false, autoDelete: false, arguments: argsTelegram);
        await channel.QueueDeclareAsync("telegram_messages_dlq", durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync("telegram_messages_dlq", "dead_letter_exchange", "telegram_messages_dlq");

        var argsDespesas = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" },
            { "x-dead-letter-routing-key", "processar_despesas_dlq" }
        };

        await channel.QueueDeclareAsync(FilaDespesas, durable: true, exclusive: false, autoDelete: false, arguments: argsDespesas);
        await channel.QueueDeclareAsync("processar_despesas_dlq", durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync("processar_despesas_dlq", "dead_letter_exchange", "processar_despesas_dlq");

        var argsRelatorios = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" },
            { "x-dead-letter-routing-key", "processar_relatorios_dlq" }
        };

        await channel.QueueDeclareAsync(FilaRelatorios, durable: true, exclusive: false, autoDelete: false, arguments: argsRelatorios);
        await channel.QueueDeclareAsync("processar_relatorios_dlq", durable: true, exclusive: false, autoDelete: false);
        await channel.QueueBindAsync("processar_relatorios_dlq", "dead_letter_exchange", "processar_relatorios_dlq");
    }
}