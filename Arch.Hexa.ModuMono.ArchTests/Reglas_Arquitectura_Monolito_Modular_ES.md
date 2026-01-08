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
