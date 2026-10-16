# Frontend do CAGE

Interface Angular 22 para gestão de confeitaria. Usa TypeScript 6, Angular Material/CDK, RxJS, componentes standalone, signals e formulários reativos.

## Executar

Use Node 22.22.3+ na linha 22, 24.15.0+ na linha 24 ou 26+, conforme o `package-lock.json`. O `package.json` declara npm 11.16.0. Não é necessário Angular CLI global.

Dentro de `frontend/`:

```bash
npm ci --legacy-peer-deps
npm start
```

A interface fica em `http://localhost:4200`. A API e um usuário previamente cadastrado são necessários para utilizar os módulos; veja a [configuração completa e o primeiro acesso](../README.md).

## Páginas e estrutura

- `/login`: autenticação por e-mail e senha.
- `/dashboard`: resumo mensal e registros recentes.
- `/estoque`: itens, alertas, entradas e saídas.
- `/receitas`: ingredientes, custos, preço de venda e situação da receita.
- `/pedidos`: cadastro, edição, pagamento e status dos pedidos.
- `/financeiro`: transações, filtros, resumo e cancelamento.
- `/configuracoes`: perfil, senha, usuários e dados da empresa.

As páginas ficam em `src/app/pages`; componentes compartilhados em `src/app/shared/components`; autenticação, guards, interceptors, modelos e serviços HTTP em `src/app/core`.

As rotas internas usam `authGuard`. A API determina as permissões de cada operação; a aba Empresa é visível somente para o Dono. Access token e refresh token são armazenados em `localStorage`; o interceptor inclui o Bearer token e o serviço de autenticação renova a sessão.

## Endereço da API

| Comando | Configuração |
| --- | --- |
| `npm start` | `src/environments/environment.development.ts` |
| `npm run build` | `src/environments/environment.ts` |

Os dois arquivos usam atualmente `http://localhost:8080/api`. Essa configuração é incorporada ao build, não lida de variáveis do container em tempo de execução. A origem do frontend também deve ser permitida pelo CORS da API (`ROTA_DO_FRONTEND`).

## Build e Docker

```bash
npm run build
```

O build de produção gera os arquivos estáticos em `dist/frontend/browser`.

O `Dockerfile` usa Node 22 para o build e Nginx para servir a aplicação na porta 80. O `nginx.conf` faz fallback para `index.html`, permitindo abrir diretamente rotas como `/pedidos`. O Compose na raiz publica o frontend na porta 4200.

## Testes

```bash
# Interativo
npm test

# Uma execução
npm test -- --watch=false
```

Os testes usam Vitest e o ambiente de testes do Angular. Há testes de componentes e fluxos HTTP simulados, incluindo configurações de perfil, usuários, empresa e edição de pedidos. Não há uma suíte de ponta a ponta configurada.

Em **16/10/2026**, o build de produção passou. A suíte completa de testes parou na compilação: `src/app/pages/financeiro/financeiro.spec.ts` importa `Financeiro` como exportação nomeada, mas o componente usa exportação padrão. Essa pendência precisa ser corrigida antes de validar a suíte completa; nenhuma alteração de código funcional foi feita nesta revisão de documentação.
