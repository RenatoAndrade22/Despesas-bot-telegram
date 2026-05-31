using Despesas.Worker.Interfaces;
using Microsoft.Extensions.Configuration; // Adicione se não estiver no global usings
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Despesas.Worker.Services;

public class GroqService : IGroqService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GroqService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GroqSettings:ApiKey"] ?? throw new ArgumentNullException("Groq ApiKey ausente");
        _model = configuration["GroqSettings:Model"] ?? "llama-3.3-70b-versatile";

        _httpClient.BaseAddress = new Uri("https://api.groq.com/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> DescobrirIntencaoAsync(string textoUsuario)
    {
        var systemPrompt = @"Você é um roteador de mensagens de um sistema financeiro.
Sua única função é classificar a intenção da mensagem do usuário.
As únicas opções válidas são: 'Despesa', 'Relatorio' ou 'Outros'.
Responda APENAS com um objeto JSON válido contendo a propriedade 'intencao'.";

        var iaResponse = await EnviarRequisicaoGroqAsync(systemPrompt, textoUsuario);

        using var resultDoc = JsonDocument.Parse(iaResponse);
        return resultDoc.RootElement.TryGetProperty("intencao", out var intencaoElement)
            ? intencaoElement.GetString() ?? "Outros"
            : "Outros";
    }

    public async Task<string> ExtrairDespesaAsync(string textoUsuario, string fornecedoresContexto)
    {
        var systemPrompt = $@"Você é um assistente financeiro especialista em extração de dados.
Sua função é ler a despesa do usuário e devolver um JSON estruturado.

Lista de fornecedores válidos cadastrados no banco de dados:{fornecedoresContexto}

Responda APENAS com um objeto JSON com as propriedades:
'valor' (decimal, use ponto para decimais),
'idFornecedor' (inteiro, retorne o ID correspondente da lista acima. Se o fornecedor não estiver na lista ou não for mencionado, retorne null).";

        return await EnviarRequisicaoGroqAsync(systemPrompt, textoUsuario);
    }

    public async Task<string> ExtrairParametrosRelatorioAsync(string textoUsuario, string dataAtual, string fornecedoresContexto)
    {
        var systemPrompt = $@"Você é um assistente de extração de dados para relatórios financeiros.
A data atual do sistema é: {dataAtual}. Baseie-se nesta data para entender termos como 'hoje', 'ontem', 'este mês', 'semana passada'.

Lista de fornecedores válidos:{fornecedoresContexto}

Responda APENAS com um objeto JSON com as propriedades:
'dataInicio' (string no formato YYYY-MM-DD),
'dataFim' (string no formato YYYY-MM-DD),
'idFornecedor' (inteiro, retorne o ID correspondente da lista acima se o usuário quiser filtrar por um fornecedor específico. Caso contrário, retorne null).";

        return await EnviarRequisicaoGroqAsync(systemPrompt, textoUsuario);
    }

    // --- Método Privado: Centraliza a comunicação HTTP e evita repetição de código ---

    private async Task<string> EnviarRequisicaoGroqAsync(string systemPrompt, string textoUsuario)
    {
        var payload = new
        {
            model = _model,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = textoUsuario }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("openai/v1/chat/completions", jsonContent);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erro da API Groq: {responseString}");

        using var document = JsonDocument.Parse(responseString);
        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";
    }
}