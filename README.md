# 🚀 Despesas Bot

O **Despesas Bot** é um sistema de gestão financeira pessoal inteligente, automatizado e focado em privacidade. Ele permite que você registre despesas e gere relatórios financeiros diretamente pelo **Telegram**, utilizando Inteligência Artificial para interpretar mensagens de voz ou texto de forma natural.

## 🛠️ O que o sistema faz?

* **Processamento Natural de Linguagem:** Esqueça formulários complexos. Basta digitar "Gastei 50 reais no almoço hoje" ou enviar um áudio, e a IA extrai automaticamente a categoria, o valor e a data.
* **Arquitetura Orientada a Eventos:** Utiliza **RabbitMQ** para processar as mensagens de forma assíncrona, garantindo que o seu bot nunca "trave" mesmo sob carga.
* **Gestão de Erros Profissional:** Implementa filas de *Dead Letter* (DLQ), garantindo que nenhuma informação seja perdida caso ocorra alguma falha no processamento.
* **Idempotência:** Proteção contra mensagens duplicadas, garantindo que sua conta não seja lançada duas vezes.
* **Pronto para o seu servidor:** Desenvolvido em .NET 8, fácil de rodar em containers ou servidores Linux.

---

## ⚙️ Como configurar e rodar

Este projeto utiliza **.NET 8**, **PostgreSQL** e **RabbitMQ**. Siga os passos abaixo:

### 1. Pré-requisitos
* [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado.
* Docker instalado (recomendado para rodar RabbitMQ e PostgreSQL facilmente).

### 2. Clonando e preparando
git clone [URL-DO-SEU-REPOSITORIO]
cd [NOME-DO-PROJETO]

### 3. Configurando o Ambiente
Como medida de segurança, este projeto não utiliza arquivos de configuração versionados.
1. Na pasta de cada projeto (`Gateway` e `Worker`), crie um arquivo chamado `appsettings.json` baseado no `appsettings.example.json`.
2. Preencha as credenciais:
    * **Telegram Bot Token:** Obtido através do @BotFather.
    * **RabbitMQ:** Host, usuário e senha.
    * **PostgreSQL:** Connection String.

### 4. Rodando os serviços
Certifique-se de que o RabbitMQ e o PostgreSQL estejam rodando. Em seguida, inicie os serviços:


# Rodar o Gateway (API de Webhooks)
dotnet run --project Despesas.Gateway

# Rodar os Workers de processamento
dotnet run --project Despesas.Worker


# Como contribuir
Contribuições são muito bem-vindas! Sinta-se à vontade para abrir uma Issue ou enviar um Pull Request para melhorar a IA, adicionar novas integrações de banco de dados ou otimizar o processamento das filas.
