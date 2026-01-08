# Reglas de arquitectura (Clean Code + Hexagonal / Puertos y Adaptadores)

Este documento es el **resumen para desarrolladores** de las reglas de arquitectura **aplicadas por los tests** actuales (NetArchTest) en esta solución.  
La solución es un **monolito modular** con un **único Host** (composition root) y varias APIs por módulo (Controllers) empaquetadas como **DLLs**.

---

## 1) Visión general de la arquitectura

### Capas por módulo
Cada módulo (por ejemplo, Customers, Orders) sigue estas capas:

- **Domain**: modelo de negocio, entidades, value objects, invariantes.
- **Application**: casos de uso (CQRS/Handlers), orquestación, puertos (interfaces) que necesita el módulo.
- **Infrastructure**: adaptadores de salida (EF Core, repositorios, mensajería, caché, integraciones externas).
- **Api (RestApi)**: adaptador de entrada (Controllers, modelos/Params HTTP, filtros).

### Host (Composition Root)
Existe un **único Host ejecutable** responsable de:
- registrar DI (wiring de Infrastructure),
- cargar los Controllers de las APIs de los módulos (Application Parts),
- configurar middleware (Swagger/Auth/etc.).

> Las APIs por módulo son **librerías** (DLLs) con Controllers, **no** ejecutables.

---

## 2) Reglas obligatorias aplicadas por los tests

### 2.1 Aislamiento entre módulos (ningún módulo depende de otro)
**Regla:** un módulo no debe referenciar assemblies de otro módulo.

- `Modules.*.Domain/Application/Infrastructure/Api` **no deben depender** de assemblies pertenecientes a otros módulos.
- **Excepción intencional:** `BuildingBlocks.*` queda **excluido** de las comprobaciones de aislamiento (se permite como shared kernel controlado).

✅ Esto evita referencias directas como `Orders -> Customers` a nivel de assemblies.

---

### 2.2 La API no puede referenciar Domain
**Regla:** cualquier assembly `*.RestApi` **no** debe depender de ningún assembly `*.Domain`.

Implicaciones prácticas:
- Los Controllers **no** deben aceptar ni devolver entidades/VOs del Domain.
- Atributos como `[ProducesResponseType(typeof(...))]` **no** pueden usar tipos del Domain.
- Las APIs deben exponer **DTOs/Contracts** (de Application o de `BuildingBlocks.Contracts`).

✅ Esto mantiene el adaptador de entrada independiente del modelo de dominio.

---

### 2.3 Contratos compartidos (enums, interfaces cross-module)
Dadas las reglas anteriores:

- Domain no puede depender de Application/Api/Infrastructure.
- Api no puede depender de Domain.
- Los módulos no pueden depender de otros módulos.

Por lo tanto, cualquier tipo que sea requerido por **Domain** y también por **DTO/API** (o entre módulos) debe vivir en un **assembly neutral de contratos** permitido por diseño.

**Ubicación recomendada (compatible con tus tests):**
- `BuildingBlocks.Contracts` / `BuildingBlocks.Abstractions`  
  (o mantenerlo en `BuildingBlocks.Application` si ese es el assembly “excluido por diseño” actual)

Candidatos típicos:
- enums expuestos en la API/Swagger pero también usados en Domain
- interfaces cross-module (por ejemplo, existence checkers) cuando se implementan fuera del módulo consumidor
- “integration DTOs” mínimos compartidos entre límites (mantenerlos pequeños)

---

### 2.4 El Composition Root es el Host (no las APIs de los módulos)
**Regla:** el wiring de DI se hace **solo** en el **Host**.

- `Host` referencia `Api + Infrastructure (+ BuildingBlocks)`.
- `Api` **no** debe referenciar `Infrastructure` “solo por DI”.
- Infrastructure se registra desde el Host con métodos tipo `AddXInfrastructure(builder.Configuration)`.

✅ Esto evita el acoplamiento API→Infrastructure y preserva Puertos y Adaptadores.

---

## 3) Guía práctica para desarrolladores (qué hacer y qué no)

### API (Controllers)
✅ Permitido:
- modelos HTTP/Params propios de la API (`Params.cs`, request DTOs)
- DTOs/Contracts de Application o `BuildingBlocks.Contracts`
- llamadas MediatR/CQRS hacia Application (si se usa)

⛔ Prohibido:
- devolver/recibir entidades/VOs del Domain
- usar tipos del Domain en atributos (`ProducesResponseType`, genéricos, etc.)
- “filtrar” tipos del Domain en firmas públicas del Controller

---

### Application (Casos de uso)
✅ Permitido:
- definir puertos (interfaces) que necesita el módulo
- devolver DTOs/resultados aptos para consumo desde API

⛔ Muy desaconsejado:
- exponer entidades Domain como tipos de retorno públicos “hacia arriba” (API)
- referenciar preocupaciones técnicas de Infrastructure (EF Core, DbContext, etc.)

---

### Domain
✅ Permitido:
- lógica de dominio pura: entidades, VOs, invariantes
- referenciar tipos neutrales de `BuildingBlocks.Contracts` **solo si** son contratos realmente compartidos (por ejemplo, enums contractuales)

⛔ Prohibido:
- depender de Api o Infrastructure
- depender de otros módulos

---

### Infrastructure
✅ Permitido:
- EF Core, repositorios, mensajería, caché, integraciones
- implementar puertos definidos por Application (del mismo módulo)

⛔ Prohibido (por tests):
- depender de cualquier assembly de otro módulo (aunque sea “solo para implementar una interfaz”)

---

## 4) Nivel de cumplimiento hexagonal si se siguen estas reglas

Si estas reglas se siguen de forma consistente, la solución está **muy cerca de una Hexagonal práctica**:

- ✅ adaptadores de entrada (Api) aislados de Domain
- ✅ adaptadores de salida (Infrastructure) detrás de puertos (interfaces)
- ✅ aislamiento estricto entre módulos
- ✅ composition root explícito (Host)

Lo que impide el “100% de pureza” (normal en monolito modular):
- `BuildingBlocks` actúa como **Shared Kernel**. Es aceptable, pero debe mantenerse **pequeño e intencional**; si crece demasiado, se convierte en acoplamiento global.
- al ser un único proceso, existe la tentación de saltarse límites vía lecturas cruzadas de BD; los tests de assemblies ayudan, pero se requiere disciplina.

**Cercanía práctica a Hexagonal:** ~**85–90%**
- más cerca de 90% si `BuildingBlocks.Contracts` se mantiene mínimo y no hay atajos cross-module por BD,
- más cerca de 85% si el shared kernel crece o se bypassan límites operacionalmente.

---

## 5) Tabla final: referencias permitidas y prohibidas

### 5.1 Referencias prohibidas
| Desde | NO debe referenciar |
|---|---|
| `Cualquier Module.* (Domain/Application/Infrastructure/Api)` | `Otro Module.* (cualquier capa)` |
| `*.RestApi` | `*.Domain` |
| `Domain` | `Api` |
| `Domain` | `Infrastructure` |
| `Api` | `Infrastructure` *(el wiring de DI debe hacerse en Host)* |

### 5.2 Referencias permitidas (recomendadas)
| Desde | Puede referenciar |
|---|---|
| `Host` | `Api`, `Infrastructure`, `BuildingBlocks.*` |
| `Api` | `Application`, `BuildingBlocks.Contracts` / `BuildingBlocks.Api` |
| `Application` | `Domain`, `BuildingBlocks.Contracts` |
| `Infrastructure` | `Application`, `Domain`, `BuildingBlocks.Infrastructure` |
| `Domain` | `BuildingBlocks.Contracts` *(solo tipos contractuales realmente compartidos)* |

---

## 6) Reglas de ubicación para tipos compartidos “conflictivos”
- **Enums usados por Domain y también por DTO/API**: colocarlos en `BuildingBlocks.Contracts` (o duplicarlos + mapping explícito si quieres máxima pureza DDD).
- **Interfaces cross-module** implementadas fuera del módulo consumidor (por ejemplo, `ICustomerExistenceChecker`) : colocarlas en `BuildingBlocks.Contracts` para mantener el aislamiento entre módulos.
- **Descubrimiento de Controllers**: los Controllers siguen en `*.RestApi` (DLLs) y el Host los carga con `AddApplicationPart(...)`.

---







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
