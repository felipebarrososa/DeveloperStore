# DeveloperStore

API de exemplo (DDD, .NET 8) para avaliação técnica. Implementa CRUD de **Vendas** com regras de desconto por quantidade, além de **Produtos**, **Usuários** e **Carrinhos**. Inclui seed de dados, autenticação JWT e testes unitários.

## Sumário
- [Arquitetura & Stack](#arquitetura--stack)
- [Pré-requisitos](#pré-requisitos)
- [Subir dependências (Docker)](#subir-dependências-docker)
- [Executar a API](#executar-a-api)
- [Autenticação (JWT)](#autenticação-jwt)
- [Endpoints Principais](#endpoints-principais)
  - [Sales (Vendas)](#sales-vendas)
  - [Products / Users / Carts](#products--users--carts)
- [Paginação / Filtro / Ordenação](#paginação--filtro--ordenação)
- [Regras de Negócio (Vendas)](#regras-de-negócio-vendas)
- [Eventos de Domínio (diferencial)](#eventos-de-domínio-diferencial)
- [Testes & Cobertura](#testes--cobertura)
- [CI (GitHub Actions)](#ci-github-actions)
- [Troubleshooting](#troubleshooting)
- [Licença](#licença)

---

## Arquitetura & Stack
**Arquitetura por camadas (DDD):**
- `DeveloperStore.Domain` — entidades, enums, regras puras.
- `DeveloperStore.Application` — DTOs, casos de uso/handlers (MediatR), validações, mapeamentos (AutoMapper).
- `DeveloperStore.Infrastructure` — EF Core (PostgreSQL), Read Model (MongoDB opcional).
- `DeveloperStore.Api` — endpoints (Minimal API/Controllers), DI, autenticação, Swagger.
- `tests/DeveloperStore.UnitTests` — xUnit + FluentAssertions + NSubstitute + EF InMemory.

**Tecnologias:** .NET 8, EF Core, PostgreSQL, (opcional) MongoDB para Read Model, AutoMapper, MediatR, xUnit, NSubstitute, Swagger.

---

## Pré-requisitos
- **Docker** e **Docker Compose**  
- **.NET SDK 8.0**
- (Opcional) **MongoDB** local, se você quiser materializar o *read model*

---

## Subir dependências (Docker)
Já existe um `docker-compose.yml` na raiz.

```bash
docker compose up -d
```

Cria:
- **Postgres** em `localhost:5432` com:
  - DB: `devstore`
  - Usuário: `devstore`
  - Senha: `devstore`

> Dica: para ver usuários seedados na tabela `Users`:
```bash
docker exec -it devstore_postgres psql -U devstore -d devstore -c 'select "Id","Username","Email","Role" from "Users" order by "Id";'
```

### (Opcional) MongoDB local
Caso deseje usar o **Read Model** de vendas:
```bash
docker run -d --name devstore_mongo -p 27017:27017 mongo:6
```
Depois, exporte as variáveis (exemplos no PowerShell/Windows):
```powershell
$env:Mongo__ConnectionString="mongodb://localhost:27017"
$env:Mongo__Database="devstore_read"
```

---

## Executar a API
Dentro de `src/DeveloperStore.Api`:

```bash
dotnet run
```

Saída esperada:
```
Now listening on: http://localhost:5000
Hosting environment: Development
```

**Swagger:** abra `http://localhost:5000/swagger`

> A API cria as tabelas no Postgres e faz seed de dados (produtos/usuários).  
> Para Vendas, você pode criar via endpoints; há também testes/seed de exemplo.

---

## Autenticação (JWT)
A API usa **JWT Bearer**. Primeiro faça login para obter o token.

**Usuário de teste principal (Admin):**
- **username:** `admin` / **password:** `Pass@123`

> (Opcional, se sua seed também incluir) usuários adicionais:
> - `johnd` / `m38rmF$`
> - `mor_2314` / `83r5^_`

**Login:**
```bash
curl -X POST http://localhost:5000/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{ "username": "admin", "password": "Pass@123" }"
```
Resposta:
```json
{ "token": "<JWT>" }
```

Use o token nas chamadas seguintes:
```
Authorization: Bearer <JWT>
```

---

## Endpoints Principais

### Sales (Vendas)
**Modelo (resumo):**
- Venda: `Number`, `Date`, `CustomerId` + `CustomerName`, `BranchId` + `BranchName`, `Total`, `Cancelled`, `Items[]`
- Item: `ProductId` + `ProductName`, `Quantity`, `UnitPrice`, `DiscountPercent`, `Total`, `Cancelled`

#### Criar venda
```bash
curl -X POST http://localhost:5000/sales ^
  -H "Authorization: Bearer <JWT>" ^
  -H "Content-Type: application/json" ^
  -d "{
    "number": "S-1001",
    "date": "2025-08-12",
    "customerId": 1, "customerName": "Ana",
    "branchId": 1, "branchName": "Centro",
    "items": [
      { "productId": 10, "productName": "Mouse", "quantity": 4, "unitPrice": 100.00 }
    ]
  }"
```

#### Atualizar venda
```bash
curl -X PUT http://localhost:5000/sales/1 ^
  -H "Authorization: Bearer <JWT>" ^
  -H "Content-Type: application/json" ^
  -d "{
    "number": "S-1001",
    "date": "2025-08-12",
    "customerId": 1, "customerName": "Ana",
    "branchId": 1, "branchName": "Centro",
    "items": [
      { "productId": 11, "productName": "Teclado", "quantity": 10, "unitPrice": 50.00 }
    ],
    "cancelled": false
  }"
```

#### Obter por id
```bash
curl -H "Authorization: Bearer <JWT>" http://localhost:5000/sales/1
```

#### Listar (com paginação/filtro/ordem)
```bash
curl -H "Authorization: Bearer <JWT>" ^
  "http://localhost:5000/sales?_page=1&_size=10&_order=date%20desc&customer=ana&branch=centro&from=2025-08-01&to=2025-08-12"
```

#### Cancelar venda
```bash
curl -X POST http://localhost:5000/sales/1/cancel ^
  -H "Authorization: Bearer <JWT>"
```

#### Cancelar item
```bash
curl -X POST http://localhost:5000/sales/1/items/2/cancel ^
  -H "Authorization: Bearer <JWT>"
```

> **DELETE literal (opcional):** Se desejar aderir ao “CRUD literal”, exponha `DELETE /sales/{id}` fazendo “soft delete” (`Cancelled = true`) e documente no Swagger/README.

### Products / Users / Carts
Os endpoints de **Produtos**, **Usuários** e **Carrinhos** também estão disponíveis e documentados no **Swagger** (`/swagger`).  
Ex.: `GET /products`, `GET /users`, `GET /carts`, etc.

**Autorização sugerida:**
- `GET` → **User** ou **Admin**
- `POST/PUT/DELETE` → **Admin**

---

## Paginação / Filtro / Ordenação
**Query params** na listagem de vendas (`GET /sales`):

| Parâmetro | Tipo      | Exemplo           | Observação                              |
|-----------|-----------|-------------------|-----------------------------------------|
| `_page`   | `int`     | `1`               | padrão 1                                |
| `_size`   | `int`     | `10`              | máximo 100                              |
| `_order`  | `string`  | `date desc`       | `date`, `total`, `number` (+ ` desc`)   |
| `from`    | `date`    | `2025-08-01`      | filtra por data mínima (inclusiva)      |
| `to`      | `date`    | `2025-08-12`      | filtra por data máxima (inclusiva)      |
| `customer`| `string`  | `ana`             | contém (case-insensitive)               |
| `branch`  | `string`  | `centro`          | contém (case-insensitive)               |

---

## Regras de Negócio (Vendas)
- **≥ 4 itens** → **10%** de desconto  
- **10–20 itens** → **20%** de desconto  
- **> 20 itens** → **não permitido** (retorna erro)  
- **< 4 itens** → **sem desconto**  

O desconto é aplicado **por item**, e o **total da venda** soma apenas itens **não cancelados**.

---

## Eventos de Domínio (diferencial)
A API publica eventos como **logs estruturados** (sem Message Broker):
- `SaleCreated`
- `SaleModified`
- `SaleCancelled`
- `ItemCancelled`

Exemplo no log:
```
EVENT SaleCreated: { "Id": 1, "Number": "S-1001", "Total": 360.00 }
```

*(Opcional) Read Model em MongoDB*  
Se configurado (`Mongo__ConnectionString` / `Mongo__Database`), os *upserts* no read model são realizados e índices são criados para consultas rápidas (ex.: por `Number`, por `BranchId+Date`).

---

## Testes & Cobertura
**Rodar testes:**
```bash
dotnet test .	ests\DeveloperStore.UnitTests\DeveloperStore.UnitTests.csproj
```
Saída esperada:
```
Resumo do teste: total: 17; falhou: 0; bem-sucedido: 17; ignorado: 0
```

**Cobertura (opcional):**
```bash
dotnet test .	ests\DeveloperStore.UnitTests\DeveloperStore.UnitTests.csproj --collect:"XPlat Code Coverage"
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"
# abra coveragereport/index.html
```

---

## CI (GitHub Actions)
Crie `.github/workflows/dotnet.yml`:

```yaml
name: .NET CI
on: [push, pull_request]
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --configuration Release --no-restore
      - run: dotnet test ./tests/DeveloperStore.UnitTests/DeveloperStore.UnitTests.csproj --no-build --logger "trx;LogFileName=test.trx" --collect:"XPlat Code Coverage"
```

---

## Troubleshooting

**1) “CartDto needs to have a constructor with 0 args…”**  
Use DTOs de **saída** como **classes** com construtor padrão e listas inicializadas (`new()`), não *records* posicionais.

**2) 401 / 403**  
- Faça **login** e use o **Bearer** no header.  
- Para rotas administrativas, use o usuário **admin**.

**3) “duplicate key value violates unique constraint on Sales.Number”**  
- O número da venda é único. Troque `Number` ou remova a venda duplicada.

**4) Conexão Postgres**  
- Confirme que o container está de pé: `docker ps`  
- Credenciais padrão: `devstore/devstore` (DB `devstore`).

**5) Read Model (Mongo) não configurado**  
- É opcional. Se quiser usar, suba um Mongo e configure `Mongo__ConnectionString`/`Mongo__Database`.

---

## Licença
Uso educacional e de avaliação técnica.

---

### Anexos (úteis para o avaliador)
- **Credenciais Postgres:** `devstore` / `devstore` (DB `devstore`)
- **Login para gerar token (Admin):** `admin` / `Pass@123`
- **Swagger:** `http://localhost:5000/swagger`
- **Comandos principais:**
  ```bash
  docker compose up -d
  dotnet run --project .\src\DeveloperStore.Api\DeveloperStore.Api.csproj
  dotnet test .	ests\DeveloperStore.UnitTests\DeveloperStore.UnitTests.csproj
  ```
