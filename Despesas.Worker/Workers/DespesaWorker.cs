using Despesas.Worker.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Despesas.Worker.Workers;

public class DespesaWorker : BackgroundService
{
    private readonly ILogger<DespesaWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    // Constante para evitar erros de digitação (Magic Strings)
    private const string FilaDespesas = "processar_despesas";

    public DespesaWorker(
        ILogger<DespesaWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Worker de Despesas...");

        // Buscando credenciais do appsettings.json
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"] ?? "localhost",
            UserName = _configuration["RabbitMq:User"] ?? "guest",
            Password = _configuration["RabbitMq:Password"] ?? "guest"
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IDespesaProcessor>();

                await processor.ProcessarAsync(Encoding.UTF8.GetString(ea.Body.ToArray()));

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no processamento da despesa.");
                // O requeue: false descarta a mensagem em caso de erro fatal para evitar loop infinito
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(queue: FilaDespesas, autoAck: false, consumer: consumer);

        // Mantém o Worker vivo escutando a fila de forma assíncrona
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}