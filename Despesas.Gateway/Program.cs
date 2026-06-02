using Despesas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Despesas.Application.Services;
using Despesas.Domain.Repositories;
using Despesas.Infrastructure.Repositories;
using Despesas.Application.Interfaces;
using Despesas.Infrastructure.ExternalServices;
using Despesas.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Registar o AppDbContext no Contentor de Injeção de Dependência
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Despesas.Infrastructure")));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddHttpClient<ITelegramBotClient, TelegramBotClient>();
builder.Services.AddScoped<IMessageBusService, RabbitMqService>();
builder.Services.AddScoped<IControleMensagemRepository, ControleMensagemRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITelegramWebhookService, TelegramWebhookService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        DbSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao popular o banco de dados.");
    }
}

app.Run();
