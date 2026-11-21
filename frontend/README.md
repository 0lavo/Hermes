# Frontend — Vue 3 + TypeScript

## Estado atual

O frontend do Hermes está configurado e integrado com o backend .NET para autenticação de utilizadores.

Já permite criar contas, iniciar sessão com email e password ou Google, terminar sessão e recuperar a autenticação depois de atualizar a página.

As páginas de Login e Registo são responsivas e utilizam os componentes visuais do projeto.

O desenvolvimento do dashboard e das restantes funcionalidades continua em curso.

## Tecnologias

- Vue 3 + Vite + TypeScript — desenvolvimento do frontend.
- Vue Router — navegação e proteção de rotas.
- Pinia — gestão do estado da autenticação.
- Axios — comunicação HTTP com a API .NET.
- Tailwind CSS — estilos e responsividade.
- Google Identity Services — autenticação com Google.
- Vitest — testes unitários.
- ESLint e Prettier — qualidade e formatação do código.

## Estrutura implementada

| Ficheiro ou pasta | Descrição |
|---|---|
| `src/main.ts` | Inicialização do Vue, Pinia e Router |
| `src/App.vue` | Componente principal da aplicação |
| `src/router/index.ts` | Configuração e proteção de rotas |
| `src/views/HomeView.vue` | Página inicial provisória, com informação da sessão e Logout |
| `src/views/LoginView.vue` | Página de Login |
| `src/views/RegisterView.vue` | Página de Registo |
| `src/views/DesignSystemTest.vue` | Página de testes dos componentes visuais |
| `src/components/ui/` | Componentes reutilizáveis da interface |
| `src/components/auth/GoogleLoginButton.vue` | Botão Google reutilizável |
| `src/stores/auth.ts` | Estado, recuperação e limpeza da sessão |
| `src/services/api.ts` | Cliente Axios com envio automático do JWT |
| `src/services/authService.ts` | Pedidos aos endpoints de autenticação |
| `src/__tests__/` | Testes unitários |
| `.env.example` | Exemplo das variáveis de ambiente |
| `index.html` | HTML principal e carregamento do Google Identity Services |

## Como executar

Antes de iniciar o frontend, é necessário ter:

- Node.js e npm instalados.
- SQL Server em execução através do Docker.
- Backend .NET em execução.

As instruções para iniciar o Docker e a API estão no [README principal](../README.md).

Na pasta `frontend`:

```bash
npm install
```

Criar o ficheiro de configuração a partir do exemplo:

```bash
cp .env.example .env
```

Configurar as variáveis de ambiente e iniciar:

```bash
npm run dev
```

Abrir no navegador:

http://localhost:5173

## Configuração das variáveis de ambiente

O ficheiro `frontend/.env` deve conter:

```env
VITE_API_BASE_URL=http://localhost:5187/api
VITE_GOOGLE_CLIENT_ID=COLOCAR_CLIENT_ID_GOOGLE
```

### VITE_API_BASE_URL

Define o endereço da API .NET.

Em desenvolvimento, utilizamos:

```text
http://localhost:5187/api
```

Se a variável não estiver configurada, o Axios utiliza `/api` como endereço base. Nesse caso, será necessário configurar um proxy ou servir a API no mesmo domínio.

### VITE_GOOGLE_CLIENT_ID

Identifica a aplicação Hermes perante o Google.

O Client ID utilizado pelo backend encontra-se em:

```text
backend/Hermes.Api/appsettings.json
```

Deve ser utilizado o mesmo Client ID no frontend e no backend.

O Client ID não é um segredo, mas passwords, Client Secrets e outros dados privados nunca devem ser colocados no frontend.

Para a autenticação Google funcionar:

- Utilizar exatamente `http://localhost:5173`.
- Este endereço deve estar nas origens JavaScript autorizadas da Google Cloud.
- Enquanto a aplicação estiver em modo de teste, a conta Google tem de estar autorizada.
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
| `/` | Página inicial provisória | Autenticado |
| `/test` | Página de testes do Design System | Público, durante o desenvolvimento |

Se um utilizador sem sessão tentar aceder à página inicial, será redirecionado para `/login`.

Se um utilizador autenticado tentar abrir `/login` ou `/register`, será redirecionado para `/`.

### Endpoints utilizados

| Método | Endpoint | Funcionalidade |
|---|---|---|
| POST | `/api/auth/register` | Criar conta |
| POST | `/api/auth/login` | Iniciar sessão |
| POST | `/api/auth/google` | Autenticar com Google |
| GET | `/api/auth/me` | Validar sessão e obter dados do utilizador |

Os pedidos estão centralizados em `src/services/authService.ts`.

O cliente Axios, definido em `src/services/api.ts`, acrescenta automaticamente o cabeçalho `Authorization: Bearer <token>` quando existe um token.

### Login e Registo

As páginas utilizam os componentes reutilizáveis do frontend.

O Login permite iniciar sessão com email e password.

O Registo permite criar uma conta com nome, email e password. Inclui validação da confirmação da password e do comprimento mínimo de 8 caracteres.

Em caso de sucesso, o backend devolve:

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

O exemplo acima é ilustrativo.

Os dados do utilizador e o JWT são guardados na store do Pinia, e o JWT é também colocado no `sessionStorage`.

Depois da autenticação, o utilizador é redirecionado para `/`.

### Autenticação com Google

O componente `GoogleLoginButton.vue` utiliza Google Identity Services para apresentar o botão oficial Google.

Quando o utilizador escolhe uma conta, o Google devolve um ID token.

O frontend envia esse token à API:

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

O componente Google é reutilizado nas páginas de Login e Registo.

O botão adapta a largura ao espaço disponível através de `ResizeObserver`, respeitando os limites do botão oficial Google.

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

O Logout está atualmente disponível na página inicial provisória (`HomeView.vue`).

Ao terminar sessão:

- O JWT é removido do Pinia.
- Os dados do utilizador são eliminados da store.
- O JWT é removido do `sessionStorage`.
- O utilizador é redirecionado para `/login`.

O endpoint `/api/auth/logout` existe no backend, mas a implementação atual do frontend termina a sessão localmente.

O JWT não é revogado no servidor.

### Segurança

A autenticação utiliza JWT e proteção de rotas no frontend.

A proteção do Vue Router melhora a navegação, mas não substitui as verificações de autenticação e autorização realizadas pelo backend.

O `sessionStorage` é utilizado nesta fase do desenvolvimento. Como pode ser acedido por JavaScript, não elimina o risco de exposição do token em caso de XSS.

Antes de disponibilizar a aplicação em produção, deverão ser revistas a estratégia de armazenamento dos tokens, a renovação de sessões e as restantes medidas de segurança.

## Testes realizados

### Testes manuais

Foram confirmados os seguintes comportamentos:

- Registo de utilizadores através da API.
- Login com email/password.
- Registo através do frontend.
- Validação das passwords no Registo.
- Tratamento de email já registado.
- Login com Google.
- Redirecionamento após autenticação.
- Proteção da página inicial.
- Logout e bloqueio de acesso após terminar sessão.
- Persistência do JWT após `F5`.
- Recuperação dos dados do utilizador através de `/api/auth/me`.
- Bloqueio de acesso a rotas privadas sem sessão validada.
- Responsividade das páginas de Login e Registo em mobile e desktop.

### Testes automáticos

O frontend utiliza Vitest para testar os componentes
e a lógica da aplicação.

Atualmente existem 6 testes automáticos:

- 1 teste do componente principal (RouterView).
- 5 testes da store de autenticação:
  - Guardar a sessão após Login.
  - Remover a sessão após Logout.
  - Recuperar uma sessão válida através da API.
  - Terminar a sessão perante um JWT inválido (401).
  - Manter o token perante uma falha temporária de rede.

Os testes de autenticação simulam as respostas da API,
pelo que não necessitam do backend nem do Docker.

Resultado: 6 testes aprovados em 2 ficheiros.

Ainda poderão ser acrescentados testes dos formulários
de Login e Registo e dos Navigation Guards.

### Comandos de verificação

Na pasta `frontend`:

```bash
npm run type-check
npm run build
npm run test:unit -- --run
```

As últimas verificações realizadas passaram sem erros.

## Funcionalidades ainda por melhorar

- Integrar o Logout no menu do dashboard definitivo.
- Substituir a página inicial provisória pelo dashboard.
- Alargar os testes automáticos aos formulários, rotas e integração com a API.
- Melhorar a apresentação de erros quando o backend está indisponível.
- Rever o armazenamento e a renovação dos tokens antes de produção.
- Remover ou restringir a rota `/test` quando deixar de ser necessária.
- Fazer pequenos ajustes de responsividade em larguras extremamente reduzidas, inferiores a aproximadamente 290px.

## Notas para a equipa

- A autenticação com email/password e Google já está integrada com a API .NET.
- Para executar o frontend, configurar primeiro o ficheiro `.env`.
- O Google Client ID deve corresponder ao utilizado pelo backend.
- A página inicial atual é provisória e inclui o botão de Logout para testes.
- As novas páginas privadas devem utilizar `meta: { requiresAuth: true }` na configuração do Vue Router.
- Os pedidos à API devem utilizar o cliente Axios já configurado em `src/services/api.ts`.
- O backend continua responsável pela validação dos JWT e pelas permissões de acesso.
- Novas funcionalidades devem reutilizar os componentes existentes sempre que possível.
