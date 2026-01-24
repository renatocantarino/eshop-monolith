# eShop Monolith

## 📋 Visão Geral do Projeto

**eShop Monolith** é uma aplicação de e-commerce construída em **.NET 9** utilizando **Blazor Server** como framework web. O projeto atualmente segue uma arquitetura monolítica e será gradualmente refatorado para aderir às melhores práticas de desenvolvimento, mantendo escalabilidade, manutenibilidade e qualidade de código.

### Objetivo

Transformar um monolito funcional em uma arquitetura bem estruturada, aplicando padrões de design, princípios SOLID, camadas de responsabilidade clara e preparação para possível evolução futura.

---

## 🏗️ Arquitetura Atual

### Estrutura do Projeto

```
eshop-monolith/
├── AppHost/                 # Orchestrador da aplicação (Aspire)
├── ServiceDefaults/         # Configurações e extensões compartilhadas
└── WebApp/
    ├── Components/          # Componentes Blazor
    │   ├── Layout/         # Layouts da aplicação
    │   └── Pages/          # Páginas (rotas)
    ├── Data/               # Camada de Dados (DbContext, Extensões)
    ├── Models/             # Entidades de Domínio
    ├── Properties/         # Configurações do projeto
    ├── wwwroot/            # Ativos estáticos (CSS, JS, imagens)
    └── Program.cs          # Configuração da aplicação
```

### Stack Tecnológico

- **Framework**: .NET 9
- **UI**: Blazor Server (Razor Components)
- **ORM**: Entity Framework Core 9.0
- **Banco de Dados**: In-Memory (atualmente)
- **Padrão de Hospedagem**: .NET Aspire

---

## 🎯 Funcionalidades Principais

### Entities (Domínio)

1. **Product** - Catálogo de produtos
   - Id, Name, Description, Price, ImageUrl

2. **ShoppingCart** - Carrinho de compras por usuário
   - Id, UserName, Items, TotalPrice

3. **ShoppingCartItem** - Itens do carrinho
   - Produto, Quantidade, Preço

4. **Order** - Pedidos finalizados
   - Informações de pedido e histórico

### Fluxo Principal

- 📦 Visualizar produtos
- 🛒 Adicionar/remover produtos do carrinho
- 💳 Proceder ao checkout
- ✅ Confirmar pedido
- 📋 Visualizar histórico de pedidos

---

## 🚀 Getting Started

### Pré-requisitos

- .NET 9 SDK instalado
- Visual Studio 2022 ou VS Code com extensões C#

### Instalação e Execução

```bash
# Clone o repositório
git clone <repo-url>
cd eshop-app/eshop-monolith

# Restaurar dependências
dotnet restore

# Compilar a solução
dotnet build

# Executar a aplicação
dotnet run --project AppHost
```

A aplicação estará disponível em: `https://localhost:5000`

---

