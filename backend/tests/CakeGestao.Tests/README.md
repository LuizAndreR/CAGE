# Testes do backend

Execute na raiz do repositório, com o SDK .NET 8:

```bash
dotnet test backend/tests/CakeGestao.Tests/CakeGestao.Tests.csproj
```

Para executar uma camada:

```bash
dotnet test backend/tests/CakeGestao.Tests/CakeGestao.Tests.csproj --filter 'FullyQualifiedName~CakeGestao.Tests.Controllers'
dotnet test backend/tests/CakeGestao.Tests/CakeGestao.Tests.csproj --filter 'FullyQualifiedName~CakeGestao.Tests.Commands'
dotnet test backend/tests/CakeGestao.Tests/CakeGestao.Tests.csproj --filter 'FullyQualifiedName~CakeGestao.Tests.Queries'
dotnet test backend/tests/CakeGestao.Tests/CakeGestao.Tests.csproj --filter 'FullyQualifiedName~CakeGestao.Tests.Repositories'
```

## Organização e cenários

- `Controllers/ControllerTests.cs`: cenários nos oito controllers; conversão de erros para HTTP 400/403/404/409/500, ausência de empresa nas consultas, IDs da rota/token, filtros financeiros e retorno dos payloads.
- `Commands/BusinessCommandTests.cs`: criação e atualização de empresas; duplicidade; entrada, saída e bloqueio de exclusão de estoque em uso; criação de pedidos com preço da receita; rejeição de receitas inativas/ausentes e quantidades inválidas; ativação/desativação de receitas; criação e cancelamento de transações.
- `Commands/AuthCommandTests.cs`: validação de credenciais, emissão de tokens, rejeição de refresh tokens ausentes/expirados/usados/revogados e rotação de sessão.
- `Queries/QueryHandlerTests.cs`: consultas de empresas, usuários, funcionários, estoque, receitas, pedidos, financeiro e dashboard. Usa os mapeamentos reais do AutoMapper e validadores reais, com repositories simulados. Verifica valores e erros, incluindo nome da receita, subtotal, data de início, filtros e saldo negativo.
- `Repositories/RepositoryTests.cs`: os sete repositories reais com EF Core InMemory; persistência e releitura por outro contexto, filtros por empresa, relações carregadas com `Include`, limites do mês, cancelamento financeiro e atualização de refresh tokens. Cada teste usa um banco independente.
- `UserSecurityTests.cs`: regressões de autorização e isolamento adicionadas anteriormente, incluindo cadastro de outro Dono na própria empresa.
- `Support/TestData.cs`: dados de exemplo, identidade de teste e configuração do AutoMapper.

## Limites

Esta suíte amplia a cobertura de cenários; não cobre todos os métodos ou caminhos do backend.

Os testes de controllers são unitários: chamam as actions diretamente e simulam o MediatR. Não executam o pipeline HTTP, autenticação JWT, políticas de autorização ou model binding. Os testes dos handlers executam regras e validadores reais, mas simulam as dependências externas.

Os testes dos repositories são testes de integração com EF Core InMemory, não testes de um PostgreSQL real. Não precisam de conexão nem alteram dados de desenvolvimento ou produção. O provider em memória não comprova tradução de SQL, migrations, constraints, unicidade, cascatas relacionais, transações ou concorrência. Uma suíte de integração com PostgreSQL continua necessária para validar esses aspectos.

Não foram alteradas regras da aplicação nesta ampliação dos testes. O sucesso da suíte não representa validação completa para produção.

## Última execução

Em 01/10/2026: **128 testes passaram, 0 falhas e 0 ignorados** (88 novos cenários e 40 de segurança existentes).
