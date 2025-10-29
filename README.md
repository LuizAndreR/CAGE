# 🍰 Cake Gestão (CAGE)

Sistema web de gerenciamento para boleira com controle integrado de estoque, finanças, receitas e pedidos.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-17-DD0031?logo=angular)](https://angular.io/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql)](https://www.postgresql.org/)

---

## 📋 Sobre o Projeto

O **Cake Gestão (CAGE)** é uma solução completa para gestão de pequenas bolos e confeitarias, permitindo controle eficiente de:

- 📦 **Estoque** - Gerenciamento de ingredientes e materiais
- 💰 **Finanças** - Controle de despesas e receitas com relatórios
- 📖 **Receitas** - Cadastro e organização de receitas com ingredientes
- 🎂 **Pedidos** - Gestão completa de pedidos com rastreamento

---

## 🏗️ Arquitetura

O projeto segue os princípios de **Domain-Driven Design (DDD)**, **SOLID** e **Clean Code**, organizando o código em camadas bem definidas:

```bash
┌─────────────────────────────────────────────┐
│        Frontend (Angular 17)                │
└─────────────────────────────────────────────┘
                    ↕ HTTP/REST
┌─────────────────────────────────────────────┐
│      API Layer (ASP.NET Core 8)             │
└─────────────────────────────────────────────┘
                    ↕
┌─────────────────────────────────────────────┐
│      Application Layer                      │
└─────────────────────────────────────────────┘
                    ↕
┌─────────────────────────────────────────────┐
│      Domain Layer                           │
└─────────────────────────────────────────────┘
                    ↕
┌─────────────────────────────────────────────┐
│      Infrastructure Layer                   │
└─────────────────────────────────────────────┘
                    ↕
┌─────────────────────────────────────────────┐
│      PostgreSQL Database                    │
└─────────────────────────────────────────────┘
```

---

## 🚀 Tecnologias

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core 8** - Web API REST
- **Entity Framework Core 8** - ORM
- **PostgreSQL 16** - Banco de dados
- **FluentValidation** - Validação de dados
- **FluentResults** - Tratamento de resultados
- **BCrypt.Net** - Criptografia de senhas
- **AutoMapper** - Mapeamento de objetos
- **xUnit** - Testes unitários
- **Swagger** - Documentação da API

### Frontend
- **Angular 17** - Framework SPA
- **TypeScript 5** - Linguagem
- **Angular Material** - Componentes UI
- **RxJS** - Programação reativa
- **HttpClient** - Comunicação com API

### DevOps
- **Git/GitHub** - Controle de versão
- **Docker** - Containerização (futuro)
- **Azure/AWS** - Hospedagem em nuvem

---

## 📋 Módulos

- Gerenciamento de Estoque
- Gerenciamento Financeiro
- Cadastro de Receitas
- Controle de Pedidos

---

## 🏗️ Estrutura de Pastas 

```bash
cake-gestao-cage/
│
├── README.md                           # Documentação principal
├── .gitignore                          # Arquivos ignorados pelo Git
│
├── docs/                               # Documentação técnica
│   ├── ERS.pdf                         # Especificação de Requisitos
│   ├── DER.png                         # Diagrama Entidade-Relacionamento
│   ├── CasosDeUso.md                   # Casos de Uso
│   └── Arquitetura.md                  # Arquitetura de Software
│
├── backend/                            # Backend .NET
│   ├── CakeGestao.sln                  # [translate:Solution] do .NET
│   ├── src/
│   │   ├── CakeGestao.API/             # Camada de apresentação
│   │   ├── CakeGestao.Application/     # Camada de aplicação
│   │   ├── CakeGestao.Domain/          # Camada de domínio
│   │   └── CakeGestao.Infrastructure/  # Camada de infraestrutura
│   └── tests/
│       └── CakeGestao.Tests/           # Testes automatizados
│
├── frontend/                           # Frontend Angular
│   └── cake-gestao-app/
│       ├── src/                        # Código-fonte
│       ├── angular.json                # Configurações Angular
│       └── package.json                # Dependências npm
│
└── scripts/                            # Scripts auxiliares
    ├── setup.sh                        # [translate:Script] de configuração
    └── deploy.sh                       # [translate:Script] de [translate:deploy]

```

---

## 🔧 Como Executar

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) e npm
- [PostgreSQL 16](https://www.postgresql.org/download/)
- [Angular CLI 17](https://angular.io/cli)

### Backend

``` bash
Clonar o repositório
git clone https://github.com/seu-usuario/cake-gestao-cage.git
cd cake-gestao-cage

Navegar para o backend
cd backend/src/CakeGestao.API

Restaurar dependências
dotnet restore

Configurar connection string no appsettings.json
"ConnectionStrings": {
	"DefaultConnection": "Host=localhost;Port=5432;Database=CakeGestaoDB;Username=postgres;Password=suasenha"
}	

Aplicar migrations
dotnet ef database update

Executar a API
dotnet run
```

API estará disponível em: `https://localhost:5001`  
Swagger: `https://localhost:5001/swagger`

### Frontend

``` bash
Navegar para o frontend
cd frontend/cake-gestao-app

Instalar dependências
npm install

Executar em modo desenvolvimento
ng serve

Acessar
http://localhost:4200
```

---

## 🧪 Testes

```bash
Executar todos os testes
cd backend
dotnet test

Testes com cobertura
dotnet test /p:CollectCoverage=true
```

---

## 📚 Documentação

- [Especificação de Requisitos (ERS)](docs/ERS.pdf)
- [Modelo de Banco de Dados (DER)](docs/DER.png)
- [Casos de Uso](docs/CasosDeUso.md)
- [Arquitetura de Software](docs/Arquitetura.md)

---

## 🗓️ Roadmap

- [x] Documentação inicial
- [x] Modelagem do banco de dados
- [ ] Implementação do backend (API)
- [ ] Implementação do frontend (Angular)
- [ ] Testes unitários e de integração
- [ ] Deploy em ambiente de homologação
- [ ] Deploy em produção

---

## 👤 Autor

Luiz André

---

## 📄 Licença

Proprietário - Todos os direitos reservados
