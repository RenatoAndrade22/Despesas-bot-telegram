using Despesas.Worker;
using Despesas.Worker.Interfaces;
using Despesas.Worker.Services;
using Despesas.Worker.Workers;
using Despesas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore; 

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Serviços
builder.Services.AddHttpClient<IGroqService, GroqService>();
builder.Services.AddHttpClient<ITelegramService, TelegramService>();
builder.Services.AddScoped<IIntentRouterService, IntentRouterService>();
builder.Services.AddScoped<IDespesaProcessor, DespesaProcessor>();
builder.Services.AddScoped<IRelatorioProcessor, RelatorioProcessor>();

// Workers
builder.Services.AddHostedService<TelegramRouterWorker>();
builder.Services.AddHostedService<DespesaWorker>();
builder.Services.AddHostedService<RelatorioWorker>();

var host = builder.Build();
host.Run();