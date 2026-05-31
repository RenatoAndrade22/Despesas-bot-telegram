using System.Text;
using Despesas.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Despesas.Infrastructure.Services;

public class RabbitMqService : IMessageBusService
{
    private readonly string _hostName;
    private readonly string _userName;
    private readonly string _password;

    public RabbitMqService(IConfiguration configuration)
    {
        // PONTO DE ATENÇÃO: No Worker nós configuramos como "RabbitMq:Host". 
        // Verifique no appsettings.json do Gateway se está "RabbitMqSettings" ou só "RabbitMq".
        _hostName = configuration["RabbitMqSettings:HostName"] ?? "localhost";
        _userName = configuration["RabbitMqSettings:UserName"] ?? "guest";
        _password = configuration["RabbitMqSettings:Password"] ?? "guest";
    }

    public async Task PublicarMensagemAsync(string fila, string mensagem)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostName,
            UserName = _userName,
            Password = _password
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        // 1. Monta os mesmos argumentos que o Worker usa para não dar conflito
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "dead_letter_exchange" },
            { "x-dead-letter-routing-key", $"{fila}_dlq" }
        };

        // 2. Declara a fila passando os argumentos em vez de 'null'
        await channel.QueueDeclareAsync(
            queue: fila,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args);

        var body = Encoding.UTF8.GetBytes(mensagem);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: fila,
            body: body);
    }
}