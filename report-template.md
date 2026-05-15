# CommerceHub — REST vs GraphQL Comparison Report

> Generated: {{GENERATED_AT}}
> Environment: .NET 10, PostgreSQL, Kestrel, identical resource limits
> Load profile: 50 RPS ramp-up (30s) → 50 RPS sustained (60s)

---

## Executive Summary

| Dimension | REST | GraphQL | Winner |
|---|---|---|---|
| Simple fetch latency (p50) | {{S1_REST_P50}} ms | {{S1_GQL_P50}} ms | {{S1_WINNER}} |
| Deep graph latency (p50) | {{S2_REST_P50}} ms | {{S2_GQL_P50}} ms | {{S2_WINNER}} |
| Over-fetch payload size | {{S3_REST_BYTES}} B | {{S3_GQL_BYTES}} B | {{S3_WINNER}} |
| N+1 list latency (p95) | {{S4_REST_P95}} ms | {{S4_GQL_P95}} ms | {{S4_WINNER}} |
| Write+read-back latency (p50) | {{S5_REST_P50}} ms | {{S5_GQL_P50}} ms | {{S5_WINNER}} |

---

## Scenario 1 — Simple GET (single product by ID)

**Expected**: REST favorable (less protocol overhead)

| Metric | REST | GraphQL |
|---|---|---|
| Throughput (RPS) | {{S1_REST_RPS}} | {{S1_GQL_RPS}} |
| Latency p50 | {{S1_REST_P50}} ms | {{S1_GQL_P50}} ms |
| Latency p95 | {{S1_REST_P95}} ms | {{S1_GQL_P95}} ms |
| Latency p99 | {{S1_REST_P99}} ms | {{S1_GQL_P99}} ms |
| Response size (avg) | {{S1_REST_BYTES}} B | {{S1_GQL_BYTES}} B |
| Backend calls/req | {{S1_REST_CALLS}} | {{S1_GQL_CALLS}} |

---

## Scenario 2 — Deep Graph Fetch (order + customer + lines + products + categories)

**Expected**: GraphQL favorable (single request vs multiple)

| Metric | REST | GraphQL |
|---|---|---|
| Throughput (RPS) | {{S2_REST_RPS}} | {{S2_GQL_RPS}} |
| Latency p50 | {{S2_REST_P50}} ms | {{S2_GQL_P50}} ms |
| Latency p95 | {{S2_REST_P95}} ms | {{S2_GQL_P95}} ms |
| Latency p99 | {{S2_REST_P99}} ms | {{S2_GQL_P99}} ms |
| Response size (avg) | {{S2_REST_BYTES}} B | {{S2_GQL_BYTES}} B |
| Backend calls/req | {{S2_REST_CALLS}} | {{S2_GQL_CALLS}} |

---

## Scenario 3 — Over-Fetch (only name + price needed)

**Expected**: GraphQL favorable (precise field selection)

| Metric | REST | GraphQL |
|---|---|---|
| Throughput (RPS) | {{S3_REST_RPS}} | {{S3_GQL_RPS}} |
| Latency p50 | {{S3_REST_P50}} ms | {{S3_GQL_P50}} ms |
| Latency p95 | {{S3_REST_P95}} ms | {{S3_GQL_P95}} ms |
| Response size (avg) | {{S3_REST_BYTES}} B | {{S3_GQL_BYTES}} B |

---

## Scenario 4 — N+1 List (50 products × category × supplier)

**Expected**: GraphQL favorable (DataLoader batching)

| Metric | REST | GraphQL |
|---|---|---|
| Throughput (RPS) | {{S4_REST_RPS}} | {{S4_GQL_RPS}} |
| Latency p50 | {{S4_REST_P50}} ms | {{S4_GQL_P50}} ms |
| Latency p95 | {{S4_REST_P95}} ms | {{S4_GQL_P95}} ms |
| Latency p99 | {{S4_REST_P99}} ms | {{S4_GQL_P99}} ms |
| Backend calls/req | {{S4_REST_CALLS}} | {{S4_GQL_CALLS}} |

---

## Scenario 5 — Write + Read-Back (place order then deep fetch)

**Expected**: Neutral

| Metric | REST | GraphQL |
|---|---|---|
| Throughput (RPS) | {{S5_REST_RPS}} | {{S5_GQL_RPS}} |
| Latency p50 | {{S5_REST_P50}} ms | {{S5_GQL_P50}} ms |
| Latency p95 | {{S5_REST_P95}} ms | {{S5_GQL_P95}} ms |
| Latency p99 | {{S5_REST_P99}} ms | {{S5_GQL_P99}} ms |
| Response size (avg) | {{S5_REST_BYTES}} B | {{S5_GQL_BYTES}} B |

---

## Observability Comparison

| Dimension | REST | GraphQL | Notes |
|---|---|---|---|
| Spans per deep-graph request | {{OBS_REST_SPANS}} | {{OBS_GQL_SPANS}} | |
| Log events per request (avg) | {{OBS_REST_LOGS}} | {{OBS_GQL_LOGS}} | |
| Trace path complexity | {{OBS_REST_TRACE}} | {{OBS_GQL_TRACE}} | See screenshots below |

---

## Known Asymmetries

| # | Asymmetry | Impact |
|---|---|---|
| 1 | GraphQL schema validation overhead | Adds ~constant per-request cost |
| 2 | REST `?expand=` requires multiple DB queries vs GraphQL resolver tree | Different query patterns |
| 3 | DataLoader batching (GraphQL) vs sequential includes (REST) | N+1 scenario favors GraphQL |

---

## Trace Screenshots

_(Insert Aspire Dashboard screenshots here)_

### REST — Deep Graph Fetch trace
<!-- ![REST trace](./screenshots/rest-deep-trace.png) -->

### GraphQL — Deep Graph Fetch trace
<!-- ![GraphQL trace](./screenshots/graphql-deep-trace.png) -->

---

## Conclusion

{{CONCLUSION}}
