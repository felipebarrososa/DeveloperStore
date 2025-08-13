# Patch: Sales + Mongo + MediatR

## Novidades
- **/sales** CRUD com regras de desconto (4+=10%, 10–20=20%, >20 inválido), cancelamento de venda e de item.
- **Eventos**: logs em Create/Update/Cancel/ItemCancel.
- **Read Model MongoDB** e endpoint `GET /sales/summary` (total diário por filial).
- **MediatR** Handlers para Sales.
- **ProblemDetails** simplificado com `UseExceptionHandler` (404/422/500).

## Como aplicar
1. Substitua/adicione os arquivos deste patch nas mesmas pastas do seu repo.
2. Suba o Mongo junto do Postgres:
   ```bash
   docker compose up -d
   ```
3. **Importante (DB já existente):** como usamos `EnsureCreated`, se o banco já existia você precisa recriar:
   ```bash
   docker compose down -v
   docker compose up -d
   ```
4. Rode a API:
   ```bash
   dotnet run --project src/DeveloperStore.Api/DeveloperStore.Api.csproj
   ```

## Exemplos
- Criar venda:
  ```http
  POST /sales
  Authorization: Bearer <token>
  {
    "number": "S-1001",
    "date": "2025-08-12",
    "customerId": 10,
    "customerName": "Maria",
    "branchId": 1,
    "branchName": "Centro",
    "items": [
      { "productId": 2, "productName": "Mouse", "quantity": 4, "unitPrice": 99.9 },
      { "productId": 3, "productName": "Teclado", "quantity": 12, "unitPrice": 199.0 }
    ]
  }
  ```
- Resumo:
  ```http
  GET /sales/summary?from=2025-08-01&to=2025-08-31
  ```
