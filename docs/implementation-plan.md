\# План розробки (Implementation Plan) — Cyberpunk Neon Barter v1.0



\*Відповідальна: Каріна (Integrator)\*



Цей документ описує план реалізації системи \*\*Cyberpunk Neon Barter v1.0\*\*, пов'язує функціональні вимоги з прецедентами, критеріями приймання та UML-моделями, а також визначає порядок розробки, залежності між модулями та критерії готовності.



\---



\## 1. Огляд проєкту



| Поле | Значення |

|---|---|

| \*\*Назва\*\* | Cyberpunk Neon Barter v1.0 |

| \*\*Тип\*\* | REST API платформа для негрошового (бартерного) обміну |

| \*\*Технології\*\* | .NET 8, ASP.NET Core Web API, Entity Framework Core, PostgreSQL |

| \*\*Методологія\*\* | Waterfall |

| \*\*Основна цінність v1.0\*\* | Автоматичний пошук циклічних ланцюжків обміну A → B → C → A |

| \*\*Обмеження v1.0\*\* | Мінімальна довжина циклу — 3 учасники; максимум — без обмежень |



\---



\## 2. Матриця відповідності FR → UC → AC → Модель



| FR | UC | AC | UML-моделі | Модуль реалізації |

|---|---|---|---|---|

| \*\*FR-01\*\* Створення оголошення | UC-01 | AC-01 | Class Diagram, Activity UC-01 | `OfferService`, `OfferController` |

| \*\*FR-02\*\* Пошук циклічного ланцюжка | UC-02 | AC-02 | Class Diagram, Activity UC-02 | `CycleFinderService`, `BarterDealService` |

| \*\*FR-03\*\* Підтвердження участі | UC-03 | AC-03 | Use Case Diagram, Activity UC-03 | `ConfirmationService`, `BarterDealService` |

| \*\*FR-04\*\* Скасування угоди | UC-04 | AC-04 | State Machine, Activity UC-04 | `BarterDealService`, `OfferService` |

| \*\*FR-05\*\* Моніторинг та модерація | UC-05 | AC-05 | Use Case Diagram, Class Diagram | `ModerationService`, `AdminController` |

| \*\*NFR-01\*\* Продуктивність | — | — | — | Індекси в БД, кешування графа |

| \*\*NFR-02\*\* Надійність | — | — | — | Транзакції EF Core |

| \*\*NFR-03\*\* Безпека | — | — | — | JWT Middleware, RBAC |



\---



\## 3. Фази розробки



\### Фаза 1: Ядро предметної області (Domain Layer)



\*\*Мета:\*\* реалізувати сутності, enum-и та базові сервіси.



| Задача | Артефакт | Відповідальний |

|---|---|---|

| Створити проєкт `CyberpunkNeonBarter.Domain` | `.csproj` | Артем |

| Реалізувати сутності `User`, `Offer`, `BarterDeal`, `BarterParticipant`, `AuditLog` | `Domain/Entities/` | Артем |

| Реалізувати enum-и `OfferStatus`, `DealStatus` | `Domain/Enums/` | Артем |

| Налаштувати зв'язки між сутностями | `Domain/Configurations/` | Артем |



\*\*Критерій завершення:\*\* всі 5 сутностей відповідають Class Diagram, проєкт компілюється.



\---



\### Фаза 2: Інфраструктура та БД (Infrastructure Layer)



\*\*Мета:\*\* налаштувати роботу з PostgreSQL через EF Core.



| Задача | Артефакт | Відповідальний |

|---|---|---|

| Підключити `Npgsql.EntityFrameworkCore.PostgreSQL` | `.csproj` | Ян |

| Створити `BarterDbContext` | `Infrastructure/Data/` | Ян |

| Написати міграції | `Migrations/` | Ян |

| Налаштувати індекси для швидкого пошуку | `Configurations/` | Ян |

| Підключити `AuditLog` через інтерцептор | `Interceptors/AuditInterceptor.cs` | Ян |



\*\*Критерій завершення:\*\* БД створюється однією командою `dotnet ef database update`, всі таблиці на місці.



\---



\### Фаза 3: Алгоритм пошуку циклів (Core Algorithm) 

\*\*Мета:\*\* реалізувати ключову цінність системи — пошук циклічних ланцюжків A → B → C → A.



| Задача | Артефакт | Відповідальний |

|---|---|---|

| Побудувати граф оголошень (напрямлений) | `Services/GraphBuilder.cs` | Артем |

| Реалізувати пошук замкнених циклів (DFS / Johnson's algorithm) | `Services/CycleFinderService.cs` | Артем |

| Фільтрація циклів довжиною ≥ 3 | `CycleFinderService.cs` | Артем |

| Створення `BarterDeal` + `BarterParticipant` для знайденого циклу | `BarterDealService.cs` | Артем |

| Оптимізація: час виконання ≤ 2 сек (NFR-01) | Бенчмарки | Артем |



\*\*Критерій завершення:\*\* на тестових даних 1000 оголошень цикл знаходиться за ≤ 2 секунди.



\---



\### Фаза 4: API-шар (Application Layer)



\*\*Мета:\*\* реалізувати REST-контролери та DTO.



| Задача | Артефакт | Відповідальний |

|---|---|---|

| `OfferController` (POST/GET/DELETE) | `Api/Controllers/OfferController.cs` | Ян |

| `BarterDealController` (GET/confirm/cancel) | `Api/Controllers/BarterDealController.cs` | Каріна |

| `AdminController` (moderate/cancel) | `Api/Controllers/AdminController.cs` | Настя |

| DTO та мапінг | `Api/DTOs/`, `AutoMapper` | Каріна |

| JWT-авторизація + RBAC | `Api/Auth/` | Ян |

| Swagger/OpenAPI | `Program.cs` | Ян |



\*\*Критерій завершення:\*\* всі endpoints доступні через Swagger, авторизація працює.



\---



\### Фаза 5: Сценарії прецедентів (Use Case Implementations)



| UC | Реалізація | Відповідальний |

|---|---|---|

| \*\*UC-01\*\* Створити оголошення | `OfferService.CreateOffer()` | Ян |

| \*\*UC-02\*\* Пошук ланцюжка | `CycleFinderService.FindCycles()` | Артем |

| \*\*UC-03\*\* Підтвердити участь | `ConfirmationService.Confirm()` | Каріна |

| \*\*UC-04\*\* Скасувати угоду | `BarterDealService.Cancel()` | Настя |

| \*\*UC-05\*\* Модерація | `ModerationService.Deactivate()` | Настя |

| \*\*UC-06\*\* Виставити зауваження | `ModerationService.AddComment()` | Настя |

| \*\*UC-07\*\* Примусове скасування | `BarterDealService.ForceCancel()` | Настя |



\*\*Критерій завершення:\*\* кожен UC має інтеграційний тест, що покриває основний та альтернативні потоки.



\---



\### Фаза 6: Тестування та QA



\*\*Мета:\*\* покрити систему тестами та перевірити NFR.



| Задача | Артефакт | Відповідальний |

|---|---|---|

| Unit-тести для сервісів | `Tests/Unit/` | Настя |

| Інтеграційні тести API | `Tests/Integration/` | Настя |

| Тест на продуктивність (NFR-01) | k6 / JMeter скрипт | Настя |

| Тест на транзакційність (NFR-02) | Integration test | Настя |

| Тест на авторизацію (NFR-03) | Integration test | Настя |

| Фінальний review документації | Checklist | Настя |



\*\*Критерій завершення:\*\* покриття ≥ 70%, всі NFR підтверджені.



\---



\## 4. Залежності між фазами



```

Фаза 1 (Domain)

&#x20;   ↓

Фаза 2 (Infrastructure) ──┐

&#x20;   ↓                     │

Фаза 3 (Algorithm) ───────┤

&#x20;   ↓                     │

Фаза 4 (API) ←────────────┘

&#x20;   ↓

Фаза 5 (Use Cases)

&#x20;   ↓

Фаза 6 (Testing)

```



\*\*Критичний шлях:\*\* Фаза 1 → Фаза 3 → Фаза 5 (алгоритм і підтвердження — серце системи).



\---



\## 5. Розподіл відповідальності



| Учасник | Роль | Основні модулі |

|---|---|---|

| \*\*Ян\*\* | Analyst | `OfferService`, `OfferController`, `requirements.md` |

| \*\*Артем\*\* | Architect | `Domain`, `CycleFinderService`, `BarterDealService` |

| \*\*Каріна\*\* | Integrator | `BarterDealController`, `design.md`, `traceability.md`, координація |

| \*\*Настя\*\* | QA / Reviewer | `ModerationService`, тести, рев'ю всіх артефактів |



\---



\## 6. Definition of Done (DoD)



Функціональна вимога вважається виконаною, якщо:



\- Реалізовано відповідний endpoint у REST API.

\- Написано unit-тест на основний потік.

\- Написано integration-тест на альтернативний потік.

\- Оновлено Swagger-документацію.

\- Пройдено code review іншим учасником команди.

\- Зміни зафіксовані в `AuditLog` .

\- Відповідний UML-артефакт оновлено.



\---



\## 7. Ризики та пом'якшення



| Ризик | Ймовірність | Вплив | Пом'якшення |

|---|---|---|---|

| Алгоритм пошуку циклів повільний на великих графах | Середня | Високий | Використати алгоритм Джонсона, кешувати граф |

| Конфлікти при одночасному підтвердженні кількома учасниками | Середня | Високий | Транзакції + optimistic locking |

| Таймаут не спрацьовує | Низька | Середній | Background job (Hangfire / Quartz.NET) |

| Не всі учасники встигають підтвердити | Висока | Середній | Гнучкий `ExpiresAt`, нагадування |



\---



\## 8. Зв'язки з іншими артефактами



\- \*\*Requirements:\*\* \[requirements.md](requirements.md)

\- \*\*Use Case Diagram:\*\* \[design.md#1](design.md)

\- \*\*Class Diagram:\*\* \[design.md#2](design.md)

\- \*\*State Machine Diagram:\*\* \[design.md#3](design.md)

\- \*\*Activity Diagrams:\*\* \[model/](../model/)

\- \*\*RTM:\*\* \[traceability.md](traceability.md)

\- \*\*UC-03 специфікація:\*\* \[uc-03-spec.md](uc-03-spec.md)

