# ConverzAI — E-Commerce API Build Skill

**Purpose:** Recovery + learning artifact. Captures everything built so far, why, and what's next. If a new chat starts, read this file first.

**Repo:** https://github.com/sharmalokesh_microsoft/ECommerce-API-design (branch: `main`)
**Root:** `C:\Users\Lokesh Sharma\Self_learning\ConverzAI`
**Data shape:** dummyjson.com `/products` schema
**Stack:** .NET 9 · EF Core 9 · Pomelo MySQL 9 · NEST 7.17.5 (Elasticsearch) · Swagger · Docker · minimal HTML/JS frontend
**Teaching style:** Socratic — AI asks "Lokesh, what's next?", gives hints on wrong answers, commits after each working step.

---

## 1. Architecture (Clean Architecture + Strategy Pattern)

```
Controller  →  IProductService  →  ProductService
                                       │
                          ┌────────────┴────────────┐
                          ▼                         ▼
                 IProductRepository        IEnumerable<ISearchStrategy>
                  (direct lookups)           (CanHandle + SearchAsync)
                          │                   │            │
                          ▼                   ▼            ▼
                    MySQL (EF Core)     MySqlStrategy  ElasticStrategy
```

**Rules:**
- Direct lookups (by id, categories list) → repository.
- Search (query/filter) → strategy pattern. Service picks first strategy where `CanHandle(req) == true`. Throws if none match (no silent fallback).
- Entities never leak past the Application layer — everything returned to Controller is a DTO.

---

## 2. Folder Structure

```
ConverzAI/
├── backend/
│   ├── EcommerceAPI.slnx
│   └── src/EcommerceAPI/
│       ├── EcommerceAPI.csproj        (net10.0)
│       ├── Domain/
│       │   └── Entities/
│       │       ├── Product.cs
│       │       ├── ProductImage.cs
│       │       ├── ProductTag.cs
│       │       ├── ProductReview.cs
│       │       └── ValueObjects/
│       │           ├── Dimensions.cs
│       │           └── ProductMeta.cs
│       ├── Application/
│       │   ├── Services/
│       │   │   ├── IProductService.cs
│       │   │   └── ProductService.cs
│       │   └── Strategies/
│       │       ├── SearchRequest.cs
│       │       ├── ISearchStrategy.cs
│       │       ├── MySqlSearchStrategy.cs
│       │       └── ElasticSearchStrategy.cs   (stub)
│       ├── Infrastructure/
│       │   └── Persistence/
│       │       ├── AppDbContext.cs
│       │       └── Repositories/
│       │           ├── IProductRepository.cs
│       │           └── ProductRepository.cs
│       ├── Contracts/
│       │   ├── Dtos/
│       │   │   ├── ProductListItemDto.cs
│       │   │   ├── ProductDetailDto.cs
│       │   │   ├── ReviewDto.cs
│       │   │   └── PagedResult.cs
│       │   └── Mappings/
│       │       └── ProductMappings.cs
│       └── API/                       (not yet created)
├── frontend/                          (not yet created)
├── infra/                             (not yet created)
├── .gitignore                         (excludes .copilot-context.md)
└── .copilot-context.md                (local notes, gitignored)
```

---

## 3. Key Design Decisions

| Decision | Rationale |
|---|---|
| **net9.0** TFM | Pomelo MySQL only supports EF Core 9 currently. |
| **`decimal` for Price/Discount** | Money must not use binary float. `(18,2)` precision. |
| **`double` for Rating/Weight** | Non-financial, small rounding acceptable. |
| **Value objects via `OwnsOne().ToJson()`** | `Dimensions` and `ProductMeta` stored as JSON columns — no extra tables, still strongly typed. |
| **Cascade delete** on Images/Tags/Reviews | Children have no life without parent Product. |
| **`AsNoTracking()`** in repo reads | Read-only queries skip change tracker → faster. |
| **Selective `Include`** (only for detail) | List queries don't pull children they won't show. |
| **Strategy Pattern for search** | OCP: adding Elasticsearch must not edit MySQL code. `CanHandle` + `SearchAsync` interface. |
| **No silent fallback** | If no strategy matches, throw. Explicit > surprising. |
| **DTOs in `Contracts/` + extension-method mappers** | Entities never reach controller. No AutoMapper — manual mapping is explicit and educational. |
| **Slim vs full DTO** | `ProductListItemDto` (~8 fields) for grids, `ProductDetailDto` (~20 fields) for detail page — prevents over-fetching on list endpoints. |

---

## 4. Roadmap

| Layer | Status | Commit |
|---|---|---|
| Domain | ✅ | 83d9037 |
| Infrastructure/Persistence | ✅ | 83d9037 |
| Application (Services + Strategies) | ✅ | b3a7f5c |
| Contracts + DTO flow | ✅ | 3dfc8ae |
| **Api (Controllers)** | ✅ | 6a6e6e2 |
| **Program.cs DI wiring** | ✅ | 6a6e6e2 |
| Infrastructure/Search (real ES) | ⏳ next | |
| Seeder | ⏳ | |
| Docker + frontend | ⏳ | |

---

## 7. Teaching Protocol (for AI continuity)

- Never dump the next step's code upfront. Ask: **"Lokesh, what's next?"**
- On wrong/partial answer: give a hint, not the solution.
- After user writes code: review → suggest improvement if needed → commit with conventional commit message → push.
- Update this SKILL.md after each completed layer.
