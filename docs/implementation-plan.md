# План розробки (Implementation Plan) — Cyberpunk Neon Barter v1.0

*Відповідальна: Каріна (Integrator)*

Цей документ описує план реалізації системи **Cyberpunk Neon Barter v1.0**, пов'язує вимоги з артефактами та визначає фактичний розподіл обов'язків команди.

## 1. Огляд проєкту

| Поле | Значення |
|---|---|
| **Назва** | Cyberpunk Neon Barter v1.0 |
| **Тип** | REST API платформа для негрошового (бартерного) обміну |
| **Технології** | **.NET 9**, ASP.NET Core Minimal API, Entity Framework Core, PostgreSQL, JWT |
| **Методологія** | Waterfall |
| **Основна цінність v1.0** | Автоматичний пошук циклічних ланцюжків обміну A → B → C → A |
| **Обмеження v1.0** | Мінімальна довжина циклу — 3 учасники |

## 2. Матриця відповідності FR → UC → AC → Модуль реалізації

| FR | UC | AC | Модуль реалізації (Файли) |
|---|---|---|---|
| **FR-01** Створення оголошення | UC-01 | AC-01 | `OfferEndpoints.cs`, `OfferDtos.cs` |
| **FR-02** Пошук ланцюжка | UC-02 | AC-02 | `CycleFinderService.cs`, `CycleEndpoints.cs` |
| **FR-03** Підтвердження участі | UC-03 | AC-03 | `DealEndpoints.cs` (`/confirm`) |
| **FR-04** Скасування угоди | UC-04 | AC-04 | `DealEndpoints.cs` (`/cancel`) |
| **FR-05** Моніторинг та модерація | UC-05 | AC-05 | `AdminEndpoints.cs` (в розробці) |
| **NFR-03** Безпека | — | — | `Program.cs` (JWT), `AuthEndpoints.cs` |

## 3. Фази розробки

### Фаза 1: Ядро предметної області (Domain Layer) — *Завершено*
- Створено проєкт `CyberpunkNeonBarter.Domain`.
- Реалізовано сутності `User`, `Offer`, `BarterDeal`, `BarterParticipant`, `AuditLog` та Enum-и.
- **Відповідальний:** Ян.

### Фаза 2: Інфраструктура та БД (Infrastructure Layer) — *Завершено*
- Налаштовано `BarterDbContext`, підключено PostgreSQL.
- Створено міграцію `InitialV1`.
- **Відповідальний:** Артем, Ян.

### Фаза 3: Алгоритм пошуку циклів (Core Algorithm) — *Завершено*
- Реалізовано `GraphBuilder`, `CycleFinderService` (DFS), `CycleToDealService`.
- **Відповідальний:** Артем.

### Фаза 4: API-шар (Application Layer) — *Завершено*
- Обрано архітектуру **Minimal API** замість класичних контролерів.
- Налаштовано Swagger UI (`Swashbuckle`).
- Налаштовано JWT-авторизацію та захист ендпоінтів.
- Реалізовано `OfferEndpoints` та `DealEndpoints`.
- **Відповідальний:** Каріна (спільно з Артемом).

### Фаза 5: Тестування та QA — *В процесі*
- Написано базові unit-тести для `CycleFinderService`.
- **Відповідальний:** Настя.

## 4. Розподіл відповідальності (Фактичний)

| Учасник | Роль | Виконана робота |
|---|---|---|
| **Ян** | Analyst | Domain Layer (судності, конфігурації), частково Infrastructure (DbContext). |
| **Артем** | Architect | Infrastructure, Алгоритм пошуку циклів (`CycleFinderService`), Sequence Diagram, ініціалізація API. |
| **Каріна** | Integrator | API Layer (JWT, Swagger, DTOs), `OfferEndpoints`, `DealEndpoints`, оновлення RTM та координація. |
| **Настя** | QA / Reviewer | State Machine Diagram, написання тестів, Review Pull Requests. |

## 5. Definition of Done (DoD)

Функціональна вимога вважається виконаною, якщо:
- [x] Реалізовано відповідний endpoint (Minimal API).
- [x] Пройдено перевірку авторизації (JWT).
- [ ] Написано unit-тест (в процесі Настею).
- [x] Оновлено Swagger-документацію (додано опис Bearer токена).
- [x] Пройдено code review іншим учасником команди.

## 6. Зв'язки з іншими артефактами

- **Requirements:** [requirements.md](requirements.md)
- **UML Моделі:** [design.md](design.md)
- **Activity Diagrams:** [model/](../model/)
- **RTM:** [traceability.md](traceability.md)