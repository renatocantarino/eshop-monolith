# CQRS Architecture

Este diretório contém a implementação do padrão **CQRS (Command Query Responsibility Segregation)** com **Mediator Pattern** para o projeto.

## 📁 Estrutura de Diretórios

```
AppShared/Cqrs/
├── Abstractions/          # Interfaces base e tipos fundamentais
│   ├── ICommand.cs        # Interface para comandos (sem retorno)
│   ├── ICommand<T>.cs     # Interface para comandos (com retorno)
│   ├── ICommandHandler.cs # Handlers para comandos
│   ├── IQuery.cs          # Interface para queries
│   ├── IQueryHandler.cs   # Handlers para queries
│   └── Unit.cs            # Tipo para representar "void" em contextos genéricos
│
├── Mediator/              # Implementação do Mediator Pattern
│   ├── IMediator.cs       # Interface do mediator
│   └── RaptorMediator.cs  # Implementação concreta do mediator
│
├── Pipeline/              # Pipeline behaviors (cross-cutting concerns)
│   ├── IPipelineBehavior.cs  # Interface para behaviors
│   ├── LoggingBehavior.cs    # Behavior de logging
│   └── MetricsBehavior.cs    # Behavior de métricas/performance
│
├── Internal/              # Implementação interna (wrappers)
│   ├── QueryWrapper.cs                  # Wrapper para queries
│   ├── CommandWrapper.cs                # Wrapper para comandos sem retorno
│   └── CommandWrapperWithResponse.cs    # Wrapper para comandos com retorno
│
└── Extensions/            # Extension methods
    └── ServiceCollectionExtensions.cs   # Extensões para DI
```

## 🎯 Conceitos Principais

### CQRS (Command Query Responsibility Segregation)

O padrão CQRS separa as operações de leitura (Queries) das operações de escrita (Commands):

- **Commands**: Modificam o estado do sistema (Create, Update, Delete)
- **Queries**: Apenas leem dados, sem modificar o estado

### Mediator Pattern

O Mediator centraliza a comunicação entre componentes, reduzindo o acoplamento:

```csharp
// Ao invés de:
var result = await queryHandler.HandleAsync(query);

// Usamos:
var result = await mediator.ExecuteQueryAsync(query);
```

## 🚀 Como Usar

### 1. Criar uma Query

```csharp
using AppShared.Cqrs.Abstractions;
using AppShared.Dtos;

namespace YourApi.Application.UseCases;

// Query record com os parâmetros necessários
public record GetProductById(int Id) : IQuery<ProductResponse>;
```

### 2. Criar um Query Handler

```csharp
using AppShared.Cqrs.Abstractions;

public class GetProductByIdHandler : IQueryHandler<GetProductById, ProductResponse>
{
    private readonly IProductService _productService;

    public GetProductByIdHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ProductResponse> HandleAsync(GetProductById query, CancellationToken ct = default)
    {
        var product = await _productService.GetByIdAsync(query.Id);
        return product?.ToDto();
    }
}
```

### 3. Registrar no DI Container

```csharp
// Program.cs
builder.Services.AddRaptorMediator();
builder.Services.AddScoped<IQueryHandler<GetProductById, ProductResponse>, GetProductByIdHandler>();
```

### 4. Usar no Endpoint

```csharp
app.MapGet("/products/{id}", async (int id, IMediator mediator, CancellationToken ct) =>
{
    var query = new GetProductById(id);
    var result = await mediator.ExecuteQueryAsync(query, ct);
    return result is not null ? Results.Ok(result) : Results.NotFound();
});
```

## 📝 Exemplos de Commands

### Command sem retorno

```csharp
// Command
public record DeleteProduct(int Id) : ICommand;

// Handler
public class DeleteProductHandler : ICommandHandler<DeleteProduct>
{
    public async Task HandleAsync(DeleteProduct command, CancellationToken ct = default)
    {
        // Lógica de deleção
    }
}

// Uso
await mediator.ExecuteCommandAsync(new DeleteProduct(productId));
```

### Command com retorno

```csharp
// Command
public record CreateProduct(string Name, decimal Price) : ICommand<int>;

// Handler
public class CreateProductHandler : ICommandHandler<CreateProduct, int>
{
    public async Task<int> HandleAsync(CreateProduct command, CancellationToken ct = default)
    {
        // Lógica de criação
        return newProductId;
    }
}

// Uso
var productId = await mediator.ExecuteCommandAsync(new CreateProduct("Product", 99.99m));
```

## 🔧 Pipeline Behaviors

Os behaviors interceptam a execução de commands/queries para adicionar funcionalidades transversais:

### Behaviors Disponíveis

1. **LoggingBehavior**: Registra logs antes e depois da execução
2. **MetricsBehavior**: Mede o tempo de execução

### Criar um Behavior Customizado

```csharp
using AppShared.Cqrs.Pipeline;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request, 
        Func<Task<TResponse>> next, 
        CancellationToken cancellationToken = default)
    {
        // Validação antes
        ValidateRequest(request);
        
        // Executa o próximo behavior ou handler
        var response = await next();
        
        // Lógica após execução (se necessário)
        return response;
    }
}
```

### Registrar Behavior

```csharp
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

## 🎨 Vantagens da Arquitetura

### ✅ Separação de Responsabilidades
- Queries e Commands têm propósitos claros e distintos
- Facilita a manutenção e testes

### ✅ Desacoplamento
- Endpoints não conhecem os handlers diretamente
- Facilita mudanças de implementação

### ✅ Extensibilidade
- Behaviors podem ser adicionados sem modificar handlers
- Pipeline configurável por tipo de request

### ✅ Testabilidade
- Handlers podem ser testados isoladamente
- Behaviors podem ser testados independentemente

### ✅ API Simplificada
- Inferência automática de tipos genéricos
- Menos código boilerplate nos endpoints

## 📚 Boas Práticas

### ✅ DO

- Use records para Queries e Commands (imutáveis)
- Mantenha handlers focados em uma única responsabilidade
- Valide parâmetros nos handlers
- Use nomes descritivos para queries/commands
- Documente queries/commands complexos com XML comments

### ❌ DON'T

- Não coloque lógica de negócio nos endpoints
- Não reutilize handlers para múltiplos propósitos
- Não faça queries modificarem estado
- Não faça commands retornarem grandes quantidades de dados

## 🔍 Troubleshooting

### Handler não encontrado

```
InvalidOperationException: Handler for request 'GetProductById' not found.
```

**Solução**: Verifique se o handler está registrado no DI container.

### Tipo de retorno incompatível

```
Cannot convert from 'Task<ProductResponse?>' to 'Task<ProductResponse>'
```

**Solução**: Certifique-se de que o tipo de retorno do handler corresponde ao tipo genérico da interface.

## 📖 Referências

- [CQRS Pattern - Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [Mediator Pattern](https://refactoring.guru/design-patterns/mediator)
- [MediatR Library](https://github.com/jbogard/MediatR) - Inspiração para esta implementação
