# Звіт про верифікацію NFR (Настя, QA)

> Статуси ⏳ заповнюються після запуску тестів. Нічого нижче не є результатом фактичного прогону,
> крім розділу «Знайдено під час code review» — це спостереження з прочитаного коду Api, ще не підтверджені тестами.

## Запуск

```bash
dotnet test Tests/CyberpunkNeonBarter.Tests.csproj --collect:"XPlat Code Coverage"
```

## Підсумок

| NFR | Вимога | Тест | Файл | Результат |
|---|---|---|---|---|
| NFR-01 | Пошук ≤ 5 с на 1000 оголошень | k6 (потрібен seed 1000 оголошень) | `Performance/nfr01-search.js` | ⏳ чекає на seed |
| NFR-03 | Коректність циклів, ≥ 20 сценаріїв | буде після перегляду CycleFinderService / GraphBuilder / Domain | — | ⏳ не написано |
| NFR-04 | Незмінність даних при відхиленні | буде після перегляду DealEndpoints / Domain | — | ⏳ не написано |
| NFR-05 | Коректні коди помилок | integration | `AuthTests.cs`, `OfferEndpointsErrorTests.cs`, `CycleAndHealthEndpointTests.cs` | ⏳ |
| — | Покриття ≥ 70 % | coverlet | — | ⏳ |

## Знайдено під час code review (потрібно завести як GitHub Issues)

| # | Де | Опис | Серйозність |
|---|---|---|---|
| 1 | `AuthEndpoints.cs` | `/api/auth/login` видає JWT будь-кому з будь-якою роллю (зокрема `Admin`) без перевірки пароля чи існування користувача | Критична |
| 2 | `appsettings*.json` | У репозиторії лежать пароль до PostgreSQL і секрет JWT | Висока |
| 3 | `OfferEndpoints.cs` (POST) | `UserId` береться з тіла запиту, а не з токена: можна створити оголошення від імені іншого користувача | Висока |
| 4 | `OfferEndpoints.cs` (DELETE) | Немає перевірки, що оголошення належить поточному користувачеві | Висока |
| 5 | `CycleEndpoints.cs` | `/api/cycles/find` не має `RequireAuthorization()`: анонім може запускати пошук і створювати угоди | Середня |
| 6 | `Program.cs` / ендпоінти | У наданому коді жодний ендпоінт не обмежений за роллю; RBAC (Admin) не застосовується | Висока (якщо ендпоінтів Admin немає в DealEndpoints) |
| 7 | `OfferEndpoints.cs` (POST) | Немає перевірки `Quantity > 0` на рівні ендпоінта; залежить від `Offer.Create` — при виключенні буде 500 | Середня (потребує перевірки) |
| 8 | `CycleEndpoints.cs` | У відповіді жорстко 3 кроки (A, B, C): цикли довжиною 4+ не підтримуються, хоча за планом потрібні цикли ≥ 3 | Середня (уточнити вимогу) |
| 9 | `CyberpunkNeonBarter.Api.http` | Залишився шаблонний `/weatherforecast` | Низька |

## Review checklist (GitHub PR)

- [ ] Код відповідає Class Diagram
- [ ] Sequence Diagram збігається з реальними викликами
- [ ] `docs/traceability.md` містить посилання на код і тести
- [ ] Усі NFR зі статусом ✅
