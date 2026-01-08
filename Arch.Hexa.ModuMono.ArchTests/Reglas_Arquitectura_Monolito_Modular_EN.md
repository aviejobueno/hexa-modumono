# Architecture Rules (Clean Code + Hexagonal / Ports & Adapters)

This document is the **developer-facing summary** of the architecture rules enforced by the current **architecture tests** in this solution (NetArchTest).  
The solution is a **modular monolith** with a **single Host** (composition root) and multiple module APIs (Controllers) packaged as **DLLs**.

---

## 1) Architecture overview

### Per-module layering
Each module (e.g., Customers, Orders) follows these layers:

- **Domain**: business model, entities, value objects, invariants.
- **Application**: use-cases (CQRS/Handlers), orchestration, ports (interfaces) required by the module.
- **Infrastructure**: outbound adapters (EF Core, repositories, messaging, caching, external integrations).
- **Api (RestApi)**: inbound adapter (Controllers, HTTP request models/Params, filters).

### Host (Composition Root)
There is a **single executable Host** responsible for:
- registering DI (Infrastructure wiring),
- loading Controllers from the module APIs (Application Parts),
- configuring middleware (Swagger/Auth/etc.).

> Module APIs are **class libraries** (DLLs) containing Controllers, **not** executables.

---

## 2) Mandatory rules enforced by tests

### 2.1 Module isolation (no module depends on another module)
**Rule:** a module must not reference assemblies from any other module.

- `Modules.*.Domain/Application/Infrastructure/Api` **must not depend** on assemblies belonging to other modules.
- **Intentional exception:** `BuildingBlocks.*` is **excluded** from module isolation checks (allowed as a controlled shared kernel).

✅ This prevents direct references like `Orders -> Customers` via assemblies.

---

### 2.2 API must not reference Domain
**Rule:** any `*.RestApi` assembly **must not** depend on any `*.Domain` assembly.

Practical implications:
- Controllers must **not** accept or return Domain entities/value objects.
- Attributes such as `[ProducesResponseType(typeof(...))]` must **not** use Domain types.
- APIs must expose **DTOs/Contracts** (from Application or `BuildingBlocks.Contracts`).

✅ This keeps the inbound adapter independent from the domain model.

---

### 2.3 Shared contracts (enums, cross-module interfaces)
Given the rules above:

- Domain cannot depend on Application/Api/Infrastructure.
- Api cannot depend on Domain.
- Modules cannot depend on other modules.

Therefore, any type that is required by **both** Domain and DTO/API (or across modules) must live in a **neutral contracts assembly** that is allowed by design.

**Recommended placement (compatible with your tests):**
- `BuildingBlocks.Contracts` / `BuildingBlocks.Abstractions`  
  (or keep in `BuildingBlocks.Application` if that is the existing “excluded by design” assembly)

Typical candidates:
- enums exposed in public API/Swagger but also used in Domain
- cross-module interfaces (e.g., existence checkers) when implemented outside the consumer module
- minimal “integration DTOs” shared across boundaries (keep them small)

---

### 2.4 Composition Root is the Host (not the module APIs)
**Rule:** DI wiring happens in the **Host** only.

- `Host` references `Api + Infrastructure (+ BuildingBlocks)`.
- `Api` must **not** reference `Infrastructure` “only for DI”.
- Infrastructure is registered from Host via methods like `AddXInfrastructure(builder.Configuration)`.

✅ This prevents API-to-Infrastructure coupling and preserves Ports & Adapters.

---

## 3) Developer guidance (do’s and don’ts)

### API (Controllers)
✅ Allowed:
- HTTP request models / Params owned by the API (`Params.cs`, request DTOs)
- DTOs/Contracts from Application or `BuildingBlocks.Contracts`
- MediatR/CQRS calls into Application (if used)

⛔ Forbidden:
- returning/accepting Domain entities/VOs
- using Domain types in attributes (`ProducesResponseType`, generics, etc.)
- “leaking” Domain types through public controller signatures

---

### Application (Use-cases)
✅ Allowed:
- defining ports (interfaces) required by the module
- returning DTOs/results suitable for API consumption

⛔ Strongly discouraged:
- exposing Domain entities as public return types “upwards” to API
- referencing technical infrastructure concerns (EF Core, DbContext, etc.)

---

### Domain
✅ Allowed:
- pure domain logic: entities, VOs, invariants
- referencing neutral `BuildingBlocks.Contracts` types **only if** they are true shared contracts (e.g., contractual enums)

⛔ Forbidden:
- depending on Api or Infrastructure
- depending on other modules

---

### Infrastructure
✅ Allowed:
- EF Core, repositories, messaging, caching, integrations
- implementing ports defined by Application (same module)

⛔ Forbidden (by tests):
- referencing any other module assembly (even “just to implement an interface”)

---

## 4) Hexagonal compliance level if rules are followed

If these rules are consistently followed, the solution is **very close to practical Hexagonal Architecture**:

- ✅ inbound adapters (Api) isolated from Domain
- ✅ outbound adapters (Infrastructure) behind ports (interfaces)
- ✅ strict module isolation
- ✅ explicit composition root (Host)

What prevents “100% purity” (normal in a modular monolith):
- `BuildingBlocks` acts as a **Shared Kernel**. This is acceptable, but it must remain **small and intentional**, otherwise it becomes “global coupling”.
- single-process runtime makes it tempting to bypass boundaries via DB reads; assembly-level tests help, but discipline is still required.

**Practical closeness to Hexagonal:** ~**85–90%**
- closer to 90% if `BuildingBlocks.Contracts` stays minimal and there are no cross-module DB shortcuts,
- closer to 85% if shared kernel grows or boundaries are bypassed operationally.

---

## 5) Allowed vs forbidden references (table)

### 5.1 Forbidden references
| From | Must NOT reference |
|---|---|
| `Any Module.* (Domain/Application/Infrastructure/Api)` | `Other Module.* (any layer)` |
| `*.RestApi` | `*.Domain` |
| `Domain` | `Api` |
| `Domain` | `Infrastructure` |
| `Api` | `Infrastructure` *(DI wiring must be done in Host)* |

### 5.2 Allowed references (recommended)
| From | May reference |
|---|---|
| `Host` | `Api`, `Infrastructure`, `BuildingBlocks.*` |
| `Api` | `Application`, `BuildingBlocks.Contracts` / `BuildingBlocks.Api` |
| `Application` | `Domain`, `BuildingBlocks.Contracts` |
| `Infrastructure` | `Application`, `Domain`, `BuildingBlocks.Infrastructure` |
| `Domain` | `BuildingBlocks.Contracts` *(only truly shared contractual types)* |

---

## 6) Placement rules for tricky shared types
- **Enums used by both Domain and DTO/API**: put them in `BuildingBlocks.Contracts` (or duplicate + explicit mapping if you want maximum DDD purity).
- **Cross-module interfaces** implemented outside the consumer module (e.g., `ICustomerExistenceChecker`): place them in `BuildingBlocks.Contracts` to keep module isolation tests green.
- **Controllers discovery**: Controllers remain in `*.RestApi` assemblies (DLLs) and are loaded by Host using `AddApplicationPart(...)`.

---
