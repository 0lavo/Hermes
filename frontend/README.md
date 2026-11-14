## Frontend — Vue 3 + TypeScript

### Estado atual

A estrutura inicial do frontend do Hermes foi criada e testada.

**Tecnologias configuradas:**
- Vue 3 + Vite + TypeScript
- Vue Router — navegação entre páginas
- Pinia — gestão de estado
- Axios — comunicação HTTP com o backend
- Vitest — testes unitários
- ESLint e Prettier — qualidade e formatação de código

### Estrutura implementada

| Ficheiro | Descrição |
|---|---|
| `src/main.ts` | Inicialização do Vue, Pinia e Router |
| `src/App.vue` | Componente principal com `RouterView` |
| `src/router/index.ts` | Configuração das rotas |
| `src/views/HomeView.vue` | Página inicial e testes temporários de autenticação |
| `src/stores/auth.ts` | Store de autenticação |
| `src/services/api.ts` | Cliente Axios com envio automático do token |
| `.env.example` | Exemplo de configuração do endereço da API |

### Como executar

Na pasta `frontend`:

```bash
npm install
npm run dev
```

Abrir o endereço apresentado pelo Vite (normalmente `http://localhost:5173`).

### Configuração da API

Copiar `.env.example` para `.env` e configurar o endereço do backend:

```env
# Substituir pela porta real do backend .NET
VITE_API_BASE_URL=http://localhost:5000/api
```

Enquanto não existir esta configuração, o Axios utiliza `/api` como endereço base.

### Autenticação

A store `useAuthStore`, disponível em `src/stores/auth.ts`, disponibiliza:

- `token` — token de autenticação atual.
- `isAuthenticated` — indica se existe um token.
- `setToken(token)` — guarda o token.
- `logout()` — limpa o token.

O cliente Axios (`src/services/api.ts`) adiciona automaticamente o cabeçalho `Authorization: Bearer <token>` aos pedidos quando existe um token.

**Nota:** atualmente o token é guardado apenas em memória e desaparece ao atualizar a página. Ainda não existe autenticação real com o backend.

### Testes realizados

- Aplicação Vue iniciada com sucesso.
- Rota inicial `/` funcional.
- Pinia testada com login e logout simulados.
- Axios testado: cabeçalho Authorization enviado corretamente.
- `npm run type-check` — sem erros.
- `npm run build` — compilação concluída com sucesso.

**Nota para a equipa:** o frontend já pode ser utilizado como base para desenvolver novas funcionalidades. Falta integrar com o backend.