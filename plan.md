# CommerceHub — REST vs GraphQL Comparison Plan

> Living document. Updated as work progresses. Check off items as they complete.
> Last updated: _Phase 9 complete_

---

## 0. Goal

Build a `.NET Aspire`-orchestrated solution (`CommerceHub` — a Product Ordering System) that serves as a **fair testing bed** to compare two Web API styles in .NET 10 over **identical business logic**:

- **REST** (Minimal APIs)
- **GraphQL** (HotChocolate)

Comparison dimensions:

| Category | Metrics |
|---|---|
| **Performance** | average latency, p50, p95, throughput |
| **Data transfer** | response payload size, total number of calls to backend services |
| **Observability / diagnosis** | request duration per API layer, number of spans per scenario, number of log events, trace path through services |

---

## 1. Guiding Principles (Fairness Rules)

- [ ] Both APIs share the **exact same** Domain / Application / Infrastructure layers — no duplicated business logic.
- [ ] Both APIs hit the **same database instance** with the **same EF Core configuration**.
- [ ] Both APIs run as **separate Aspire resources** on identical hosting (Kestrel, .NET 10, same container base image).
- [ ] Both APIs share the **same OpenTelemetry pipeline** (traces, metrics, logs) exporting to the same backend.
- [ ] Both APIs expose **functionally equivalent operations** (same inputs, same outputs, same validation).
- [ ] Same HTTP version, same compression setting, same warm-up period before measurement.
- [ ] Any unavoidable asymmetry is **documented**, not hidden.

---

## 2. Target Solution Structure

```
commerce-hub/
├── src/
│   ├── Domain/                          # Entities, value objects, domain events
│   │   ├── Products/                    # Product, Category, Sku
│   │   ├── Supplies/                    # Supplier, StockItem, SupplyOrder
│   │   ├── Orders/                      # Order, OrderLine, OrderStatus
│   │   └── Customers/                   # Customer
│   ├── SharedKernel/                    # Base types (already present)
│   ├── Application/                     # CQRS handlers (MediatR or hand-rolled)
│   │   ├── Products/{Queries,Commands}
│   │   ├── Supplies/{Queries,Commands}
│   │   └── Orders/{Queries,Commands}
│   ├── Infrastructure/                  # EF Core, repositories, external services
│   ├── Web.RestApi/                     # REST endpoints (Minimal API) — exists
│   ├── Web.GraphQLApi/                  # NEW — HotChocolate-based GraphQL
│   ├── Web.Shared/                      # NEW — shared OTel setup, auth, health, etc.
│   ├── ServiceDefaults/                 # Aspire defaults
│   └── Aspire.AppHost/                  # Orchestration — exists
└── tests/
    ├── Benchmarks/                      # NBomber scenarios — NEW
    ├── *.UnitTests
    └── *.IntegrationTests
```

---

## 3. Domain Model (Product Ordering System)

Entities chosen to produce graph-shaped data so REST over-/under-fetching is visible:

- **Product** — Id, Name, Sku, Description, Price, CategoryId, StockItems
- **Category** — Id, Name, ParentCategoryId (self-referential → nested queries)
- **Supplier** — Id, Name, ContactInfo
- **StockItem** — ProductId, SupplierId, QuantityOnHand, ReorderLevel
- **SupplyOrder** — Id, SupplierId, Lines, Status (Pending/Received)
- **Customer** — Id, Name, Email, Addresses
- **Order** — Id, CustomerId, OrderLines, Status, PlacedAt, Total
- **OrderLine** — ProductId, Quantity, UnitPriceAtOrder

Graph paths to exploit: `Order → Customer → Addresses`, `Order → OrderLines → Product → Category → ParentCategory`, `Product → StockItems → Supplier`.

---

## 4. API Surface — Functional Parity Matrix

| # | Operation | REST | GraphQL |
|---|---|---|---|
| 1 | Get product by id | `GET /products/{id}` | `query { product(id) { … } }` |
| 2 | List products (filter/page) | `GET /products?category=&page=` | `query { products(filter, paging) { … } }` |
| 3 | Product + category + stock + supplier (deep) | Multiple calls or `?include=` | Single nested query |
| 4 | Place order | `POST /orders` | `mutation placeOrder(input)` |
| 5 | Order detail with customer + lines + products | `GET /orders/{id}?expand=…` | Nested `query { order(id) { … } }` |
| 6 | Order status update (optional) | `GET /orders/{id}/status` polling | `subscription orderStatus(id)` |

Each row gets a **canonical scenario** the benchmark fetches identically on both sides.

---

## 5. Cross-Cutting Setup (`Web.Shared`)

- [ ] OpenTelemetry: ASP.NET Core + HttpClient + EF Core + HotChocolate instrumentation, OTLP export to Aspire dashboard.
- [ ] Custom `ActivitySource` per layer: `CommerceHub.Application`, `CommerceHub.Infrastructure` — enables per-layer duration measurement.
- [ ] Custom `Meter` for: response payload size histogram, backend-call counter per request.
- [ ] `PayloadSizeMiddleware` (REST) + HotChocolate diagnostic event listener (GraphQL) → `commerce_hub.response.bytes` metric tagged by operation.
- [ ] Request-scoped backend-call counter (EF command executed / outbound HTTP) → `commerce_hub.backend.calls` per request.

---

## 6. Benchmark Scenarios (`tests/Benchmarks`)

Tooling: **NBomber** (.NET-native, Aspire-friendly). Alt: k6.

| # | Scenario | Expected favorable side |
|---|---|---|
| 1 | Simple GET — single product by id | REST (less overhead) |
| 2 | Deep graph fetch — order with customer/lines/products/categories | GraphQL |
| 3 | Over-fetch — need only name + price | GraphQL |
| 4 | N+1 list — 50 products × category × supplier | GraphQL (DataLoader) |
| 5 | Write + read-back — place order then deep fetch | Neutral |

Load levels per scenario: **50 / 200 / 500 RPS** for 60s, after 30s warm-up.

Collect:

| Metric | Source |
|---|---|
| Latency p50/p95/p99, throughput | NBomber report |
| Response payload bytes | Custom OTel metric |
| Backend call count per request | Custom OTel metric |
| Per-layer duration | Custom `ActivitySource` spans |
| Spans / logs per scenario | Aspire Dashboard / OTLP backend query |
| Trace path | Aspire Dashboard trace view |

---

## 7. Aspire `AppHost` Composition

```
AppHost
├── postgres                   ─── shared DB (Aspire PostgreSQL resource)
├── seq (optional)            ─── log/trace backend
├── web-rest                  → references postgres, seq
├── web-graphql               → references postgres, seq
└── benchmarks                → references web-rest, web-graphql (on-demand)
```

Both APIs receive **identical** connection string and OTel endpoint from Aspire.

---

## 8. Technology Choices

- **GraphQL**: HotChocolate 14+ (OTel built-in, DataLoader, filtering/sorting/paging conventions).
- **REST**: Minimal APIs (current style).
- **ORM**: EF Core 10 — `AsNoTracking`, compiled queries where useful — identical on both sides.
- **CQRS dispatch**: hand-rolled handlers (no MediatR).
- **Validation**: FluentValidation in Application layer, transport-agnostic.
- **Auth**: none — removed from the template.
- **Database**: PostgreSQL (Aspire `AddPostgres`).
- **Container base**: `mcr.microsoft.com/dotnet/aspnet:10.0` for both.

---

## 9. Implementation Phases & Progress

Legend: ⬜ todo · 🟡 in progress · ✅ done · ⏸ blocked

### Phase 1 — Domain reset
- ✅ Remove all template `User`/auth code: `Domain/Users/*`, `Application/Users/*`, `Web.RestApi/Endpoints/Users/*`, auth services & registrations, JWT config in `appsettings*.json`.
- ✅ Remove auth middleware/DI calls from `Web.RestApi/Program.cs` and `DependencyInjection.cs`.
- ✅ Add `Product`, `Category`, `Sku` under `Domain/Products`.
- ✅ Add `Supplier`, `StockItem`, `SupplyOrder` under `Domain/Supplies`.
- ✅ Add `Order`, `OrderLine`, `OrderStatus` under `Domain/Orders`.
- ✅ Add `Customer`, `Address` under `Domain/Customers`.
- ✅ EF Core configurations + `InitialCreate` migration against Postgres.
- ✅ Seed data (a few categories, ~50 products, ~5 suppliers, ~10 customers, ~20 orders).

### Phase 2 — Application layer
- ✅ Handlers for parity matrix §4 rows 1–5. Pure C#, transport-agnostic.
- ✅ DTOs/response shapes (shared by both APIs).
- ✅ FluentValidation for `PlaceOrderCommand`.

### Phase 3 — REST surface (`Web.RestApi`)
- ✅ Replace `Users` endpoints with `Products`, `Orders`, `Customers` endpoints.
- ✅ Implement `?expand=` / `?include=` for deep fetch parity (row 3, 5).
- ✅ Add `GET /orders/{id}/status` polling endpoint (parity row 6).
- ✅ Confirm Swagger/OpenAPI still generated.

### Phase 4 — GraphQL surface (`Web.GraphQLApi`)
- ✅ Create project, add HotChocolate 16.0.2.
- ✅ Schema types mirroring DTOs.
- ✅ Resolvers call the same Application handlers.
- ✅ Configure DataLoader for N+1 scenarios.
- ✅ Enable filtering/sorting/paging conventions.
- ✅ Add `subscription orderStatus(id)` (parity row 6).

### Phase 5 — Web.Shared instrumentation
- ✅ Create `Web.Shared` project.
- ✅ Centralized `AddCommerceHubObservability(...)` extension.
- ✅ Per-layer `ActivitySource`s wired into Application/Infrastructure.
- ✅ Payload-size metric (middleware + HotChocolate listener).
- ✅ Backend-call counter (EF interceptor + `HttpClient` handler).

### Phase 6 — Aspire `AppHost` wiring
- ✅ Add Postgres resource.
- ✅ Add Seq (optional).
- ✅ Register both APIs with reference to DB + OTel.
- ✅ Configure identical resource limits (CPU/memory) for both APIs.

### Phase 7 — Benchmarks
- ✅ Create `tests/Benchmarks` project (NBomber 6.4.0 + NBomber.Http 6.2.0).
- ✅ Implement 5 scenarios × 2 APIs (REST + GraphQL).
- ✅ Warm-up step before measurement (30s ramp).
- ✅ CSV/JSON export of results (NBomber default HTML+CSV).

### Phase 8 — Reporting
- ✅ `report-template.md` with comparison tables (latency, throughput, payload, calls, spans).
- ✅ Script (`tools/build-report.ps1`) to ingest NBomber output + OTel queries → produce filled report.
- ⬜ Capture screenshots of Aspire trace views for the qualitative trace-path comparison.

### Phase 9 — Postgres temporal tables (stretch)
- ✅ Enable `system_versioning` / temporal history on key tables (`orders`, `products`, `stock_items`).
- ✅ EF Core migration adding period columns (`valid_from`, `valid_to`) and history tables.
- ✅ Query helpers for "as-of" / "between" temporal queries.
- ✅ Expose optional temporal query in both APIs (e.g., `GET /orders/{id}/history`, `query { orderHistory(id, asOf) }`).

---

## 10. Pitfalls to Avoid (Fairness Checklist)

- ❌ Response compression enabled on only one side.
- ❌ Different HTTP versions (HTTP/1.1 vs HTTP/2).
- ❌ GraphQL DataLoader without an equivalent batch endpoint or documented note on REST.
- ❌ Measuring cold start — always warm up.
- ❌ Different EF Core tracking behavior between the two.
- ✅ Document every unavoidable asymmetry explicitly in the final report.

---

## 11. Decisions (locked-in)

- [x] **Database**: **PostgreSQL** (Aspire `Aspire.Hosting.PostgreSQL` resource).
- [x] **CQRS dispatch**: **hand-rolled handlers** (no MediatR).
- [x] **Auth**: **none** — remove all existing `User`/auth logic carried over from the template; it has no role in the REST-vs-GraphQL comparison.
- [x] **Subscriptions / SSE**: **included** — implement parity-matrix row 6 (REST polling vs GraphQL subscription).
- [x] **Benchmark tool**: **NBomber**.

### Deferred / stretch goals (post-comparison)

- ⬜ **Postgres temporal tables** — promoted to **Phase 9** above. Will implement after the comparison is complete and benchmarked.

---

## 12. Progress Log

| Date | Change |
|---|---|
| _today_ | Initial plan committed. |
| _today_ | Decisions locked in: Postgres, hand-rolled handlers, no auth, include subscriptions, NBomber. Added stretch goal: Postgres temporal tables. |
| _today_ | **Phase 1 complete.** Removed all Users/Todos/Auth template code. Added domain entities (Product, Category, Supplier, StockItem, SupplyOrder, Customer, Address, Order, OrderLine). EF configurations created. DataSeeder with ~50 products, 7 categories, 5 suppliers, 10 customers, 20 orders. Build green. |
| _today_ | **Phase 2 complete.** Application layer handlers: GetProductById, GetProducts (paged/filtered), GetProductDetail (deep with category + stock + supplier), PlaceOrder (with validation), GetOrderById (detail), GetOrderStatus. DTOs shared across APIs. Migration `InitialCreate` generated. Local dotnet-ef 10.0.4 tool manifest added. Build green. |
| _today_ | **Phase 3 complete.** REST endpoints: `GET /products`, `GET /products/{id}`, `GET /products/{id}/detail` (deep), `POST /orders`, `GET /orders/{id}`, `GET /orders/{id}/status`. Swagger still functional. Build green. |
| _today_ | **Phase 4 complete.** `Web.GraphQLApi` project created with HotChocolate 16.0.2. Types: ProductType, CategoryType, OrderType, OrderLineType, CustomerType, StockItemType, SupplierType. Query (products filtered/sorted/projected, productById deep, orderById deep, orderStatus). Mutation (placeOrder). Subscription (onOrderStatusChanged). Filtering/sorting/projections enabled. Wired into Aspire AppHost. Build green. DataLoader deferred (add when benchmarking N+1). |
| _today_ | **Phase 5 complete.** `Web.Shared` project: `CommerceHubDiagnostics` (ActivitySources for Application/Infrastructure layers, Meter with response-size histogram + backend-call counter), `ObservabilityExtensions.AddCommerceHubObservability()`, `PayloadSizeMiddleware`, `BackendCallCountingInterceptor`. Both APIs wired: interceptor via DI into EF DbContext, middleware in REST pipeline, OTel tracing+metrics with OTLP export. Build green. |
| _today_ | **Phase 6 complete.** Aspire AppHost already had Postgres + both APIs wired. Marked done. |
| _today_ | **Phase 7 complete.** `tests/Benchmarks` project with NBomber 6.4.0. 5 scenarios: SimpleGet, DeepGraph, OverFetch, N+1List, WriteReadBack. Each runs REST + GraphQL side-by-side. Config via env vars (REST_URL, GRAPHQL_URL, BENCHMARK_PRODUCT_ID, etc.). Auto-discovers product IDs from REST. Reports to `./reports/`. Build green. |
| _today_ | **Phase 4 & 6 remaining items complete.** DataLoaders added (CategoryById, SupplierById, ProductById, StockItemsByProductId) wired into ProductType, StockItemType, OrderLineType resolvers. Seq container added to AppHost with ingestion + UI endpoints; both APIs receive SEQ_URL. Build green. |
| _today_ | **Phase 8 complete.** `report-template.md` with comparison tables for all 5 scenarios. `tools/build-report.ps1` script ingests NBomber CSV output and fills the template. |
| _today_ | **Phase 9 complete.** Temporal tables for PostgreSQL: `valid_from`/`valid_to` period columns on Order, Product, StockItem. History tables (`orders_history`, `products_history`, `stock_items_history`) with BEFORE UPDATE/DELETE triggers for versioning. Query handlers: GetOrderHistory, GetProductHistory. REST endpoints: `GET /orders/{id}/history`, `GET /products/{id}/history`. GraphQL queries: `orderHistory(id)`, `productHistory(id)`. Build green. |

