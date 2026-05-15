# CommerceHub — A Fair Testing Bed for REST vs GraphQL

> A .NET 10 reference application designed to **scientifically compare** two web API styles —
> **REST** (Minimal APIs) and **GraphQL** (HotChocolate) — over **identical business logic**.
>
> Built as the experimental platform for a master's/PhD-level research paper on API design trade-offs.

---

## Table of Contents

1. [Why this project exists](#1-why-this-project-exists)
2. [The big idea: fairness as a first-class concern](#2-the-big-idea-fairness-as-a-first-class-concern)
3. [What is being compared, exactly?](#3-what-is-being-compared-exactly)
4. [High-level architecture](#4-high-level-architecture)
5. [The domain — a small e-commerce world](#5-the-domain--a-small-e-commerce-world)
6. [Solution structure](#6-solution-structure)
7. [How REST and GraphQL share the same brain](#7-how-rest-and-graphql-share-the-same-brain)
8. [Observability: how we *see* what's happening](#8-observability-how-we-see-whats-happening)
9. [Benchmark scenarios](#9-benchmark-scenarios)
10. [How to run it](#10-how-to-run-it)
11. [How to read the results](#11-how-to-read-the-results)
12. [Bonus: PostgreSQL temporal tables](#12-bonus-postgresql-temporal-tables)
13. [Known asymmetries](#13-known-asymmetries)

---

## 1. Why this project exists

If you search the web for *"REST vs GraphQL — which is faster?"* you'll get a thousand contradictory answers. Most of them are biased because:

- The REST API was built by someone who likes REST, the GraphQL API by someone who likes GraphQL.
- They run against different databases, different machines, or different business rules.
- The benchmark measures the wrong thing (cold-start latency, a single request, no warm-up).

This project removes those biases. **The same Domain, Application and Infrastructure layers power both APIs.** The only thing that changes is the *transport* — the thin layer that translates HTTP requests into method calls.

This way, every measured difference is attributable to the API style itself, not to accidental implementation differences.

---

## 2. The big idea: fairness as a first-class concern

```
┌─────────────────────────────────────────────────────────┐
│  Domain  +  Application  +  Infrastructure  (shared)    │
│  └─ Entities, business rules, EF Core, DB queries       │
└────────────┬────────────────────────┬───────────────────┘
             │                        │
             ▼                        ▼
   ┌─────────────────┐       ┌─────────────────┐
   │  Web.RestApi    │       │  Web.GraphQLApi │
   │  (Minimal APIs) │       │  (HotChocolate) │
   └─────────────────┘       └─────────────────┘
             │                        │
             └────────┬───────────────┘
                      ▼
              same PostgreSQL DB
              same OpenTelemetry pipeline
              same Aspire host
              same .NET 10 runtime
```

**Fairness rules** (enforced by structure, not just convention):

- ✅ Shared `Domain` / `Application` / `Infrastructure` layers — no duplicated business logic.
- ✅ Same database instance, same EF Core configuration (`AsNoTracking`, same migrations).
- ✅ Same Kestrel host, same container base image (`mcr.microsoft.com/dotnet/aspnet:10.0`).
- ✅ Same OpenTelemetry pipeline exporting to the same backend.
- ✅ Functionally equivalent operations (same inputs, same outputs, same validation).
- ✅ Same HTTP version, same compression, same warm-up period.
- ✅ Any unavoidable asymmetry is **documented**, not hidden.

---

## 3. What is being compared, exactly?

Three dimensions:

| Dimension | What we measure | Why it matters |
|---|---|---|
| **Performance** | average latency, p50, p95, p99, throughput (RPS) | How fast does the user perceive the app? |
| **Data transfer** | response payload size, number of backend calls per request | How much bandwidth and DB pressure does each style cause? |
| **Observability** | spans per scenario, log events, trace path | How easy is it to diagnose a problem in production? |

Each dimension is captured automatically and exported to NBomber reports + the Aspire Dashboard.

---

## 4. High-level architecture

```
┌────────────────────────── Aspire AppHost ──────────────────────────┐
│                                                                    │
│   ┌──────────────┐    ┌─────────────────┐    ┌─────────────────┐   │
│   │  PostgreSQL  │    │  Web.RestApi    │    │ Web.GraphQLApi  │   │
│   │   (shared)   │◄───┤  (Minimal APIs) │    │ (HotChocolate)  ├──►│ Seq
│   └──────────────┘    └────────┬────────┘    └────────┬────────┘   │ (logs)
│          ▲                     │                      │            │
│          │                     │  OTLP                │  OTLP      │
│          │              ┌──────▼──────────────────────▼──────┐     │
│          └──────────────┤        Aspire Dashboard            │     │
│                         │  (traces, metrics, logs viewer)    │     │
│                         └────────────────────────────────────┘     │
└────────────────────────────────────────────────────────────────────┘
                                  ▲
                                  │ HTTP load
                                  │
                         ┌────────┴────────┐
                         │ tests/Benchmarks │
                         │   (NBomber)     │
                         └─────────────────┘
```

- **Aspire** orchestrates everything locally: starts Postgres, both APIs, and Seq.
- **OpenTelemetry** ships traces and metrics from both APIs to the Aspire Dashboard.
- **NBomber** is run on demand against the running APIs.

---

## 5. The domain — a small e-commerce world

We picked **product ordering** because it produces nicely *graph-shaped* data. That's the natural habitat where GraphQL's promised advantages (less over-fetching, fewer round-trips) should show up.

```
   Customer ──┐
              │ places
              ▼
            Order ──── OrderLines ──── Product ──── Category ──┐
                                          │                    │ self-ref
                                          │                    ▼
                                          │                ParentCategory
                                          ▼
                                      StockItems ──── Supplier
```

Graph paths to exploit in queries:
- `Order → Customer → Addresses`
- `Order → OrderLines → Product → Category → ParentCategory`
- `Product → StockItems → Supplier`

The database is seeded with **~50 products, 7 categories, 5 suppliers, 10 customers, 20 orders** at startup.

---

## 6. Solution structure

```
commerce-hub/
├── src/
│   ├── SharedKernel/                # Result type, Error type, base Entity
│   ├── Domain/                      # Pure C# entities, no dependencies
│   │   ├── Products/                # Product, Category
│   │   ├── Supplies/                # Supplier, StockItem, SupplyOrder
│   │   ├── Orders/                  # Order, OrderLine, OrderStatus
│   │   └── Customers/               # Customer, Address
│   ├── Application/                 # CQRS handlers (hand-rolled, no MediatR)
│   │   ├── Products/{Queries}       # GetProductById, GetProducts, GetHistory…
│   │   └── Orders/{Queries,Commands}
│   ├── Infrastructure/              # EF Core, repositories, DataSeeder
│   ├── Web.RestApi/                 # Minimal API endpoints
│   ├── Web.GraphQLApi/              # HotChocolate schema + DataLoaders
│   ├── Web.Shared/                  # OTel pipeline, middleware, interceptors
│   ├── Aspire.ServiceDefaults/      # Aspire defaults
│   └── Aspire.AppHost/              # Orchestration entry point
├── tests/
│   ├── Benchmarks/                  # NBomber scenarios
│   └── *.UnitTests / *.IntegrationTests / *.ArchitectureTests
├── tools/
│   └── build-report.ps1             # Aggregates NBomber CSV → report.md
├── report-template.md
└── plan.md                          # The research plan / progress log
```

---

## 7. How REST and GraphQL share the same brain

Both APIs **call the same Application-layer query/command handlers**. The transport layer is the *only* difference.

```
HTTP request                           HTTP request / GraphQL query
     │                                          │
     ▼                                          ▼
┌─────────────┐                          ┌─────────────────┐
│ REST        │                          │ GraphQL         │
│ Endpoint    │                          │ Resolver        │
│ (Minimal    │                          │ (HotChocolate)  │
│  API)       │                          │                 │
└──────┬──────┘                          └────────┬────────┘
       │                                          │
       │      same IQueryHandler / ICommandHandler│
       └────────────────────┬─────────────────────┘
                            ▼
                  ┌──────────────────────┐
                  │ Application Handler  │
                  │ (e.g. GetOrderById)  │
                  └──────────┬───────────┘
                             ▼
                  ┌──────────────────────┐
                  │ IApplicationDbContext│
                  │ (EF Core)            │
                  └──────────┬───────────┘
                             ▼
                       PostgreSQL
```

### Functional parity matrix

Every row of this table is reachable by **both** APIs with semantically identical behaviour:

| # | Operation | REST | GraphQL |
|---|---|---|---|
| 1 | Get product by id | `GET /products/{id}` | `query { productById(id) { … } }` |
| 2 | List products (filter/page) | `GET /products?…` | `query { products(where, order) { … } }` |
| 3 | Product + category + stock + supplier (deep) | `GET /products/{id}/detail` | Single nested query |
| 4 | Place order | `POST /orders` | `mutation placeOrder(input)` |
| 5 | Order detail (customer + lines + products) | `GET /orders/{id}` | Nested `query { orderById(id) { … } }` |
| 6 | Order status update | `GET /orders/{id}/status` (polling) | `subscription onOrderStatusChanged(id)` |
| 7 | Temporal history | `GET /orders/{id}/history` | `query { orderHistory(id) { … } }` |

### Specific choices that keep both sides honest

- **CQRS**: hand-rolled `IQueryHandler<,>` and `ICommandHandler<,>` (no MediatR overhead difference between sides).
- **Validation**: `FluentValidation` in the Application layer — transport-agnostic.
- **EF Core**: `AsNoTracking()` everywhere on the read side.
- **GraphQL DataLoaders**: implemented (CategoryById, SupplierById, ProductById, StockItemsByProductId) to prevent N+1. REST's equivalent is `?include=` / `?expand=` deep includes.
- **Auth**: explicitly **none** — auth would add a transport-specific concern that has nothing to do with the comparison.

---

## 8. Observability: how we *see* what's happening

The `Web.Shared` project sets up a single, identical observability pipeline for both APIs:

### Custom telemetry primitives

- **`CommerceHubDiagnostics.ApplicationSource`** — `ActivitySource` for the Application layer.
- **`CommerceHubDiagnostics.InfrastructureSource`** — `ActivitySource` for the Infrastructure layer.
- **`commerce_hub.response.bytes`** — Histogram metric for response payload size.
- **`commerce_hub.backend.calls`** — Counter metric incremented for every EF Core command executed during a request.

### How those metrics get produced

```
┌─────────────────────┐
│ Incoming HTTP req   │
└──────────┬──────────┘
           │
           ├──► PayloadSizeMiddleware (REST)
           │    └─ wraps response stream, emits response.bytes on completion
           │
           ├──► HotChocolate diagnostic listener (GraphQL)
           │    └─ same metric, tagged with the operation name
           │
           ▼
   Application handler runs
           │
           ▼
   EF Core executes SQL
           │
           ▼
  BackendCallCountingInterceptor
   └─ increments commerce_hub.backend.calls
```

This means we can answer questions like:
- *"For a deep order fetch, how many SQL queries does each API actually run?"*
- *"What's the average byte size of an over-fetch response on REST vs GraphQL?"*
- *"How much time was spent in the Application layer vs the Infrastructure layer?"*

…all *without* running NBomber. The Aspire Dashboard surfaces them live.

---

## 9. Benchmark scenarios

Five scenarios, each one targeted at a hypothesis. Each scenario runs **both** APIs side-by-side under identical load (30s ramp + 60s sustained at 50 RPS).

| # | Scenario | Hypothesis | Expected winner |
|---|---|---|---|
| 1 | Simple GET — single product by id | REST has less protocol overhead per request | REST |
| 2 | Deep graph fetch — order with customer + lines + products + categories | GraphQL collapses multiple round-trips into one | GraphQL |
| 3 | Over-fetch — only `name` + `price` needed | GraphQL only ships requested fields | GraphQL |
| 4 | N+1 list — 50 products × category × supplier | GraphQL DataLoader batches sibling fetches | GraphQL |
| 5 | Write + read-back — place order then deep fetch | Mutation overhead is roughly equal | Neutral |

The point isn't to crown a winner — it's to **measure how big the gap is in each direction** and let the data inform real-world decisions.

---

## 10. How to run it

### Prerequisites

- **.NET 10 SDK**
- **Docker Desktop** (for the PostgreSQL container)
- **PowerShell 7+** (for the report generator)

### Run the full stack

```powershell
# from the repo root
dotnet run --project src/Aspire.AppHost
```

The Aspire Dashboard opens in your browser. From there you can:
- Visit the REST Swagger UI (`/swagger`) on the `web-restapi` resource.
- Open the GraphQL Banana Cake Pop IDE (`/graphql`) on the `web-graphqlapi` resource.
- Watch live traces and metrics for both APIs.

### Run the benchmarks

In a second terminal, with the AppHost still running:

```powershell
# point the benchmark project at the running APIs
$env:REST_URL = "http://localhost:5000"
$env:GRAPHQL_URL = "http://localhost:5001"

dotnet run --project tests/Benchmarks -c Release
```

NBomber writes its raw output (HTML + CSV + JSON) to `tests/Benchmarks/reports/<scenario>/`.

### Generate the comparison report

```powershell
./tools/build-report.ps1
# produces ./report.md filled from the CSV stats
```

---

## 11. How to read the results

For each scenario you'll get a row like:

| Metric | REST | GraphQL |
|---|---|---|
| Throughput (RPS) | 49.8 | 47.2 |
| Latency p50 | 12 ms | 18 ms |
| Latency p95 | 28 ms | 41 ms |
| Response size (avg) | 1.2 kB | 0.4 kB |
| Backend calls/req | 4 | 1 |

Read this **per scenario**, not in aggregate. The whole point is that the answer changes depending on the shape of the workload. Use the Aspire Dashboard trace view for the qualitative comparison (trace path complexity, span counts).

---

## 12. Bonus: PostgreSQL temporal tables

SQL Server has `SYSTEM_VERSIONING` built in; PostgreSQL doesn't. So we implemented it manually:

```
                            ┌──────────────────┐
   UPDATE/DELETE row ───►   │ BEFORE trigger   │
                            │  (per table)     │
                            └────────┬─────────┘
                                     │
                                     ├─ stamps OLD.valid_to = now()
                                     ├─ INSERT OLD INTO {table}_history
                                     └─ stamps NEW.valid_from = now()
```

Tables with versioning: `orders`, `products`, `stock_items`. Each one has a sibling `*_history` table and a `BEFORE UPDATE OR DELETE` trigger.

Exposed in both APIs:
- REST: `GET /orders/{id}/history`, `GET /products/{id}/history`
- GraphQL: `query { orderHistory(id) }`, `query { productHistory(id) }`

This is a *stretch goal* — included so the paper can discuss whether one API style handles temporal data more ergonomically than the other.

---

## 13. Known asymmetries

No matter how careful you are, REST and GraphQL are not the same thing. Honesty matters more than pretending they are:

| # | Asymmetry | Mitigation |
|---|---|---|
| 1 | GraphQL has schema-validation overhead on every request | Documented; not bypassed |
| 2 | REST `?expand=` runs a *different* SQL query shape than GraphQL's resolver tree | Both compared with `AsNoTracking()`; SQL is observable via OTel |
| 3 | GraphQL DataLoader batches; REST cannot without a custom batch endpoint | DataLoader is enabled — this is *part of what we're measuring* |
| 4 | GraphQL responses use a slightly heavier JSON envelope (`data:`, `errors:`) | Captured in `response.bytes` metric |
| 5 | GraphQL subscriptions use WebSockets; REST uses HTTP polling | Inherent to the styles — documented |

The conclusion of the paper rests on **acknowledging these openly**, not on hiding them. Each asymmetry that affects a measurement is called out in the final `report.md`.

---

## License & citation

This project is part of academic research. If you use the benchmark methodology or code in your own work, please cite the accompanying paper (link to follow once published).

---

*Built with .NET 10, Aspire, HotChocolate 16, EF Core 10, PostgreSQL 17, OpenTelemetry, and NBomber.*
