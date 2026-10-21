# Матриця простежуваності вимог (Requirements Traceability Matrix — RTM)

## 1. Покриття функціональних вимог (Functional Requirements)

| ID вимоги | Опис вимоги | Use Case | Критерій приймання (AC) | Проєктний артефакт (UML) | Фактична реалізація (Код) | Результат перевірки | Власник |
|---|---|---|---|---|---|---|---|
| **FR-01** | Створення оголошення про обмін ресурсу/послуги | UC-01 | AC-01 (Оголошення збережено зі статусом Active) | Class Diagram, Activity UC-01 | `OfferEndpoints.cs` (POST), `Offer.cs` | Успішно (201 Created) | Каріна / Ян |
| **FR-02** | Автоматичний пошук циклічних ланцюжків обміну (A->B->C->A) | UC-02 | AC-02 (Сформовано BarterDeal) | Class Diagram, Activity UC-02, Sequence UC-02 | `CycleFinderService.cs`, `CycleEndpoints.cs` | Успішно (200 OK) | Артем (Architect) |
| **FR-03** | Підтвердження участі в угоді учасниками ланцюжка | UC-03 | AC-03 (При 100% згоді статус Active) | Use Case Diagram, Activity UC-03 | `DealEndpoints.cs` (POST /confirm) | Очікує тестів | Каріна (Integrator) |
| **FR-04** | Скасування угоди та розформування ланцюжка | UC-04 | AC-04 (Угода Cancelled, оголошення в Active) | State Machine Diagram, Activity UC-04 | `DealEndpoints.cs` (POST /cancel) | Очікує тестів | Настя (QA) |
| **FR-05** | Моніторинг та деактивація заблокованих пропозицій | UC-05 | AC-05 (Деактивовано порушені оголошення) | Use Case Diagram, Class Diagram | `AdminEndpoints.cs` (очікується) | В розробці | Настя / Каріна |

## 2. Покриття нефункціональних вимог (Non-Functional Requirements)

| ID вимоги | Тип вимоги | Опис вимоги | Метрика / Спосіб перевірки | Фактична реалізація (Код) | Результат перевірки |
|---|---|---|---|---|---|
| **NFR-01** | Продуктивність | Час пошуку циклів ≤ 2 сек | Бенчмарки (`CycleSearchBenchmark.cs`) | `CycleFinderService.cs` | Очікує тестів |
| **NFR-02** | Надійність | Атомарність зміни стану угоди | Транзакційність у БД | `BarterDbContext.cs` (EF Core) | Успішно |
| **NFR-03** | Безпека | Авторизація та аутентифікація | JWT Tokens, RBAC | `Program.cs`, `AuthEndpoints.cs` | Успішно (401/200) |

## 3. Зв'язок вимог з прецедентами

| Use Case | Пов'язані FR | Пов'язані NFR |
|---|---|---|
| UC-01 | FR-01 | NFR-03 |
| UC-02 | FR-02 | NFR-01, NFR-02 |
| UC-03 | FR-03 | NFR-02, NFR-03 |
| UC-04 | FR-04 | NFR-02, NFR-03 |
| UC-05 | FR-05 | NFR-03 |