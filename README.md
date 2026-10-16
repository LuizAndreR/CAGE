# 🍰 Cake Gestão (CAGE)

Sistema web para pequenas confeitarias, com gestão de estoque, receitas culinárias, pedidos e movimentações financeiras por empresa.

O projeto possui API em ASP.NET Core 8, interface em Angular 22 e persistência em PostgreSQL 16. Os módulos principais estão implementados; a implantação em produção ainda depende de validação do ambiente e dos fluxos com usuários reais.

## Funcionalidades implementadas

| Módulo | Recursos atuais |
| --- | --- |
| Dashboard | Total de itens cadastrados no estoque, entradas, saídas e saldo do mês, últimas receitas e últimos pedidos. |
| Estoque | Cadastro por nome e marca, unidades de medida, quantidade mínima, alertas, entradas, saídas e cálculo do custo médio. Compras pela entrada de estoque geram uma despesa financeira. |
| Receitas | Ingredientes vinculados ao estoque, modo de preparo, conversão de unidades, custo dos ingredientes, custo extra, margem sobre o custo e preço de venda. Cadastro, edição, exclusão e ativação/desativação. |
| Pedidos | Dados do cliente, telefone, entrega, receitas e quantidades, valor total, edição, exclusão, status de produção e controle de pagamento. |
| Financeiro | Entradas e saídas por categoria, filtros por tipo, categoria, mês e ano, resumo financeiro e cancelamento com motivo e responsável. |
| Configurações | Consulta e edição do próprio nome/e-mail, troca de senha, listagem e cadastro de usuários da empresa e edição do nome/endereço da empresa pelo Dono. |
| Autenticação | Login com e-mail e senha, JWT, renovação por refresh token e permissões por perfil. Senhas armazenadas com hash BCrypt. |

O frontend tem telas de login, dashboard, estoque, receitas, pedidos, financeiro e configurações. A administração global de empresas e algumas operações de usuários existem na API, sem uma tela administrativa correspondente no frontend atual.

## Regras entre os módulos

- As quantidades dos ingredientes são convertidas para a unidade do estoque ao cadastrar ou editar uma receita. São aceitas `G`, `KG`, `ML`, `L`, `TSP`, `TBS`, `CUP` e `UN`. Conversões entre massa e volume exigem a referência de peso cadastrada no ingrediente; `UN` não é convertida para massa ou volume.
- O estoque permite saldo negativo. A precisão persistida das quantidades de estoque e ingredientes é de duas casas decimais.
- Os status de pedido são `Pendente`, `EmProducao`, `ProntoEntregue` e `Concluido`.
- A mudança de `Pendente` para `EmProducao` desconta os ingredientes. A mudança de `EmProducao` para `Pendente` devolve os ingredientes; essa devolução não é aplicada às demais transições.
- Alterar receitas ou quantidades de um pedido faz o pedido voltar para `Pendente`. Se estava em produção, os ingredientes dos itens antigos são devolvidos antes da substituição. Se estava pago, o pagamento ativo é cancelado.
- Marcar um pedido como pago gera uma entrada financeira. Desfazer o pagamento cancela a entrada ativa correspondente ao pedido e à empresa.
- Excluir um pedido cancela suas transações ativas e preserva o histórico financeiro, removendo o vínculo com o pedido excluído.
- Transações canceladas permanecem no histórico, mas não entram nos totais financeiros nem no resumo mensal do dashboard.

## Perfis e acesso

Os perfis cadastrados são `Admin`, `Dono`, `Confeiteiro`, `Auxiliar`, `Decorador`, `Atendente` e `Caixa`. A existência de um perfil não significa que ele tenha acesso a todos os módulos.

- **Admin:** administração global de empresas e operações administrativas de usuários pela API.
- **Dono:** operação da própria empresa, cadastro de funcionários e de outros Donos da mesma empresa, além de consulta e edição dos dados da empresa em `/api/empresa/me`.
- **Demais perfis:** acesso conforme as regras das rotas. Atualmente, dashboard, pedidos e financeiro exigem `Admin` ou `Dono`; as operações da empresa também dependem do contexto de empresa disponível no token.

A interface usa guards de autenticação, e a aba Empresa é exibida somente para o Dono. As verificações da API continuam sendo responsáveis por autorizar as operações.

## Tecnologias e organização

| Área | Tecnologias |
| --- | --- |
| Backend | .NET 8, ASP.NET Core, EF Core 8, Npgsql/PostgreSQL, MediatR, FluentValidation, FluentResults, AutoMapper, UnitsNet, BCrypt, JWT, Serilog e Swagger. |
| Frontend | Angular 22, TypeScript 6, Angular Material/CDK, RxJS, formulários reativos, signals e componentes standalone com rotas carregadas sob demanda. |
| Testes | xUnit, Moq, EF Core InMemory e Coverlet no backend; Vitest e ferramentas de teste do Angular no frontend. |
| Containers | Docker Compose, imagens da API .NET e do PostgreSQL 16, build Node e Nginx para servir o frontend. |

O backend separa controllers HTTP, commands/queries e handlers da aplicação, entidades e interfaces de domínio, e implementações de persistência e segurança. A API registra as dependências e encaminha as operações ao MediatR. Os repositories usam o `CageContext` e o EF Core.

```text
Cage/
├── README.md
├── .env.example
├── docker-compose.yml
├── backend/
│   ├── CakeGestao.sln
│   ├── src/
│   │   ├── CakeGestao.API/             # Controllers, autenticação, configuração e middleware
│   │   ├── CakeGestao.Application/     # Features, commands, queries, validações e mapeamentos
│   │   ├── CakeGestao.Domain/          # Entidades, enums e interfaces
│   │   └── CakeGestao.Infrastructure/  # EF Core, repositories, migrations e JWT
│   └── tests/CakeGestao.Tests/
└── frontend/
    ├── src/app/
    │   ├── core/                      # Autenticação, guards, interceptors, serviços e modelos
    │   ├── pages/                     # Páginas da aplicação
    │   └── shared/components/         # Formulários, listas e componentes reutilizáveis
    ├── src/environments/
    ├── angular.json
    ├── package.json
    ├── Dockerfile
    └── nginx.conf
```

## Executar em desenvolvimento

Os comandos abaixo partem da raiz do repositório, em um terminal Bash.

### Pré-requisitos

- SDK .NET 8.
- Node.js **22.22.3+ na linha 22**, **24.15.0+ na linha 24** ou **26+**, conforme os requisitos registrados no `frontend/package-lock.json`. A validação desta revisão usou Node 24.18.0.
- npm; o `package.json` declara `npm@11.16.0`. Não é necessário instalar Angular CLI globalmente.
- PostgreSQL 16, local ou em container.
- Ferramenta `dotnet-ef` da linha 8 para aplicar migrations.
- Docker com Compose, caso utilize os containers.

### Configuração do Docker Compose

Copie o exemplo somente se ainda não houver um `.env` local:

```bash
cp -n .env.example .env
```

Edite o `.env` e substitua a senha do banco e a chave JWT. As variáveis consumidas pelo Compose são:

| Variável | Finalidade |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da API; `Development` habilita Swagger. |
| `ROTA_DO_FRONTEND` | Origem permitida pelo CORS, como `http://localhost:4200`. |
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | Banco e credenciais do PostgreSQL. |
| `JWT_KEY` | Chave de assinatura JWT; use uma chave aleatória com pelo menos 32 bytes. |
| `JWT_ISSUER`, `JWT_AUDIENCE` | Emissor e público esperado dos tokens. |
| `JWT_ACCESS_EXPIRATION` | Duração do access token, em minutos. |
| `JWT_REFRESH_EXPIRATION` | Duração do refresh token, em dias. |

O Compose transforma essas variáveis nas configurações esperadas pelo .NET. A API executada com `dotnet run` **não lê o arquivo `.env` automaticamente**.

### Backend local

Disponibilize um PostgreSQL 16. Para usar apenas o banco do Compose:

```bash
docker compose up -d db
```

Configure a API por variáveis de ambiente, usando os mesmos dados do banco:

```bash
export ASPNETCORE_ENVIRONMENT=Development
export ROTA_DO_FRONTEND=http://localhost:4200
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=cakegestao_db;Username=postgres;Password=SUA_SENHA_LOCAL'
export Jwt__Key='SUBSTITUA_POR_UMA_CHAVE_ALEATORIA_DE_PELO_MENOS_32_BYTES'
export Jwt__Issuer=CakeGestaoAPI
export Jwt__Audience=CakeGestaoApp
export Jwt__AccessTokenDurationMinutes=30
export Jwt__RefreshTokenDurationDays=7

dotnet restore backend/CakeGestao.sln
```

Se ainda não tiver `dotnet-ef` instalado:

```bash
dotnet tool install --global dotnet-ef --version 8.0.10
```

Com o banco disponível, aplique as migrations usando a Infrastructure como projeto de migrations e a API como projeto de inicialização:

```bash
dotnet ef database update \
  --project backend/src/CakeGestao.Infrastructure \
  --startup-project backend/src/CakeGestao.API
```

A aplicação não aplica migrations nem cria usuários automaticamente ao iniciar. A execução das migrations em um PostgreSQL vazio não foi validada nesta revisão; consulte as pendências abaixo antes de preparar um ambiente novo.

Inicie a API na porta já usada pelos arquivos de ambiente do frontend:

```bash
dotnet run --project backend/src/CakeGestao.API --launch-profile http --urls http://localhost:8080
```

- API: `http://localhost:8080/api`
- Swagger, em `Development`: `http://localhost:8080/swagger`

Sem a opção `--urls`, o perfil `http` usa `http://localhost:5001`. O perfil `https` usa `https://localhost:7027` e `http://localhost:5001`. Ajuste o `apiUrl` do frontend se optar por essas portas.

### Frontend local

Em outro terminal:

```bash
cd frontend
npm ci --legacy-peer-deps
npm start
```

Acesse `http://localhost:4200`. O comando usa o Angular CLI do próprio projeto.

- Desenvolvimento: `frontend/src/environments/environment.development.ts`.
- Build de produção: `frontend/src/environments/environment.ts`.

Ambos apontam atualmente para `http://localhost:8080/api`. O endereço é incorporado durante o build; alterar o ambiente exige reconstruir o frontend.

### Primeiro acesso

Não existe cadastro público nem seed do primeiro administrador. Um banco novo precisa ter o primeiro usuário `Admin` provisionado por um procedimento administrativo, com senha em hash BCrypt; esse procedimento ainda não está automatizado neste repositório.

Com um Admin existente, a API permite cadastrar uma empresa (`POST /api/empresa`) e cadastrar seu Dono (`POST /api/auth/cadastro?id=<empresaId>`). O Dono então faz login no frontend e pode cadastrar outros usuários da própria empresa, inclusive outro Dono, em Configurações.

## Executar os três serviços com Docker

Após configurar o `.env`, preparar o banco e aplicar as migrations:

```bash
docker compose up --build -d
```

| Serviço | Endereço local |
| --- | --- |
| Frontend servido pelo Nginx | `http://localhost:4200` |
| API | `http://localhost:8080/api` |
| PostgreSQL | `localhost:5432` |

O volume `db_data` mantém os dados do PostgreSQL. A porta `8081` também está publicada no Compose, mas isso por si só não configura HTTPS ou certificados.

O Compose atual é uma base de execução local: o frontend usa `localhost` para acessar a API, e o Nginx serve os arquivos estáticos com fallback para as rotas Angular. Para outro servidor, ajuste a URL da API no frontend e a origem CORS antes do build. Não há configuração de implantação específica para Azure ou AWS neste repositório.

## Testes e build

Na raiz do repositório:

```bash
# Testes do backend
dotnet test backend/CakeGestao.sln

# Cobertura do backend
dotnet test backend/CakeGestao.sln --collect:"XPlat Code Coverage"

# Testes do frontend, sem modo watch
npm --prefix frontend test -- --watch=false

# Build de produção do frontend
npm --prefix frontend run build
```

O build do frontend gera os arquivos em `frontend/dist/frontend/browser`. Detalhes dos testes: [backend](backend/tests/CakeGestao.Tests/README.md) e [frontend](frontend/README.md).

### Validação em 16/10/2026

- Backend: **233 testes passaram**, sem falhas ou testes ignorados.
- Frontend: build de produção concluído.
- Suíte completa do frontend: bloqueada na compilação pelo import nomeado de `Financeiro` em `frontend/src/app/pages/financeiro/financeiro.spec.ts`; o componente utiliza exportação padrão.
- Não foram executados nesta revisão o ambiente completo em Docker, migrations contra PostgreSQL nem testes de ponta a ponta no navegador.

Os testes de repositories usam EF Core InMemory, que não valida o comportamento relacional real do PostgreSQL. Os testes de autorização da empresa verificam as políticas combinadas, mas não substituem um teste HTTP completo com JWT.

## Pendências conhecidas

- Corrigir a compilação da suíte completa do frontend e validar os demais testes.
- Validar as migrations e os mappings em PostgreSQL real: `TokenRefreshMap` e o snapshot ainda declaram `datetime2`, enquanto a migration de criação usa `timestamp with time zone`.
- Automatizar o provisionamento do primeiro administrador.
- Definir o tratamento de falhas parciais nas operações que alteram estoque, pedidos e financeiro: os repositories persistem por operação, sem uma transação única envolvendo todo o fluxo.
- Concluir a validação com usuários e a configuração do ambiente de implantação.

## Autor e licença

Luiz André.

Proprietário — todos os direitos reservados.
