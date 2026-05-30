Excellent — both documents reveal a lot of valuable, currently-unused outputs you can include in the thesis. Here is a structured list of what you have, where it lives, and exactly how to extract it.
1. Aspire Dashboard — trace screenshots (the "Trace Screenshots" mention)
This is the single most thesis-friendly artifact, and you already have everything wired for it. The Aspire Dashboard is the web UI that opens automatically when you run dotnet run --project src/Aspire.AppHost. It has built-in distributed tracing powered by OpenTelemetry (BackendCallCountingInterceptor.cs already configures the OTLP export).
How to capture them:
1.	Start the AppHost, wait for the dashboard to open.
2.	Open ApplicationBuilderExtensions.cs's Swagger or the GraphQL Nitro IDE and manually send one request for each scenario you want to illustrate (Simple GET, Deep Graph, N+1 List, etc.).
3.	In the dashboard's Traces tab, find that request — it appears as a "waterfall" diagram showing every span: HTTP, EF Core SQL, HotChocolate resolvers, your custom CommerceHub.Application / CommerceHub.Infrastructure spans.
4.	Click the trace, expand it, and take a screenshot.
What this gives you for the thesis:
•	A side-by-side waterfall for REST and GraphQL on the same operation visually explains why the numbers differ — REST shows one fat SQL span, GraphQL shows the resolver tree plus several smaller DataLoader-batched SQL spans.
•	It also visualises the per-layer time breakdown (transport → application → infrastructure → DB), which is one of the comparison dimensions explicitly listed in plan.md §0.
2. Backend SQL call count per request (commerce_hub.backend.calls)
BackendCallCountingInterceptor.cs registers a BackendCallCountingInterceptor that increments a counter on every EF Core SQL command. This is gold for the N+1 narrative.
How to extract it:
•	In the Aspire Dashboard → Metrics tab → pick the ApplicationBuilderExtensions.cs / appsettings.Development.json resource → find commerce_hub.backend.calls.
•	Filter by the route/operation tag to see "how many SQL calls did this single request issue?".
•	For the thesis you can present a table: "Deep Graph Fetch: REST = 1 SQL call, GraphQL with DataLoader = N SQL calls" (whatever the real numbers turn out to be).
3. Response payload size histogram (commerce_hub.response.bytes)
Both APIs feed this metric (REST via PayloadSizeMiddleware, GraphQL via the HotChocolate diagnostic listener). It is more granular than the single payload number NBomber reports.
How to extract it: Same place as #2 (Metrics tab). You get distribution (min/avg/p95/max), not just an average — useful as a chart in the thesis.
4. Per-layer duration spans
CommerceHubDiagnostics exposes two ActivitySources — CommerceHub.Application and CommerceHub.Infrastructure. Every trace already includes them.
This lets you produce a thesis chart like:
Layer	REST (ms)	GraphQL (ms)
Transport (Kestrel + parsing)	…	…
Application (handler)	…	…
Infrastructure (EF + SQL)	…	…
You read these numbers off the span durations in any single Aspire trace. It's the cleanest way to show "GraphQL spends extra time before it ever reaches the handler".
5. Seq logs (structured logs from both APIs)
AppHost includes an optional Seq container. Seq is a structured-log viewer; you can:
•	Show how many log events one request emits on each API (Phase 5 in plan.md calls this out explicitly).
•	Show a side-by-side query like RequestPath = '/products/{id}/detail' vs the GraphQL operation name — useful for the "observability comparison" row of the template.
Screenshot of a Seq query for one request on each API would be another high-value thesis figure.
6. NBomber's built-in HTML reports
NBomber doesn't only output the markdown you pasted in — it also writes a full HTML report with latency distribution charts and throughput-over-time graphs to tests/Benchmarks/reports/<scenario>/. Those charts are publication-quality and would look great in a thesis appendix; the README only shows the summary numbers.
7. GraphQL schema (SDL) export
HotChocolate can export the full schema as schema.graphql. Including a small SDL excerpt in the thesis is a nice way to show how one schema replaces many REST endpoints. You can generate it with:
dotnet tool install -g StrawberryShake.Tools  # or use HotChocolate's CLI
# then point it at the running /graphql endpoint


…or simply copy it from the Nitro IDE's Schema tab while the GraphQL API is running.
8. REST OpenAPI / Swagger document
Symmetric to #7: export swagger.json (or take a screenshot of the Swagger UI) to visually contrast "many endpoints, fixed shapes" vs the GraphQL SDL's "one endpoint, flexible shape". Already available at /swagger on ApplicationBuilderExtensions.cs.
9. EF Core generated SQL (deep-include vs DataLoader batches)
For Scenarios 2 and 4 specifically, you can capture the actual SQL each API runs:
•	The Aspire Dashboard trace shows the full SQL text in each db.statement span.
•	Or temporarily enable LogTo(Console.WriteLine, LogLevel.Information) on the DbContext.
Putting the two SQL queries side-by-side in the thesis is one of the most convincing artifacts you can produce — it directly proves the architectural difference behind the latency gap in Scenarios 2 and 4.
10. Temporal-table history queries (Phase 9 — completely undocumented in the README)
You implemented temporal tables (orders_history, products_history, stock_items_history) and exposed them in both APIs (GET /orders/{id}/history and query { orderHistory(id) }). This is currently invisible in the README and in the thesis-relevant output. Even if you don't add a 6th benchmark scenario, a short thesis section comparing how each API style models a "time-travel query" would showcase the work without much extra effort.
---
Suggested priority for the thesis
If I were prioritising, the top picks for figures in the thesis itself would be (in order):
1.	Trace waterfall screenshots (Aspire Dashboard) — REST vs GraphQL for Scenarios 2 and 4. Single best visual.
2.	NBomber HTML latency-distribution chart for Scenario 4 — the percentile collapse is striking.
3.	Side-by-side SQL that each API generated for the same logical operation.
4.	Backend-calls-per-request table from commerce_hub.backend.calls.
5.	Per-layer duration breakdown from your custom ActivitySources.
Items 6–10 are great appendix material if you want to be thorough.
