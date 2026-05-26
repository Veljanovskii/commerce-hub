# CommerceHub

A .NET 10 Aspire application built as the experimental platform for a **master's thesis** comparing **REST (Minimal APIs)** and **GraphQL (HotChocolate)** web API styles over identical business logic.

> Stack: .NET 10 · Aspire · HotChocolate 16 · EF Core 10 · PostgreSQL 17 · OpenTelemetry · NBomber

---

## Motivation

The thesis investigates measurable differences between REST and GraphQL when both operate on the same domain, same database, and same runtime. Most existing comparisons are unreliable because the two APIs are built by different teams, against different data, with different optimisation efforts. CommerceHub eliminates those variables by making the two API layers **thin transports over a single shared core**.

---

## Fairness guarantees

The comparison is meaningful only if the playing field is level. These constraints are enforced structurally:

- Both APIs share the same `Domain`, `Application`, and `Infrastructure` projects -- zero duplicated logic.
- Both connect to the same PostgreSQL instance with the same EF Core configuration (`AsNoTracking` on all reads).
- Both run as separate Aspire resources on the same Kestrel/.NET 10 runtime.
- Both export telemetry through the same OpenTelemetry pipeline to the Aspire Dashboard.
- Functionally equivalent operations (same inputs, same outputs, same validation rules).
- No authentication -- it would introduce a transport-specific variable unrelated to the comparison.
- Any unavoidable asymmetry is documented (see [Known asymmetries](#known-asymmetries)).

---

## Architecture

The system is orchestrated locally by .NET Aspire. The `Aspire.AppHost` project starts a PostgreSQL container and both API projects as separate resources. Both APIs receive their connection string from Aspire and export OpenTelemetry (OTLP) signals to the Aspire Dashboard.

```
+------------------------------------------------------------------+
|                         Aspire AppHost                            |
|                                                                  |
|  +--------------+     +---------------+     +-----------------+  |
|  |  PostgreSQL  |<----|  Web.RestApi   |     | Web.GraphQLApi  |  |
|  |              |<----|               |     |                 |  |
|  +--------------+     +-------+-------+     +--------+--------+  |
|                               |                      |           |
|                               |  OTLP                |  OTLP     |
|                               v                      v           |
|                     +--------------------------------+           |
|                     |       Aspire Dashboard         |           |
|                     |   (traces, metrics, logs)      |           |
|                     +--------------------------------+           |
+------------------------------------------------------------------+
```

Internally, both APIs call into the same application layer:

```
HTTP Request                            GraphQL Query
     |                                       |
     v                                       v
+--------------+                     +------------------+
| REST Endpoint|                     | GraphQL Resolver |
| (Minimal API)|                     |  (HotChocolate)  |
+------+-------+                     +--------+---------+
       |                                      |
       |    IQueryHandler / ICommandHandler    |
       +------------------+-------------------+
                          v
              +------------------------+
              |   Application Layer    |
              |  (CQRS handlers, DTOs) |
              +-----------+------------+
                          v
              +------------------------+
              |  Infrastructure Layer  |
              |   (EF Core, DbContext) |
              +-----------+------------+
                          v
                     PostgreSQL
```

---

## Domain model

The domain models a small **product ordering system** -- chosen because it produces naturally graph-shaped data where GraphQL's advantages (fewer round-trips, reduced over-fetching) should be most visible.

### Entity relationship diagram

```
+------------------+         +----------------------+
|     Category     |         |       Supplier       |
+------------------+         +----------------------+
| Id: Guid         |<--+     | Id: Guid             |
| Name: string     |   |     | Name: string         |
| ParentCategoryId |---+     | ContactEmail: string |
| ParentCategory?  | self-   | ContactPhone: string |
| SubCategories[]  | ref     +----------+-----------+
| Products[]       |                    |
+--------+---------+                    | supplies
         | categorises                  |
         v                              v
+----------------------+     +----------------------+
|       Product        |     |      StockItem       |
+----------------------+     +----------------------+
| Id: Guid             |<----| Id: Guid             |
| Name: string         |     | ProductId: Guid      |
| Sku: string          |     | SupplierId: Guid     |
| Description: string  |     | QuantityOnHand: int  |
| Price: decimal       |     | ReorderLevel: int    |
| CategoryId: Guid     |     +----------------------+
| StockItems[]         |
+----------+-----------+
           | ordered in
           v
+----------------------+     +----------------------+
|      OrderLine       |     |       Customer       |
+----------------------+     +----------------------+
| Id: Guid             |     | Id: Guid             |
| OrderId: Guid        |     | Name: string         |
| ProductId: Guid      |     | Email: string        |
| Quantity: int        |     | Addresses[]          |
| UnitPriceAtOrder:    |     +----------+-----------+
|   decimal            |                | places
+----------+-----------+                |
           | belongs to                 v
           v                 +----------------------+
+----------------------+     |        Order         |
|       Address        |     +----------------------+
+----------------------+     | Id: Guid             |
| Id: Guid             |     | CustomerId: Guid     |
| CustomerId: Guid     |     | Status: OrderStatus  |
| Street: string       |     | PlacedAt: DateTime   |
| City: string         |     | Total: decimal       |
| PostalCode: string   |     | OrderLines[]         |
| Country: string      |     +----------------------+
+----------------------+

OrderStatus: Pending | Confirmed | Shipped | Delivered | Cancelled
```

### Key graph paths exploited in benchmarks

- `Order -> Customer -> Addresses`
- `Order -> OrderLines -> Product -> Category -> ParentCategory`
- `Product -> StockItems -> Supplier`

These multi-level paths are where GraphQL's single-request nested fetching and DataLoader batching are hypothesised to outperform REST's multiple calls or over-fetched deep includes.

---

## Data seeding

The database is seeded on first startup (only if the `Products` and `Customers` tables are empty). The seeding is deterministic in structure but uses `Guid.NewGuid()` for IDs.

| Entity | Count | Generation logic |
|---|---|---|
| **Categories** | 7 | 3 root categories (`Electronics`, `Clothing`, `Home & Garden`) + 4 children (`Computers`, `Phones`, `Men's Clothing`, `Women's Clothing`) |
| **Suppliers** | 5 | Named `Supplier 1`–`5` with generated contact info |
| **Products** | 50 | Named `Product 1`–`50`, each assigned a category by round-robin over `[Computers, Computers, Phones, Phones, Men's, Women's, Home]`. Price = `10 + i * 3.5` |
| **StockItems** | ~67 | Every product gets 1 stock item (supplier assigned round-robin). Every 3rd product gets a second stock item from the next supplier |
| **Customers** | 10 | Each with 1 address |
| **Orders** | 20 | Distributed across customers round-robin. Each order has 1–4 lines (cycling). Products selected by `(i * 3 + j) % 50`. Status cycles through all `OrderStatus` values. Dates span the last 20 days |

This gives a dataset large enough to produce realistic query plans and DataLoader batching behaviour, while remaining small enough to run in a local Docker container.

---

## How both APIs expose the same operations

Both API layers call the same `IQueryHandler<TQuery, TResponse>` and `ICommandHandler<TCommand, TResponse>` implementations registered in the Application layer. The CQRS dispatch is hand-rolled (no MediatR) and decorated with logging and FluentValidation.

| # | Operation | REST endpoint | GraphQL operation |
|---|---|---|---|
| 1 | Get product by id | `GET /products/{id}` | `query { productById(id) { ... } }` |
| 2 | List products (filter/page) | `GET /products?page=&pageSize=` | `query { products(where, order) { ... } }` |
| 3 | Deep product detail | `GET /products/{id}/detail` | Single nested query on `productById` |
| 4 | Place order | `POST /orders` | `mutation placeOrder(input)` |
| 5 | Order detail (customer + lines + products) | `GET /orders/{id}` | `query { orderById(id) { ... } }` |
| 6 | Order status | `GET /orders/{id}/status` (polling) | `subscription onOrderStatusChanged(id)` |
| 7 | Product list with deep includes | `GET /products?pageSize=50` (flat DTO) | `query { productsWithDetails { ... } }` |

**GraphQL-specific optimisations:**
- **DataLoaders** (`CategoryByIdDataLoader`, `SupplierByIdDataLoader`, `ProductByIdDataLoader`, `StockItemsByProductIdDataLoader`) batch sibling entity fetches to prevent N+1 queries.
- **Filtering/Sorting** via HotChocolate conventions (`[UseFiltering]`, `[UseSorting]`).
- **Projection** via `[UseProjection]` on list queries for server-side column selection.

**REST-specific patterns:**
- Deep includes via dedicated endpoints (e.g., `/products/{id}/detail` loads category + stock + supplier in a single query with EF `.Include()`).
- Pagination via `?page=&pageSize=` query parameters.

---

## Observability

Both APIs share a single OpenTelemetry pipeline configured in the `Web.Shared` project. This is **not** used for the benchmark measurements themselves (NBomber handles that), but provides a way to **qualitatively inspect** what each API style does under the hood during development and in the Aspire Dashboard.

### What is collected

| Signal | Name | Purpose |
|---|---|---|
| Trace | `CommerceHub.Application` ActivitySource | Shows time spent in application handlers |
| Trace | `CommerceHub.Infrastructure` ActivitySource | Shows time spent in EF Core / DB |
| Metric | `commerce_hub.response.bytes` (histogram) | Records response payload size per request, tagged by route/operation |
| Metric | `commerce_hub.backend.calls` (counter) | Counts SQL commands executed per request via an EF Core interceptor |

### How it works

- `PayloadSizeMiddleware` (REST) wraps the response stream and records bytes written on completion.
- A HotChocolate diagnostic event listener records the same metric for GraphQL, tagged with the operation name.
- `BackendCallCountingInterceptor` (EF Core `DbCommandInterceptor`) increments the backend calls counter on every `ReaderExecuting` / `NonQueryExecuting` / `ScalarExecuting` event.

This allows answering questions like:
- *"How many SQL queries does each API actually run for a deep order fetch?"*
- *"What's the response payload size when REST returns the full DTO vs. GraphQL returning only requested fields?"*

...directly in the Aspire Dashboard without running load tests.

---

## Benchmark scenarios (NBomber)

**[NBomber](https://nbomber.com/)** is a .NET load-testing framework used to drive controlled, reproducible HTTP traffic against both APIs simultaneously and capture latency, throughput, and error metrics.

Each scenario runs both the REST and GraphQL variants under **identical load** -- same RPS, same duration, same ramp profile -- so the results are directly comparable.

*Detailed results and analysis will be provided in `report.md` after benchmark runs.*

| # | Scenario | What it tests | Hypothesis |
|---|---|---|---|
| 1 | **Simple GET** -- single product by id | Baseline per-request overhead | REST is faster (less protocol machinery) |
| 2 | **Deep graph fetch** -- order with customer, lines, products, categories | Multi-level nested data retrieval | GraphQL faster (single request vs. pre-composed JOIN) |
| 3 | **Over-fetch** -- client only needs `name` + `price` | Response payload efficiency | GraphQL transfers less data |
| 4 | **N+1 list** -- all products with category + stock + supplier | DataLoader batching vs. flat DTO | GraphQL's DataLoader reduces backend calls |
| 5 | **Write + read-back** -- place order then fetch it | Mutation/write overhead comparison | Roughly neutral |

---

## Solution layout

```
src/
  SharedKernel/            Result<T>, Error, base Entity
  Domain/                  Pure entities (Products, Supplies, Orders, Customers)
  Application/             Hand-rolled CQRS handlers, DTOs, FluentValidation
  Infrastructure/          EF Core DbContext, configurations, DataSeeder, interceptor
  Web.RestApi/             Minimal API endpoints
  Web.GraphQLApi/          HotChocolate schema, resolvers, DataLoaders, types
  Web.Shared/              OTel pipeline, PayloadSizeMiddleware, ActivitySources
  Aspire.ServiceDefaults/  Aspire service defaults
  Aspire.AppHost/          Orchestration entry point
tests/
  Benchmarks/              NBomber scenarios and configuration
  *.UnitTests / *.IntegrationTests / *.ArchitectureTests
tools/
  build-report.ps1         Aggregates NBomber CSV output into report.md
report-template.md
plan.md
```

---

## Running it

**Prerequisites:** .NET 10 SDK, Docker Desktop, PowerShell 7+.

### 1. Start the full stack

```powershell
dotnet run --project src/Aspire.AppHost
```

The Aspire Dashboard opens in your browser. From there:
- REST Swagger UI at `/swagger` on the `web-restapi` resource.
- GraphQL Nitro IDE at `/graphql` on the `web-graphqlapi` resource.
- Live traces and metrics for both APIs.

### 2. Run the benchmarks

In a second terminal, with the AppHost still running:

```powershell
$env:REST_URL    = "http://localhost:5269"
$env:GRAPHQL_URL = "http://localhost:5288"

dotnet run --project tests/Benchmarks -c Release
```

The benchmark runner auto-discovers product, order, and customer IDs from the running APIs. NBomber writes HTML + CSV + JSON reports to `tests/Benchmarks/reports/<scenario>/`.

### 3. Build the comparison report

```powershell
./tools/build-report.ps1
```

Ingests NBomber CSVs and fills `report-template.md` into `report.md`.

---

## Known asymmetries

REST and GraphQL are fundamentally different protocols. These differences cannot be eliminated -- only documented honestly:

| # | Asymmetry | How it is handled |
|---|---|---|
| 1 | GraphQL parses and validates the schema on every request | Documented; not bypassed |
| 2 | REST deep-include endpoints produce different SQL shapes than GraphQL's resolver tree | Both use `AsNoTracking()`; SQL observable via OTel |
| 3 | GraphQL DataLoader batches N+1; REST uses pre-composed JOINs | DataLoader is enabled -- this is part of what we measure |
| 4 | GraphQL responses carry a heavier JSON envelope (`"data":`, `"errors":`) | Captured by `commerce_hub.response.bytes` metric |
| 5 | GraphQL subscriptions use WebSockets; REST status uses HTTP polling | Inherent to the styles -- documented |
