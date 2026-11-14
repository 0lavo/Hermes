# Hermes — Radar de Dúvidas em Tempo Real

Aplicação web para os alunos sinalizarem dúvidas durante a aula, em tempo real e de forma anónima para os colegas. O professor cria uma sala, os alunos entram com o código, submetem dúvidas e votam, e o professor acompanha tudo num dashboard.

**Equipa:** João Borges, Olavo Moreira, Diogo Pimenta, Maria Giulia Barbosa

Projeto da UC Laboratório de Programação (22356), ISLA Gaia, 2026/2027.

> Estado: Sprint 2 (codificação e testes) em curso. Este README é atualizado à medida que o projeto avança.

## Tecnologias

- Frontend: Vue + TypeScript
- Backend: C# + .NET 10 (API REST + SignalR para o tempo real)
- Base de dados: SQL Server (em Docker), com Entity Framework Core 10
- Controlo de versões: Git + GitHub

## Estrutura do repositório

```
Hermes/
├── backend/
│   ├── Hermes.Api/         API (Entities, Data, Migrations, Controllers)
│   ├── Hermes.Tests/       testes
│   └── dotnet-tools.json   ferramentas do projeto (dotnet-ef)
├── frontend/               aplicação Vue + TypeScript
├── docs/                   documentação (modelo de dados, API, testes)
├── docker-compose.yml      SQL Server para desenvolvimento
└── .env.example            modelo das variáveis de ambiente
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
   O ficheiro `.env` não vai para o Git. A base de dados fica em `localhost:1433`. Não mudar a password do `.env`: a ligação de desenvolvimento (`backend/Hermes.Api/appsettings.Development.json`) usa a do `.env.example`.

3. Instalar as ferramentas do projeto e criar as tabelas:
   ```
   cd backend
   dotnet tool restore
   dotnet ef database update --project Hermes.Api
   ```
   O primeiro comando instala o `dotnet-ef` na versão fixada no repositório. O segundo cria a base de dados `Hermes` e as tabelas. Repetir o segundo sempre que alguém acrescentar uma migração (depois de um `git pull`).

4. Compilar e correr a API (dentro de `backend/`):
   ```
   dotnet build
   dotnet run --project Hermes.Api
   ```
   O endereço da API aparece no terminal.

5. Correr os testes (dentro de `backend/`):
   ```
   dotnet test
   ```

## Base de dados e migrações

As tabelas são geradas a partir das classes em `backend/Hermes.Api/Entities`, com as regras definidas em `Data/HermesDbContext.cs`.

Para alterar a base de dados (a partir de `backend/`):
```
dotnet ef migrations add NomeDaMigracao --project Hermes.Api
dotnet ef database update --project Hermes.Api
```

Regras:
- Fazer `git pull` antes de criar uma migração e avisar a equipa, porque duas migrações criadas ao mesmo tempo dão conflito.
- Nunca editar uma migração que já foi para a `main`. Cria-se uma nova.
- Se houver conflito no `HermesDbContextModelSnapshot.cs`: apagar a própria migração com `dotnet ef migrations remove --project Hermes.Api`, fazer `git pull` e criá-la de novo.
- Fazer commit da migração junto com o código que a precisa.

## Ligar à base de dados com um cliente (DBeaver, etc.)

- Servidor: `localhost`, porta `1433`
- Autenticação: SQL Server Authentication, utilizador `sa`
- Password: a que está no teu `.env`
- Base de dados: `Hermes` (aparece depois de aplicar a migração)
- Se der erro de certificado, ativar `Trust server certificate`

## Comandos úteis (base de dados)

- Parar: `docker compose down` (os dados mantêm-se)
- Apagar tudo, incluindo os dados: `docker compose down -v` (depois, voltar a correr `dotnet ef database update`)
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
- `dotnet ef` não é reconhecido: correr `dotnet tool restore` dentro de `backend/`.
- A API dá erro de ligação à base de dados: confirmar com `docker compose ps` que o `hermes-db` está `running`.