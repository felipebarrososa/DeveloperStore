# DeveloperStore API

Uma API completa para gerenciamento de vendas desenvolvida em .NET 8.0 seguindo princípios de Domain-Driven Design (DDD).

## 🚀 Tecnologias Utilizadas

- **.NET 8.0** - Framework principal
- **C#** - Linguagem de programação
- **Entity Framework Core** - ORM para PostgreSQL
- **MongoDB** - Banco de dados para read models
- **MediatR** - Implementação do padrão Mediator
- **AutoMapper** - Mapeamento de objetos
- **FluentValidation** - Validação de dados
- **JWT Bearer** - Autenticação
- **xUnit** - Framework de testes
- **NSubstitute** - Mocking framework
- **FluentAssertions** - Assertions para testes

## 📋 Funcionalidades

### 🛍️ API de Vendas (Core Business)
- ✅ **CRUD completo** de vendas
- ✅ **Regras de negócio** de desconto por quantidade:
  - 4+ itens: 10% de desconto
  - 10-20 itens: 20% de desconto
  - Máximo 20 itens por produto
- ✅ **Cancelamento** de vendas e itens individuais
- ✅ **Eventos de domínio** (SaleCreated, SaleModified, SaleCancelled, ItemCancelled)

### 🔧 APIs de Suporte
- ✅ **Products API** - Gerenciamento de produtos com categorias
- ✅ **Users API** - Usuários com roles (Admin, Manager, Customer)
- ✅ **Carts API** - Carrinhos de compra
- ✅ **Auth API** - Autenticação JWT

### 🌟 Funcionalidades Avançadas
- ✅ **Paginação** com `_page` e `_size`
- ✅ **Ordenação** com `_order` (ex: "price desc, title asc")
- ✅ **Filtros avançados**:
  - Filtros parciais com `*` (ex: `title=*phone*`)
  - Filtros de range com `_min` e `_max` (ex: `_minPrice=50&_maxPrice=200`)
  - Filtros por categoria, status, role, etc.
- ✅ **Validação automática** com FluentValidation
- ✅ **Tratamento de erros** padronizado
- ✅ **Autenticação e autorização** por roles

## 🏗️ Arquitetura

```
src/
├── DeveloperStore.Domain/          # Entidades e regras de negócio
├── DeveloperStore.Application/      # Casos de uso e DTOs
├── DeveloperStore.Infrastructure/   # Persistência e infraestrutura
└── DeveloperStore.Api/             # Controllers e configuração

tests/
├── DeveloperStore.UnitTests/       # Testes unitários
└── DeveloperStore.IntegrationTests/ # Testes de integração
```

## 🚀 Como Executar

### Pré-requisitos
- .NET 8.0 SDK
- Docker e Docker Compose
- IDE (Visual Studio, VS Code, Rider)

### 1. Clone o repositório
```bash
git clone <repository-url>
cd DeveloperStore
```

### 2. Inicie os bancos de dados
```bash
docker-compose up -d
```

### 3. Execute a aplicação
```bash
cd src/DeveloperStore.Api
dotnet run
```

A API estará disponível em: `https://localhost:7000` ou `http://localhost:5000`

### 4. Acesse o Swagger
```
https://localhost:7000/
```

## 🧪 Como Testar

### Executar testes unitários
```bash
cd tests/DeveloperStore.UnitTests
dotnet test
```

### Executar testes de integração
```bash
cd tests/DeveloperStore.IntegrationTests
dotnet test
```

### Executar todos os testes
```bash
dotnet test tests/
```

## 🔐 Credenciais de Teste

### Usuários pré-criados:
- **Admin**: `admin` / `Pass@123`
- **Manager**: `manager` / `Pass@123`
- **Customer**: `user` / `Pass@123`

### Obter token JWT:
```bash
POST /auth/login
{
  "username": "admin",
  "password": "Pass@123"
}
```

## 📊 Exemplos de Uso

### Listar produtos com filtros e ordenação
```bash
GET /products?_page=1&_size=10&_order=price desc&category=electronics&_minPrice=50
```

### Listar vendas com filtros de data
```bash
GET /sales?from=2024-01-01&to=2024-01-31&_minTotal=100
```

### Criar uma venda
```bash
POST /sales
Authorization: Bearer <token>
{
  "number": "S-1001",
  "date": "2024-01-15",
  "customerId": 1,
  "customerName": "João Silva",
  "branchId": 1,
  "branchName": "Centro",
  "items": [
    {
      "productId": 1,
      "productName": "Mouse Gamer",
      "quantity": 5,
      "unitPrice": 100.00
    }
  ]
}
```

## 🧪 Cobertura de Testes

### Testes Unitários
- ✅ **DiscountCalculator** - Regras de desconto
- ✅ **OrderParser** - Parser de ordenação
- ✅ **Validators** - Validação de DTOs
- ✅ **Handlers** - Lógica de negócio das vendas

### Testes de Integração
- ✅ **APIs principais** - Products, Users, Carts, Sales
- ✅ **Filtros e paginação** - Funcionalidades de listagem
- ✅ **Tratamento de erros** - Respostas HTTP corretas

## 🔧 Configuração

### Variáveis de ambiente
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=developerstore;Username=devuser;Password=devpass",
    "Mongo": "mongodb://localhost:27017"
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "DeveloperStore",
    "Audience": "DeveloperStoreAudience",
    "ExpiresMinutes": 120
  }
}
```

### Docker Compose
```yaml
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: developerstore
      POSTGRES_USER: devuser
      POSTGRES_PASSWORD: devpass
    ports:
      - "5432:5432"
  
  mongo:
    image: mongo:6
    ports:
      - "27017:27017"
```

## 📈 Funcionalidades de Venda

### Regras de Desconto
- **Quantidade 1-3**: Sem desconto
- **Quantidade 4-9**: 10% de desconto
- **Quantidade 10-20**: 20% de desconto
- **Quantidade >20**: Não permitido

### Estrutura de Venda
```json
{
  "id": 1,
  "number": "S-1001",
  "date": "2024-01-15",
  "customerId": 1,
  "customerName": "João Silva",
  "branchId": 1,
  "branchName": "Centro",
  "total": 450.00,
  "cancelled": false,
  "items": [
    {
      "productId": 1,
      "productName": "Mouse Gamer",
      "quantity": 5,
      "unitPrice": 100.00,
      "discountPercent": 0.10,
      "total": 450.00,
      "cancelled": false
    }
  ]
}
```

## 🎯 Endpoints Principais

### Sales API
- `GET /sales` - Listar vendas com filtros
- `GET /sales/{id}` - Obter venda por ID
- `POST /sales` - Criar nova venda
- `PUT /sales/{id}` - Atualizar venda
- `POST /sales/{id}/cancel` - Cancelar venda
- `POST /sales/{id}/items/{itemId}/cancel` - Cancelar item
- `GET /sales/summary` - Resumo diário por filial

### Products API
- `GET /products` - Listar produtos com filtros
- `GET /products/{id}` - Obter produto por ID
- `POST /products` - Criar produto (Admin/Manager)
- `PUT /products/{id}` - Atualizar produto (Admin/Manager)
- `DELETE /products/{id}` - Deletar produto (Admin/Manager)
- `GET /products/categories` - Listar categorias
- `GET /products/category/{category}` - Produtos por categoria

### Users API
- `GET /users` - Listar usuários com filtros
- `GET /users/{id}` - Obter usuário por ID
- `POST /users` - Criar usuário (Admin/Manager)
- `PUT /users/{id}` - Atualizar usuário (Admin/Manager)
- `DELETE /users/{id}` - Deletar usuário (Admin)

### Auth API
- `POST /auth/login` - Autenticação

## 🚀 Deploy

### Produção
1. Configure as variáveis de ambiente
2. Use um banco PostgreSQL gerenciado
3. Use um cluster MongoDB gerenciado
4. Configure HTTPS e certificados SSL
5. Use um reverse proxy (nginx, IIS)

### Docker
```bash
docker build -t developerstore .
docker run -p 5000:5000 developerstore
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📝 Licença

Uso educacional e de avaliação técnica.


**DeveloperStore** - Uma API robusta e escalável para gerenciamento de vendas 🚀
