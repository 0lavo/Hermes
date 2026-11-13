# Hermes — Radar de Dúvidas em Tempo Real

Aplicação web para os alunos sinalizarem dúvidas durante a aula, em tempo real e de forma anónima para os colegas. O professor cria uma sala, os alunos entram com o código, submetem dúvidas e votam, e o professor acompanha tudo num dashboard.

**Equipa:** João Borges, Olavo Moreira, Diogo Pimenta, Maria Giulia Barbosa

Projeto da UC Laboratório de Programação (22356), ISLA Gaia, 2026/2027.

> Estado: Sprint 2 (codificação e testes) em curso. Este README é atualizado à medida que o projeto avança.

## Tecnologias

- Frontend: Vue + TypeScript
- Backend: C# + .NET 10 (API REST + SignalR para o tempo real)
- Base de dados: SQL Server (em Docker)
- Controlo de versões: Git + GitHub

## Estrutura do repositório

```
Hermes/
├── backend/            solução .NET (Hermes.Api e Hermes.Tests)
├── frontend/           aplicação Vue + TypeScript
├── docs/               documentação (modelo de dados, API, testes)
├── docker-compose.yml  SQL Server para desenvolvimento
└── .env.example        modelo das variáveis de ambiente
```

## Pré-requisitos

- Git (com acesso ao repositório)
- .NET SDK 10
- Docker (Docker Desktop ou Docker Engine) com `docker compose`

## Como arrancar

1. Clonar o repositório:
   ```
   git clone git@github.com:0lavo/Hermes.git
   cd Hermes
   ```

2. Criar o ficheiro de ambiente e arrancar a base de dados:
   ```
   cp .env.example .env
   docker compose up -d
   ```
   O ficheiro `.env` não vai para o Git. A base de dados fica em `localhost:1433`.

3. Compilar e correr a API:
   ```
   cd backend
   dotnet build
   dotnet run --project Hermes.Api
   ```
   O endereço da API aparece no terminal.

4. Correr os testes:
   ```
   dotnet test
   ```

## Ligar à base de dados com um cliente (DBeaver, etc.)

- Servidor: `localhost`, porta `1433`
- Autenticação: SQL Server Authentication, utilizador `sa`
- Password: a que está no teu `.env`
- Se der erro de certificado, ativar `Trust server certificate`

## Comandos úteis (base de dados)

- Parar: `docker compose down` (os dados mantêm-se)
- Apagar tudo, incluindo os dados: `docker compose down -v`
- Ver os logs: `docker logs hermes-db --tail 20`

## Como trabalhar em equipa

- Nunca fazer commit direto na `main`. Cada User Story tem a sua branch: `feature/USxx-descricao` (ex.: `feature/US17-autenticacao`).
- Commits curtos e com prefixo: `feat:`, `fix:`, `chore:`, `docs:`, `test:`.
- Abrir Pull Request para a `main`; só se faz merge depois de outra pessoa da equipa rever.
- Cada pessoa escreve os testes do que implementa.
- Nunca fazer commit de segredos (`.env`, credenciais do Google, etc.).

## Problemas comuns

- O container não arranca: ver os logs. A password do `.env` tem de ter 8 ou mais caracteres e três de quatro categorias (maiúsculas, minúsculas, números, símbolos).
- A porta 1433 está ocupada: parar o outro serviço que a usa, ou mudar a porta no `docker-compose.yml`.