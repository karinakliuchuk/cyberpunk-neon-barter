# Sequence Diagram — UC-02 «Знайти циклічний ланцюжок обміну»

**Автор:** Артем (Architect)
**Сценарій:** UC-02
**Пов'язані вимоги:** FR-06, FR-07, FR-08, NFR-02, NFR-03, NFR-05
**Критерії приймання:** AC-06, AC-07, AC-11
**Фактичний код:**
- `src/CyberpunkNeonBarter.Infrastructure/Services/CycleSearch/CycleFinderService.cs`
- `src/CyberpunkNeonBarter.Infrastructure/Services/CycleSearch/GraphBuilder.cs`
- `src/CyberpunkNeonBarter.Infrastructure/Services/CycleSearch/CycleToDealService.cs`
- `src/CyberpunkNeonBarter.Infrastructure/Data/BarterDbContext.cs`
- `src/CyberpunkNeonBarter.Api/Endpoints/CycleEndpoints.cs`

---

## 1. Джерело Mermaid

```mermaid
sequenceDiagram
    autonumber
    actor U as Учасник (клієнт)
    participant API as CycleEndpoints<br/>POST /api/cycles/find
    participant Finder as CycleFinderService
    participant Builder as GraphBuilder
    participant DB as BarterDbContext<br/>(PostgreSQL)
    participant Deal as CycleToDealService

    U->>API: POST /api/cycles/find
    API->>Finder: FindAsync()

    Note over Finder,DB: Етап 1 — побудова графа
    Finder->>Builder: BuildAsync()
    Builder->>DB: Offers.Where(Status == Active).ToListAsync()
    DB-->>Builder: List<Offer> (Gives + Needs)
    Builder->>Builder: gives = GroupBy(UserId).First()
    Builder->>Builder: needs = GroupBy(UserId).First()
    Builder->>Builder: для кожної пари (give, need):<br/>перевірити Category == Category<br/>і Quantity == Quantity
    Builder-->>Finder: Graph { Adjacency, Edges }

    alt EdgeCount == 0
        Finder-->>API: null
        API-->>U: 409 cycle_not_found<br/>(A2: «Обмін не знайдено»)
    else Граф має ребра
        Note over Finder: Етап 2 — пошук трикутників
        Finder->>Finder: EnumerateTriangles(graph)
        Note right of Finder: для A→B→C→A<br/>фіксація Give_A, Give_B, Give_C<br/>і Need_A, Need_B, Need_C

        alt candidates.Count == 0
            Finder-->>API: null
            API-->>U: 409 cycle_not_found
        else Є кандидати
            Note over Finder: Етап 3 — валідація (NFR-03)
            Finder->>Finder: IsValid(c):<br/>A≠B≠C,<br/>Give_A↔Need_B,<br/>Give_B↔Need_C,<br/>Give_C↔Need_A

            alt valid.Count == 0
                Finder-->>API: null
                API-->>U: 409 cycle_not_found<br/>(A3: «Кандидат невалідний»)
            else Є валідні цикли
                Note over Finder: Етап 4 — детермінований вибір (A4)
                Finder->>Finder: OrderBy(min(A,B,C)).First()

                Note over Finder,DB: Етап 5 — перевірка активності (A5)
                Finder->>DB: Offers.CountAsync(ids, Status == Active)
                DB-->>Finder: count

                alt count < 3
                    Finder-->>API: null
                    API-->>U: 409 cycle_not_found<br/>(A5: «Спробуйте повторити пошук»)
                else Усі 3 Give-оголошення активні
                    Finder-->>API: FoundCycle

                    Note over API,Deal: Етап 6 — створення угоди (FR-08)
                    API->>Deal: CreateDealAsync(cycle)
                    Deal->>DB: Add(BarterDeal + 3 BarterParticipant + AuditLog)
                    Deal->>DB: Offers[3 Give].Status = Matched
                    Deal->>DB: SaveChangesAsync()
                    DB-->>Deal: ok
                    Deal-->>API: BarterDeal
                    API-->>U: 200 { dealId, status, steps[] }
                end
            end
        end
    end