using Despesas.Worker.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Despesas.Worker.Workers;

public class RelatorioWorker : BackgroundService
{
    private readonly ILogger<RelatorioWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    // Constante para evitar erros de digitação (Magic Strings)
    private const string FilaRelatorios = "processar_relatorios";

    public RelatorioWorker(
        ILogger<RelatorioWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o Especialista em Relatórios...");

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
                var body = ea.Body.ToArray();
                var mensagemJson = Encoding.UTF8.GetString(body);

                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IRelatorioProcessor>();

                await processor.ProcessarAsync(mensagemJson, stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar o relatório.");
           
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        // Utilizando a constante da fila
        await channel.BasicConsumeAsync(queue: FilaRelatorios, autoAck: false, consumer: consumer);

        // Mantém o Worker vivo escutando a fila
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}