# Frontend — Vue 3 + TypeScript

## Estado atual

O frontend do Hermes está configurado e integrado com o backend .NET para autenticação e entrada em salas.

Já permite criar contas, iniciar sessão com email e palavra-passe ou Google, terminar sessão e recuperar a autenticação depois de atualizar a página.

A **US09 — Entrada numa sala e escolha de identidade/anonimato** também está implementada no frontend. Um utilizador autenticado pode introduzir o código de uma sala ativa, escolher como pretende participar e enviar o pedido ao backend.

As páginas de Login e Registo e o modal de entrada numa sala são responsivos e utilizam os componentes visuais do projeto. A página inicial já apresenta uma saudação e o acesso ao modal, mas o **dashboard definitivo**, a listagem de salas e os arquivos continuam em desenvolvimento.

## Tecnologias

- Vue 3 + Vite + TypeScript — desenvolvimento do frontend.
- Vue Router — navegação e proteção de rotas.
- Pinia — gestão do estado da autenticação.
- Axios — comunicação HTTP com a API .NET.
- Tailwind CSS — estilos e responsividade.
- Google Identity Services — autenticação com Google.
- Vitest + Vue Test Utils — testes unitários e de componentes.
- Oxlint, ESLint e Prettier — qualidade e formatação do código.

## Estrutura implementada

| Ficheiro ou pasta | Descrição |
|---|---|
| `src/main.ts` | Inicialização do Vue, Pinia e Router |
| `src/App.vue` | Componente principal da aplicação |
| `src/router/index.ts` | Configuração e proteção de rotas |
| `src/views/HomeView.vue` | Página inicial provisória, saudação, entrada numa sala e Logout |
| `src/views/LoginView.vue` | Página de Login |
| `src/views/RegisterView.vue` | Página de Registo |
| `src/views/DesignSystemTest.vue` | Página de testes dos componentes visuais |
| `src/components/ui/` | Componentes reutilizáveis da interface |
| `src/components/auth/GoogleLoginButton.vue` | Botão Google reutilizável |
| `src/components/rooms/JoinRoomModal.vue` | Modal de entrada numa sala e escolha de anonimato (US09) |
| `src/stores/auth.ts` | Estado, recuperação e limpeza da sessão |
| `src/services/api.ts` | Cliente Axios com envio automático do JWT |
| `src/services/authService.ts` | Pedidos aos endpoints de autenticação |
| `src/services/roomService.ts` | Pedido de entrada numa sala |
| `src/__tests__/` | Testes unitários e de componentes |
| `.env.example` | Exemplo das variáveis de ambiente |
| `index.html` | HTML principal e carregamento do Google Identity Services |

## Como executar

Antes de iniciar o frontend, é necessário ter:

- Node.js e npm instalados.
- SQL Server em execução através do Docker.
- Backend .NET em execução.

As instruções para iniciar o Docker e a API estão no [README principal](../README.md). Depois de alterações ao esquema da base de dados, pode ser necessário aplicar as migrations do Entity Framework Core.

Na pasta `frontend`:

```bash
npm install
```

Criar o ficheiro `.env` a partir de `.env.example` (em PowerShell, pode utilizar `Copy-Item .env.example .env`) e configurar as variáveis de ambiente.

Iniciar o frontend:

```bash
npm run dev
```

Abrir no navegador:

`http://localhost:5173`

## Configuração das variáveis de ambiente

O ficheiro `frontend/.env` deve conter:

```env
VITE_API_BASE_URL=http://localhost:5187/api
VITE_GOOGLE_CLIENT_ID=COLOCAR_CLIENT_ID_GOOGLE
```

### VITE_API_BASE_URL

Define o endereço da API .NET. Em desenvolvimento, utilizamos `http://localhost:5187/api`.

Se a variável não estiver configurada, o Axios utiliza `/api` como endereço base. Nesse caso, é necessário configurar um proxy ou servir a API no mesmo domínio.

### VITE_GOOGLE_CLIENT_ID

Identifica a aplicação Hermes perante o Google. O Client ID utilizado pelo backend encontra-se em `backend/Hermes.Api/appsettings.json` e deve corresponder ao utilizado no frontend.

O Client ID não é um segredo, mas passwords, Client Secrets e outros dados privados nunca devem ser colocados no frontend.

Para a autenticação Google funcionar:

- Utilizar exatamente `http://localhost:5173`.
- Incluir esse endereço nas origens JavaScript autorizadas da Google Cloud.
- Enquanto a aplicação estiver em modo de teste, autorizar a conta Google utilizada.
- Evitar utilizar `127.0.0.1:5173` ou outra porta sem a configurar previamente na Google Cloud.

**Importante:** o ficheiro `.env` está no `.gitignore` e não deve ser enviado para o GitHub.

## Autenticação

### Funcionalidades implementadas

- Login com email e palavra-passe.
- Registo de novos utilizadores.
- Autenticação com Google.
- Logout.
- Proteção de rotas privadas através do Vue Router.
- Persistência do JWT no `sessionStorage`.
- Recuperação dos dados do utilizador através de `GET /api/auth/me`.
- Validação da sessão junto do backend.
- Tratamento de erros de autenticação.
- Páginas de Login e Registo responsivas.

### Páginas disponíveis

| Rota | Descrição | Acesso |
|---|---|---|
| `/login` | Login com email/password ou Google | Público |
| `/register` | Registo com email/password ou Google | Público |
| `/` | Página inicial provisória e modal da US09 | Autenticado |
| `/test` | Página de testes do Design System | Público, durante o desenvolvimento |

Se um utilizador sem sessão tentar aceder à página inicial, será redirecionado para `/login`. Se um utilizador autenticado tentar abrir `/login` ou `/register`, será redirecionado para `/`.

### Endpoints utilizados

| Método | Endpoint | Funcionalidade |
|---|---|---|
| POST | `/api/auth/register` | Criar conta |
| POST | `/api/auth/login` | Iniciar sessão |
| POST | `/api/auth/google` | Autenticar com Google |
| GET | `/api/auth/me` | Validar sessão e obter dados do utilizador |

Os pedidos estão centralizados em `src/services/authService.ts`. O cliente Axios, definido em `src/services/api.ts`, acrescenta automaticamente o cabeçalho `Authorization: Bearer <token>` quando existe um token.

### Login e Registo

As páginas utilizam os componentes reutilizáveis do frontend.

O Login permite iniciar sessão com email e palavra-passe. O Registo permite criar uma conta com nome, email e palavra-passe, incluindo validação da confirmação e do comprimento mínimo de 8 caracteres.

Em caso de sucesso, o backend devolve uma resposta semelhante a esta:

```json
{
  "user": {
    "id": 1,
    "name": "Utilizador Exemplo",
    "email": "exemplo@email.com"
  },
  "token": "JWT_DEVOLVIDO_PELO_BACKEND"
}
```

O exemplo é ilustrativo. Os dados do utilizador e o JWT são guardados na store do Pinia; o JWT é também colocado no `sessionStorage`. Depois da autenticação, o utilizador é redirecionado para `/`.

### Autenticação com Google

O componente `GoogleLoginButton.vue` utiliza Google Identity Services para apresentar o botão oficial Google.

Quando o utilizador escolhe uma conta, o Google devolve um ID token. O frontend envia esse token à API:

```http
POST /api/auth/google
Content-Type: application/json
```

```json
{
  "idToken": "TOKEN_RECEBIDO_DO_GOOGLE"
}
```

O backend valida o token e devolve `{ user, token }`, tal como no Login normal.

O componente Google é reutilizado nas páginas de Login e Registo. O botão adapta a largura ao espaço disponível através de `ResizeObserver`, respeitando os limites do botão oficial Google.

**Nota:** se já existir uma conta com o mesmo email criada através de email/password, a API pode devolver `409 Conflict`. Atualmente, a associação automática entre contas Google e contas existentes não está implementada.

### Gestão da sessão

A autenticação é gerida pela store `useAuthStore`, em `src/stores/auth.ts`.

A store disponibiliza:

- `token` — JWT atual.
- `user` — dados do utilizador autenticado.
- `isAuthenticated` — indica se existe uma sessão validada.
- `setSession(data)` — guarda os dados após autenticação.
- `restoreSession()` — recupera e valida a sessão.
- `logout()` — limpa os dados da sessão.

O JWT é guardado no `sessionStorage`, permitindo manter a sessão após atualizar a página com `F5`.

Quando a aplicação é atualizada:

1. A store recupera o JWT do `sessionStorage`.
2. O Vue Router chama `restoreSession()`, quando necessário.
3. O frontend envia `GET /api/auth/me`.
4. O backend valida o JWT.
5. Se for válido, os dados do utilizador são recuperados.
6. Se o backend devolver `401 Unauthorized`, a sessão é limpa.

Em caso de falha temporária de rede, o token é mantido, mas as rotas privadas ficam bloqueadas até a sessão poder ser validada novamente.

### Logout

O Logout está disponível na página inicial provisória (`HomeView.vue`). Ao terminar sessão, a store limpa o JWT e os dados do utilizador, remove o token do `sessionStorage` e redireciona para `/login`.

O endpoint `/api/auth/logout` existe no backend, mas a implementação atual do frontend termina a sessão localmente. O JWT não é revogado no servidor.

## US09 — Entrada numa sala e anonimato

### Funcionalidades implementadas

Na página inicial, o botão **Entrar numa sala** abre o componente `JoinRoomModal.vue`, que reutiliza `BaseModal`, `BaseInput` e `BaseButton`.

O formulário permite:

- Introduzir o código numérico de **6 dígitos** (entre `100000` e `999999`).
- Selecionar explicitamente **Mostrar o meu nome** ou **Participar anonimamente**, sem escolha predefinida.
- Validar os campos antes de enviar o pedido.
- Bloquear submissões repetidas enquanto o pedido está pendente.
- Apresentar mensagens de erro para respostas da API e falhas de comunicação.

O `BaseInput` suporta `maxlength` e `inputmode="numeric"`, o que facilita a introdução do código em mobile. A validação do formato é feita no formulário; a API é responsável pela validação efetiva da entrada.

O modal foi ajustado para permitir deslocamento vertical em ecrãs baixos, sem cortar o conteúdo na parte superior. Também reutiliza o comportamento de fecho e gestão de foco do `BaseModal`.

### Endpoint utilizado

O pedido está centralizado em `src/services/roomService.ts`:

```http
POST /api/sessions/{code}/join
Authorization: Bearer <token>
Content-Type: application/json
```

Exemplo de corpo:

```json
{
  "anonymous": true
}
```

Resposta de sucesso:

```json
{
  "anonymous": true
}
```

A resposta contém a opção de anonimato **efetivamente guardada pelo backend**. A primeira escolha fica associada ao participante durante a sessão: se voltar a entrar e selecionar outra opção, o backend devolve a escolha original.

Atualmente, todas as entradas exigem uma conta autenticada. Participar anonimamente significa ocultar a identidade na apresentação aos colegas, não entrar sem autenticação.

O dono não pode entrar na própria sala como participante e apenas é possível entrar quando existe uma sessão ativa.

### Tratamento de erros

| Código HTTP | Comportamento no frontend |
|---|---|
| `400` | Indica que os dados enviados são inválidos |
| `401` | Termina a sessão local e redireciona para `/login` |
| `403` | Indica que o dono não pode entrar como participante |
| `404` | Indica que a sala não foi encontrada |
| `409` | Indica que a sala não tem sessão ativa |
| Outros / sem resposta | Apresenta uma mensagem genérica e permite tentar novamente |

Após uma entrada bem-sucedida, a `HomeView.vue` apresenta uma **confirmação temporária** com o código da sala e a opção de anonimato devolvida pelo servidor. A navegação para o espaço de participação será implementada numa etapa posterior.

**Fora do âmbito desta tarefa:** criação de salas pela interface, dashboard da sessão, listagem de salas participadas e arquivo. O endpoint `GET /api/sessions` disponível no backend lista atualmente as salas de que o utilizador é dono.

## Segurança

A autenticação utiliza JWT e proteção de rotas no frontend. A proteção do Vue Router melhora a navegação, mas não substitui as verificações de autenticação e autorização realizadas pelo backend.

No caso da US09, o servidor confirma a existência da sala, a existência de uma sessão ativa, a identidade do participante e a escolha de anonimato guardada. O código de 6 dígitos não deve ser considerado um segredo forte; recomenda-se avaliar **limitação de tentativas (rate limiting)** no endpoint de entrada em salas.

O `sessionStorage` é utilizado nesta fase do desenvolvimento. Como pode ser acedido por JavaScript, não elimina o risco de exposição do token em caso de XSS.

Antes de disponibilizar a aplicação em produção, deverão ser revistas a estratégia de armazenamento dos tokens, a renovação de sessões e as restantes medidas de segurança.

## Testes realizados

### Testes manuais

Foram confirmados os seguintes comportamentos:

- Registo de utilizadores através da API e do frontend.
- Login com email/password e com Google.
- Validação das passwords no Registo e tratamento de email já registado.
- Redirecionamento após autenticação e proteção das rotas privadas.
- Logout e bloqueio de acesso após terminar sessão.
- Persistência do JWT após `F5` e recuperação através de `/api/auth/me`.
- Responsividade das páginas de Login e Registo em mobile e desktop.
- Abertura e fecho do modal de entrada numa sala.
- Validação do código e da escolha de identidade no modal.
- Entrada numa sala ativa com uma conta diferente da conta do dono.
- Manutenção da primeira escolha de anonimato em entradas posteriores, conforme o comportamento atual do backend.
- Correção do deslocamento vertical do modal em ecrãs de altura reduzida.

### Testes automáticos

O frontend utiliza Vitest e Vue Test Utils. Na última execução, **17 testes passaram em 3 ficheiros**:

- `App.spec.ts` — 1 teste do componente principal (`RouterView`).
- `auth.spec.ts` — 5 testes da store de autenticação: guardar sessão, Logout, recuperar sessão válida, rejeitar JWT inválido (`401`) e manter o token perante falha temporária de rede.
- `roomJoin.spec.ts` — 11 testes da US09: código inválido, escolha obrigatória, envio dos dados, respeito pela escolha devolvida, erros HTTP (`400`, `401`, `403`, `404`, `409`), prevenção de submissões duplicadas e fecho do modal.

Os testes simulam as respostas da API, pelo que não necessitam do backend nem do Docker. O fluxo real de entrada em sala foi também verificado manualmente com o backend e a base de dados em execução.

### Comandos de verificação

Na pasta `frontend`:

```bash
npx oxlint .
npx eslint . --no-cache
npm run type-check
npm run test:unit -- --run
npm run build
```

Na última validação, os **17 testes**, o **type-check** e o **build** passaram. Antes de abrir o PR, todos os comandos de verificação, incluindo Oxlint e ESLint, devem terminar sem erros.

## Funcionalidades ainda por melhorar

- Substituir a página inicial provisória pelo dashboard definitivo.
- Implementar as funcionalidades de criação e gestão de salas na interface.
- Definir com a equipa o conteúdo de **My Rooms** e dos **Arquivos**.
- Criar o espaço de participação para onde o aluno avançará depois de entrar.
- Integrar o Logout no menu do dashboard definitivo.
- Alargar os testes aos formulários de Login/Registo e aos Navigation Guards.
- Melhorar a apresentação de erros quando o backend está indisponível.
- Rever o armazenamento e a renovação dos tokens antes de produção.
- Avaliar limitação de tentativas de entrada em salas no backend.
- Remover ou restringir a rota `/test` quando deixar de ser necessária.
- Fazer pequenos ajustes de responsividade em larguras extremamente reduzidas.

## Notas para a equipa

- A autenticação com email/password e Google está integrada com a API .NET.
- Para executar o frontend, configurar primeiro o ficheiro `.env` e iniciar o backend.
- O Google Client ID deve corresponder ao utilizado pelo backend.
- A página inicial é provisória; inclui o botão de entrada numa sala, feedback de sucesso e Logout.
- As novas páginas privadas devem utilizar `meta: { requiresAuth: true }` no Vue Router.
- Os pedidos à API devem utilizar o cliente Axios de `src/services/api.ts`.
- O backend continua responsável pela validação dos JWT e pelas permissões de acesso.
- O endpoint de entrada numa sala recebe `{ anonymous: boolean }` e devolve a escolha guardada.
- Novas funcionalidades devem reutilizar os componentes existentes sempre que possível.
