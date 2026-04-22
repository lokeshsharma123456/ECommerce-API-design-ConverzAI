# 🛒 ECommerce API — How to Run

## Tech Stack

| Layer          | Tech                                                |
|----------------|-----------------------------------------------------|
| Backend        | .NET 9 Web API, EF Core (Pomelo 9.0), NEST 7.17.5   |
| Database       | MySQL 8.0                                           |
| Search Engine  | Elasticsearch 7.17.5 (single-node, security off)    |
| Frontend       | Vanilla HTML + CSS + JS, served via Nginx           |
| Container      | Docker + Docker Compose (4 services, 1 network)     |

---

## Why These Choices?

- **Why MySQL?** — Relational product catalog with strict shape (SKU, price, stock, reviews, images). ACID guarantees matter for inventory; joins are cheap; everyone on the team already knows SQL. NoSQL (Mongo/Cosmos) would force us to denormalize reviews/images and lose referential integrity for a catalog that is *structured*, not document-shaped.
- **When to use Elasticsearch?** — Only for **user-facing search** (typos, partial matches, relevance ranking across title/brand/tags/category with field boosts). It's a read-side **projection**, not a source of truth. MySQL stays authoritative; ES is rebuilt from it on startup.
- **Why the split?** — Writes go to MySQL (consistent). Searches with a text query go to Elasticsearch (fast + fuzzy). Browse/filter with no query stays on MySQL (no need to pay the ES round-trip). The Strategy Pattern picks the right backend per request — the controller has no idea which one was used.

---

## Prerequisites

- **Docker Desktop** — installed and running
- **.NET 9 SDK** — only if you want to run the backend outside Docker
- **Git** — to clone the repo

---

## Option 1 — Docker Compose (Recommended)

This spins up **MySQL + Elasticsearch + Backend** in one command.

```bash
cd C:\Users\Lokesh Sharma\Self_learning\ConverzAI
docker-compose up --build
```

| Service         | URL / Port                        |
|-----------------|-----------------------------------|
| **Frontend UI** | http://localhost:3000             |
| Backend API     | http://localhost:5000             |
| Swagger UI      | http://localhost:5000/swagger     |
| MySQL           | localhost:3306 (user: root / dev) |
| Elasticsearch   | http://localhost:9200             |

The frontend proxies `/api/*` requests through Nginx to the backend, so there are no CORS issues.

To stop everything:

```bash
docker-compose down
```

To stop **and wipe the database volume**:

```bash
docker-compose down -v
```

---

## Option 2 — Run Backend Locally (without Docker for the API)

### Step 1: Start MySQL & Elasticsearch via Docker

```bash
docker-compose up mysql elasticsearch
```

Wait until both services are healthy (check with `docker ps`).

### Step 2: Restore & Run the .NET API

```bash
cd backend
dotnet restore src/EcommerceAPI/EcommerceAPI.csproj
dotnet run --project src/EcommerceAPI
```

The API starts at **http://localhost:5000** (or whichever port Kestrel picks — check console output).  
Swagger UI: **https://localhost:{port}/swagger**

> **Note:** The local connection string in `appsettings.json` already points to `localhost:3306`, so it works when MySQL is running via Docker.

---

## What Happens on Startup

1. **EF Core migrations** run automatically (`MigrateAsync`) — creates `Products`, `ProductImages`, `ProductTags`, `ProductReviews` tables.
2. **DummyJsonSeeder** fetches ~194 sample products from [dummyjson.com](https://dummyjson.com/products) and seeds MySQL if the table is empty (idempotent).
3. **ElasticProductIndexer.EnsureIndexAsync** creates the `products` index with a custom mapping (title as `text`+`keyword`, brand/category/tags as `keyword`, price/rating as numeric).
4. **BulkIndexAllAsync** pushes all MySQL products into Elasticsearch (skipped if the index is already populated).

---

## API Endpoints

All endpoints return JSON. Base URL: `http://localhost:5000` (direct) or `http://localhost:3000/api` (via Nginx proxy).

| Method | Route                                                  | Backend used      | Purpose                                      |
|--------|--------------------------------------------------------|-------------------|----------------------------------------------|
| GET    | `/api/products?page=1&size=20&category=beauty`         | MySQL             | Paginated browse, optional category filter   |
| GET    | `/api/products/{id}`                                   | MySQL             | Full product detail incl. images, reviews    |
| GET    | `/api/products/search?q=mascara&category=beauty&page=1`| Elasticsearch     | Full-text search with fuzziness + boosting   |
| GET    | `/api/products/categories`                             | MySQL (DISTINCT)  | List all distinct categories (for filters)   |

### Examples

```bash
# 1. Browse page 1, 20 items
curl "http://localhost:5000/api/products?page=1&size=20"

# 2. Filter by category (no text query → MySQL)
curl "http://localhost:5000/api/products?category=beauty"

# 3. Full-text search with typo tolerance (goes to Elasticsearch)
curl "http://localhost:5000/api/products/search?q=mascara"
# → returns "Essence Mascara Lash Princess" even for misspellings like "mascra"

# 4. Search + filter combined
curl "http://localhost:5000/api/products/search?q=phone&category=smartphones&size=5"

# 5. Get product detail
curl "http://localhost:5000/api/products/1"

# 6. List all categories (for dropdowns)
curl "http://localhost:5000/api/products/categories"
# → ["beauty","fragrances","furniture","groceries", ... 24 total]
```

**Which strategy runs?** The `ProductService` loops through `IEnumerable<ISearchStrategy>` and picks the first one whose `CanHandle(request)` returns `true`:
- `q` is **empty** → `MySqlSearchStrategy` wins (cheap EF Core query).
- `q` is **present** → `ElasticSearchStrategy` wins (NEST `MultiMatch` with `Fuzziness.Auto`, boosts: `title×3`, `tags×2`, `brand×2`, `category×1.5`).

---

## Useful Commands

| Task                        | Command                                                       |
|-----------------------------|---------------------------------------------------------------|
| Build only                  | `dotnet build backend/src/EcommerceAPI`                       |
| Add a migration             | `dotnet ef migrations add <Name> --project backend/src/EcommerceAPI` |
| Check Docker container logs | `docker logs ecommerce-backend`                               |
| Rebuild Docker image        | `docker-compose up --build backend`                           |

---

## Architecture

### High-Level System View

```
┌──────────────┐       ┌──────────────────────────────────────────────┐
│              │  HTTP  │              .NET 9 Backend (:5000)          │
│    Client    │───────▶│                                              │
│  (Browser /  │◀───────│  ┌──────────────────────────────────────┐   │
│   Swagger)   │  JSON  │  │         ProductsController           │   │
│              │        │  └──────────────┬───────────────────────┘   │
└──────────────┘        │                 │                            │
                        │                 ▼                            │
                        │  ┌──────────────────────────────────────┐   │
                        │  │         IProductService               │   │
                        │  │         (ProductService)              │   │
                        │  └────┬─────────────────────┬───────────┘   │
                        │       │                     │                │
                        │       ▼                     ▼                │
                        │  ┌──────────┐    ┌───────────────────┐      │
                        │  │ IProduct │    │  ISearchStrategy   │      │
                        │  │ Repo     │    │  (Strategy Pattern)│      │
                        │  └────┬─────┘    └──┬──────────┬─────┘      │
                        │       │             │          │             │
                        └───────┼─────────────┼──────────┼─────────────┘
                                │             │          │
                                ▼             ▼          ▼
                        ┌──────────────┐ ┌────────────────────┐
                        │   MySQL 8.0  │ │ Elasticsearch 7.17 │
                        │    (:3306)   │ │      (:9200)       │
                        │              │ │                    │
                        │  Products    │ │  "products" index  │
                        │  Images      │ │  (full-text search)│
                        │  Tags        │ │                    │
                        │  Reviews     │ │                    │
                        └──────────────┘ └────────────────────┘
```

### Layered Architecture (Clean Architecture)

```
┌─────────────────────────────────────────────────────────────────────┐
│  API Layer                                                          │
│  └── Controllers/ProductsController.cs                              │
│      Receives HTTP → calls Service → returns DTOs                   │
├─────────────────────────────────────────────────────────────────────┤
│  Contracts Layer (DTOs)                                             │
│  ├── ProductListItemDto.cs    (slim, 8 fields — for grid/list)     │
│  ├── ProductDetailDto.cs      (full, 20 fields — single product)   │
│  ├── ReviewDto.cs             (nested inside detail)                │
│  ├── PagedResult<T>.cs        (generic pagination wrapper)          │
│  └── ProductMappings.cs       (Entity → DTO mapping)                │
├─────────────────────────────────────────────────────────────────────┤
│  Application Layer                                                  │
│  ├── Services/                                                      │
│  │   ├── IProductService.cs          (interface)                    │
│  │   └── ProductService.cs           (orchestrates repo+strategy)   │
│  └── Strategies/                     ◄── STRATEGY PATTERN           │
│      ├── ISearchStrategy.cs          (CanHandle + SearchAsync)      │
│      ├── SearchRequest.cs            (query/category/page/size)     │
│      ├── MySqlSearchStrategy.cs      (no query → DB browse)        │
│      └── ElasticSearchStrategy.cs    (query present → ES search)   │
├─────────────────────────────────────────────────────────────────────┤
│  Domain Layer                                                       │
│  └── Entities/                                                      │
│      ├── Product.cs              (aggregate root)                   │
│      ├── ProductImage.cs         (child — cascade delete)           │
│      ├── ProductTag.cs           (child — cascade delete)           │
│      ├── ProductReview.cs        (child — cascade delete)           │
│      └── ValueObjects/                                              │
│          ├── Dimensions.cs       (flat owned cols — Width/Height/..)│
│          └── ProductMeta.cs      (flat owned cols — Barcode/QR/..)  │
├─────────────────────────────────────────────────────────────────────┤
│  Infrastructure Layer                                               │
│  ├── Persistence/                                                   │
│  │   ├── AppDbContext.cs                (EF Core DbContext)         │
│  │   ├── Migrations/                    (auto-generated)            │
│  │   └── Repositories/                                              │
│  │       ├── IProductRepository.cs      (interface)                 │
│  │       └── ProductRepository.cs       (EF Core implementation)   │
│  ├── Search/                                                        │
│  │   ├── ElasticProductIndexer.cs  (EnsureIndex/BulkIndex/Search)   │
│  │   └── ProductSearchDoc.cs       (flat ES document)               │
│  └── Seeding/                                                       │
│      ├── DummyJsonSeeder.cs             (fetches from DummyJSON)    │
│      └── DummyJsonModels.cs             (deserialization models)    │
└─────────────────────────────────────────────────────────────────────┘
```

### Request Flow

```
HTTP GET /api/products?q=phone&page=1
        │
        ▼
  ProductsController
        │
        ▼
  ProductService.SearchAsync(request)
        │
        ├── Loops through ISearchStrategy list
        │   ├── ElasticSearchStrategy.CanHandle(request) → true (q is set)
        │   └── ElasticSearchStrategy.SearchAsync(request) → PagedResult<Product>
        │
        ▼
  Map entities → ProductListItemDto[]
        │
        ▼
  Return PagedResult<ProductListItemDto> as JSON
```

### Strategy Pattern Decision

```
SearchRequest { Query, Category, Page, PageSize }
        │
        ├── Query is non-empty?
        │     YES → ElasticSearchStrategy  (full-text via Elasticsearch)
        │     NO  → MySqlSearchStrategy    (browse/filter via EF Core)
        │
        └── First strategy where CanHandle() == true wins
```

### Docker Compose Services

```
docker-compose.yml
        │
        ├── mysql          (MySQL 8.0 on :3306)
        │     ├── volume: mysqldata
        │     └── env: MYSQL_ROOT_HOST='%'  ◄── required; default is localhost-only
        │
        ├── elasticsearch  (ES 7.17.5 on :9200, single-node, security off)
        │
        ├── backend        (Dockerfile multi-stage, :5000 → :8080)
        │     ├── depends_on: mysql (healthy)
        │     └── depends_on: elasticsearch (healthy)
        │
        └── frontend       (nginx:alpine, :3000 → :80)
              └── proxies /api/ → backend:8080  (no CORS needed)
```

### Startup Sequence

```
Program.cs
    │
    ├── 1. Register DI services (DbContext, Repo, Strategies, Service)
    ├── 2. Build app
    ├── 3. Run EF Core MigrateAsync() — creates/updates tables
    ├── 4. Run DummyJsonSeeder — seeds products if table is empty
    ├── 5. Enable Swagger (Development only)
    └── 6. MapControllers → app.Run()
```

## Project Structure

```
ConverzAI/
├── docker-compose.yml          ← orchestrates all services
├── README.md                   ← this file
├── backend/
│   ├── Dockerfile                                     ← multi-stage .NET 9 build
│   └── src/EcommerceAPI/
│       ├── Api/Controllers/ProductsController.cs      ← 4 REST endpoints
│       ├── Application/
│       │   ├── Services/ProductService.cs             ← orchestrates repo + strategy
│       │   └── Strategies/                            ← STRATEGY PATTERN
│       │       ├── ISearchStrategy.cs
│       │       ├── MySqlSearchStrategy.cs             ← empty query → DB browse
│       │       └── ElasticSearchStrategy.cs           ← query present → ES search
│       ├── Contracts/                                 ← DTO records (list/detail/paged)
│       ├── Domain/Entities/Product.cs                 ← aggregate root + children
│       ├── Infrastructure/
│       │   ├── Persistence/AppDbContext.cs            ← EF Core + OwnsOne mapping
│       │   ├── Persistence/Migrations/                ← auto-generated
│       │   ├── Search/ElasticProductIndexer.cs        ← NEST index + bulk + search
│       │   └── Seeding/DummyJsonSeeder.cs             ← fetch from dummyjson.com
│       ├── Program.cs                                 ← DI wiring + startup pipeline
│       └── appsettings.json                           ← connection strings
├── frontend/
│   ├── Dockerfile                                     ← nginx:alpine
│   ├── nginx.conf                                     ← reverse-proxy /api → backend
│   ├── index.html                                     ← UI shell
│   ├── style.css
│   └── app.js                                         ← debounced search + category filter
└── skills/                                            ← Copilot skill definitions
```

---

## Architecture Decisions (ADRs)

Short, pragmatic rationale for the non-obvious choices — the "why" interviewers ask about.

| # | Decision | Why | Trade-off |
|---|----------|-----|-----------|
| 1 | **Clean Architecture** (Api → Application → Domain / Infrastructure) | Domain has zero framework references; easy to unit-test `ProductService` with a fake repo. | More folders / interfaces for a small app. Worth it for interview clarity + real-world growth. |
| 2 | **Strategy Pattern for search** (`ISearchStrategy` list, first `CanHandle` wins) | Swapping/adding a backend (e.g., OpenSearch, vector DB) means adding one class — no `if/else` in the service. | Slightly more indirection than a `switch` statement. |
| 3 | **MySQL is source of truth; ES is a projection** | ES index is rebuilt from MySQL on startup (`BulkIndexAllAsync`). If ES dies, we re-seed and continue. | First boot is slower; no incremental sync yet (future: outbox + background reindex). |
| 4 | **Pomelo over MySql.Data EF provider** | Actively maintained, handles MySQL 8 auth + timezone quirks. | Pomelo **does not support `.ToJson()`** — so `Dimensions` & `Meta` use flat owned columns instead of JSON. Acceptable; queryable too. |
| 5 | **Fixed `MySqlServerVersion(8,0,36)` instead of `AutoDetect`** | `AutoDetect` requires a live DB at migration-generation time → breaks `dotnet ef migrations add` in CI. | Must bump the version manually if MySQL is upgraded. |
| 6 | **`Swashbuckle.AspNetCore` pinned at 7.2.0** | 10.x has a broken `Microsoft.OpenApi` dep chain → `ReflectionTypeLoadException` at runtime. | Stuck on older OpenAPI surface until upstream fixes it. |
| 7 | **Nginx reverse-proxy in the frontend container** (`/api/ → backend:8080`) | Frontend and API share an origin (`:3000`), so **no CORS setup** needed in the backend. | One more container to run. Trivial vs. debugging CORS preflights. |
| 8 | **Healthcheck-gated `depends_on`** in Compose | Backend waits for MySQL + ES to actually be **healthy**, not just "started" — no retry loop needed in code. | Startup feels slow (~30s cold) but is deterministic. |
| 9 | **`MYSQL_ROOT_HOST: '%'`** env on MySQL service | MySQL 8 defaults to `root@localhost` only — backend in another container would get `Host 'x.x.x.x' is not allowed`. | Root is now accessible from any host on the compose network. Fine for dev; production uses a dedicated app user. |
| 10 | **Seeder is idempotent** (skips if `Products.Count > 0`) | Safe to restart the stack; `docker compose up` repeatedly won't duplicate data. | Forces `down -v` for a true reset. |
| 11 | **DTOs via C# `record` types + hand-rolled `ProductMappings`** | Zero reflection, zero AutoMapper config surprises, fastest JSON serialization. | Manual mapping boilerplate. Acceptable for this size. |
| 12 | **Dumb frontend (vanilla JS, 300ms debounce)** | Keeps demo easy to explain and removes Node/build toolchain from the critical path. | No SPA niceties (routing, state management). Could swap in React any time — API is unchanged. |

**Things deliberately left out** (would add next):
- `/healthz` endpoint + container healthcheck for the backend itself
- Global exception handler (`IExceptionHandler`) → consistent Problem+JSON responses
- Outbox pattern for MySQL→ES sync (today it's full reindex on startup)
- Integration tests against a Testcontainers MySQL + ES
- GitHub Actions CI (build + push images)

---

## Environment Variables (Docker Compose overrides)

| Variable                  | Default (in docker-compose)                                        |
|---------------------------|--------------------------------------------------------------------|
| `ConnectionStrings__MySql`| `server=mysql;port=3306;database=ecommerce;user=root;password=dev` |
| `Elasticsearch__Uri`      | `http://elasticsearch:9200`                                        |
| `Elasticsearch__Index`    | `products`                                                         |

---

## Troubleshooting

| Problem                                                  | Fix                                                              |
|----------------------------------------------------------|------------------------------------------------------------------|
| Port 3306 / 5000 / 9200 / 3000 already in use            | Stop the conflicting local service or remap the port in `docker-compose.yml` |
| `Host 'x.x.x.x' is not allowed to connect to MySQL`      | Ensure `MYSQL_ROOT_HOST: '%'` is set on the `mysql` service (MySQL 8 restricts root to `localhost` by default) |
| Backend exits immediately on `docker compose up`         | Check `docker compose logs backend` — usually MySQL not yet healthy; compose waits via `depends_on: condition: service_healthy` |
| `dotnet ef` not found                                    | Install: `dotnet tool install --global dotnet-ef`                |
| Elasticsearch container exits / OOM                      | Increase Docker Desktop memory to ≥ 4 GB                         |
| Swagger 500 / `ReflectionTypeLoadException`              | Keep `Swashbuckle.AspNetCore` pinned to **7.2.0** — 10.x has a broken `Microsoft.OpenApi` dependency chain |
