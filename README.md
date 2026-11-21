# Hermes — Radar de Dúvidas em Tempo Real

Aplicação web para os alunos sinalizarem dúvidas durante a aula, em tempo real e de forma anónima para os colegas. O professor cria uma sala, os alunos entram com o código, submetem dúvidas e votam, e o professor acompanha tudo num dashboard.

**Equipa:** João Borges, Olavo Moreira, Diogo Pimenta, Maria Giulia Barbosa

Projeto da UC Laboratório de Programação (22356), ISLA Gaia, 2026/2027.

> Estado: Sprint 2 (codificação e testes) em curso. Este README é atualizado à medida que o projeto avança.

## Tecnologias

- Frontend: Vue 3 + TypeScript + Vite, Pinia, Vue Router e Tailwind CSS
- Backend: C# + .NET 10 (API REST + SignalR para o tempo real)
- Base de dados: SQL Server (em Docker), com Entity Framework Core 10
- Controlo de versões: Git + GitHub

## Estrutura do repositório

```text
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
- Node.js 22.18 ou superior (para o frontend)
- Docker (Docker Desktop ou Docker Engine) com `docker compose`

## Como arrancar

1. Clonar o repositório:

   ```bash
   git clone git@github.com:0lavo/Hermes.git
   cd Hermes
   ```

2. Criar o ficheiro de ambiente e arrancar a base de dados:

   ```bash
   cp .env.example .env
   docker compose up -d
   ```

   O ficheiro `.env` não vai para o Git. A base de dados fica disponível em `127.0.0.1:1433`.

   A password definida em `MSSQL_SA_PASSWORD` tem de corresponder à utilizada na connection string do backend. Se for necessário utilizar uma configuração diferente, pode ser definida localmente através da variável de ambiente `ConnectionStrings__Default`.

   Não colocar passwords reais em commits.

3. Instalar as ferramentas do projeto e criar as tabelas:

   ```bash
   cd backend
   dotnet tool restore
   dotnet ef database update --project Hermes.Api
   ```

   O primeiro comando instala o `dotnet-ef` na versão fixada no repositório. O segundo cria a base de dados `Hermes` e as respetivas tabelas.

   Repetir o segundo comando sempre que alguém acrescentar uma migração (depois de um `git pull`).

4. Compilar e correr a API (dentro de `backend/`):

   ```bash
   dotnet build
   dotnet run --project Hermes.Api --launch-profile http
   ```

   O endereço da API aparece no terminal.

   A interface Swagger (apenas em desenvolvimento) fica em http://localhost:5187/swagger.

5. Correr os testes (dentro de `backend/`):

   ```bash
   dotnet test
   ```

6. Iniciar o frontend (a partir da raiz do repositório):

   ```bash
   cd frontend
   cp .env.example .env
   npm install
   npm run dev
   ```

   Antes de iniciar, configurar o Google Client ID no ficheiro `frontend/.env`.

   O frontend fica disponível em http://localhost:5173.

## Base de dados e migrações

As tabelas são geradas a partir das classes em `backend/Hermes.Api/Entities`, com as regras definidas em `Data/HermesDbContext.cs`.

Para alterar a base de dados (a partir de `backend/`):

```bash
dotnet ef migrations add NomeDaMigracao --project Hermes.Api
dotnet ef database update --project Hermes.Api
```

Regras:

- Fazer `git pull` antes de criar uma migração e avisar a equipa, porque duas migrações criadas ao mesmo tempo podem gerar conflitos.
- Nunca editar uma migração que já foi para a `main`. Cria-se uma nova.
- Se houver conflito no `HermesDbContextModelSnapshot.cs`: remover a própria migração com `dotnet ef migrations remove --project Hermes.Api`, fazer `git pull` e criá-la novamente.
- Fazer commit da migração juntamente com o código que a necessita.

## Autenticação (API)

Não há perfil Professor/Aluno: qualquer utilizador autenticado pode criar salas.

| Método | Caminho | Descrição | Respostas |
|---|---|---|---|
| POST | `/api/auth/register` | Registo com nome, email e password (mínimo 8 caracteres) | 201, 400, 409 |
| POST | `/api/auth/login` | Login com email e password | 200, 401 |
| POST | `/api/auth/google` | Login com Google (`{ "idToken": "..." }`) | 200, 201, 400, 401, 409 |
| GET | `/api/auth/me` | Obtém os dados do utilizador autenticado | 200, 401 |
| POST | `/api/auth/logout` | Termina a sessão do lado do cliente; exige token | 204, 401 |

As respostas de sucesso do Login e Registo trazem `{ user, token }`. O JWT segue nos pedidos seguintes no cabeçalho `Authorization: Bearer <token>` e dura 8 horas.

O endpoint `/api/auth/me` permite ao frontend validar o JWT e recuperar os dados do utilizador.

O Logout atual limpa a sessão no cliente, mas não revoga o JWT no servidor.

Configuração (`backend/Hermes.Api`):

- `Jwt:Key`: em desenvolvimento vem do `appsettings.Development.json`. Em produção tem de ser definida pela variável de ambiente `Jwt__Key` (mínimo 32 caracteres), senão a API não arranca.
- `Google:ClientId` (em `appsettings.json`): Client ID do projeto Hermes no Google Cloud. Não é um segredo. Enquanto a aplicação estiver em modo de teste, apenas as contas autorizadas podem entrar com Google.
- CORS: a API aceita pedidos do frontend em `http://localhost:5173`.

Os testes de integração (`dotnet test`, em `backend/`) usam uma base de dados em memória e uma Google simulada, pelo que não precisam do Docker nem de internet.

## Frontend

O frontend utiliza Vue 3, TypeScript, Vite, Pinia, Vue Router, Axios e Tailwind CSS.

Já estão implementadas as seguintes funcionalidades:

- Login com email e palavra-passe.
- Registo de utilizadores.
- Autenticação com Google Identity Services.
- Logout.
- Proteção de rotas privadas.
- Persistência do JWT no `sessionStorage`.
- Recuperação e validação da sessão através de `GET /api/auth/me`.
- Páginas de Login e Registo responsivas.

Para iniciar o frontend:

```bash
cd frontend
npm install
npm run dev
```

É necessário configurar previamente o ficheiro `frontend/.env` com o endereço da API e o Google Client ID.

Para consultar a estrutura do frontend, as instruções de configuração e os testes, ver o [README do frontend](frontend/README.md).

## Documentação

- [Permissões no servidor (US15)](docs/permissoes.md): como validar se um utilizador pode agir numa sala.
- [Contrato da API (OpenAPI)](docs/openapi.json): especificação gerada pela API (`/openapi/v1.json`).

Para atualizar o contrato OpenAPI, arrancar a API e executar na raiz do repositório:

```bash
curl -s http://localhost:5187/openapi/v1.json -o docs/openapi.json
```

## Ligar à base de dados com um cliente (DBeaver, etc.)

- Servidor: `localhost`, porta `1433`
- Autenticação: SQL Server Authentication, utilizador `sa`
- Password: a configurada no `.env`
- Base de dados: `Hermes` (aparece depois de aplicar a migração)
- Se der erro de certificado, ativar `Trust server certificate`

## Comandos úteis (base de dados)

- Parar: `docker compose down` (os dados mantêm-se)
- Apagar tudo, incluindo os dados: `docker compose down -v` (depois, voltar a correr `dotnet ef database update`)
- Ver os logs: `docker logs hermes-db --tail 20`

## Como trabalhar em equipa

- Nunca fazer commit direto na `main`. Cada funcionalidade deve ser desenvolvida numa branch própria.
- Commits curtos e com prefixo: `feat:`, `fix:`, `chore:`, `docs:`, `test:`.
- Abrir Pull Request para a `main`; só se faz merge depois de outra pessoa da equipa rever.
- Cada pessoa escreve os testes do que implementa.
- Nunca fazer commit de segredos (`.env`, passwords, tokens, etc.).

## Problemas comuns

- **O contentor não arranca:** verificar os logs. A password do `.env` tem de ter pelo menos 8 caracteres e cumprir os requisitos de complexidade do SQL Server.
- **A porta 1433 está ocupada:** parar o outro serviço que a utiliza ou alterar a porta no `docker-compose.yml`.
- **Erro de ligação à base de dados:** confirmar com `docker compose ps` que o `hermes-db` está em execução e que a password do backend coincide com a do SQL Server.
- **Base de dados Hermes não existe:** executar `dotnet ef database update --project Hermes.Api` a partir de `backend/`.
- **`dotnet ef` não é reconhecido:** executar `dotnet tool restore` dentro de `backend/`.
- **Erro no Google Login:** confirmar que o Client ID está correto, que o endereço é exatamente `http://localhost:5173` e que a conta Google está autorizada para testes.
- **Frontend não comunica com a API:** confirmar que o backend está em execução e que `VITE_API_BASE_URL` aponta para `http://localhost:5187/api`.
