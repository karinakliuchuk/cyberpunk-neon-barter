\# Архітектурне проектування та UML-моделі (System Design)



Цей документ містить повний набір UML-моделей для проекту \*\*Cyberpunk Neon Barter v1.0\*\*, розроблений командою відповідно до методології Waterfall.



\---



\## 1. Прецеденти використання (Use Case Diagram)



\*Відповідальний: Каріна (Integrator)\*



Загальна діаграма варіантів використання відображає основні сценарії взаємодії Учасника бартеру та Адміністратора з системою.



!\[Use Case Diagram](images/use-case.png)



\*\*Основні прецеденти:\*\*

\- \*\*UC-01:\*\* Створити оголошення про обмін

\- \*\*UC-02:\*\* Пошук циклічного ланцюжка обміну

\- \*\*UC-03:\*\* Підтвердити участь у бартерній угоді

\- \*\*UC-04:\*\* Скасувати бартерну угоду

\- \*\*UC-05:\*\* Моніторинг та модерація пропозицій

\- \*\*UC-06:\*\* Виставити зауваження (розширює UC-04)

\- \*\*UC-07:\*\* Примусово скасувати угоду (для Адміністратора)



\---



\## 2. Модель предметної області (Class Diagram)



\*Відповідальний: Артем (Architect)\*



Діаграма класів описує ключові сутності системи (`User`, `Offer`, `BarterDeal`, `BarterParticipant`, `AuditLog`), їхні атрибути, методи та зв'язки.



```mermaid

classDiagram

&#x20;   class User {

&#x20;       +Guid Id

&#x20;       +string Username

&#x20;       +string Email

&#x20;       +double Rating

&#x20;       +bool IsActive

&#x20;       +CreateOffer() Offer

&#x20;       +ConfirmBarter(dealId) void

&#x20;       +CancelBarter(dealId) void

&#x20;   }



&#x20;   class Offer {

&#x20;       +Guid Id

&#x20;       +Guid UserId

&#x20;       +string OfferedItemTitle

&#x20;       +string DesiredItemTitle

&#x20;       +string Category

&#x20;       +OfferStatus Status

&#x20;       +DateTime CreatedAt

&#x20;       +Publish() void

&#x20;       +Deactivate() void

&#x20;       +MarkInBarter() void

&#x20;   }



&#x20;   class BarterDeal {

&#x20;       +Guid Id

&#x20;       +int CycleLength

&#x20;       +DateTime CreatedAt

&#x20;       +DateTime ExpiresAt

&#x20;       +DealStatus Status

&#x20;       +AddParticipant(userId, offerId) void

&#x20;       +ConfirmByParticipant(participantId) void

&#x20;       +ActivateDeal() void

&#x20;       +CancelDeal(reason) void

&#x20;   }



&#x20;   class BarterParticipant {

&#x20;       +Guid Id

&#x20;       +Guid DealId

&#x20;       +Guid UserId

&#x20;       +Guid OfferId

&#x20;       +bool IsConfirmed

&#x20;       +DateTime? ConfirmedAt

&#x20;       +SetConfirmation(value) void

&#x20;   }



&#x20;   class AuditLog {

&#x20;       +Guid Id

&#x20;       +string EventType

&#x20;       +DateTime Timestamp

&#x20;       +string Description

&#x20;       +Log(eventType, description) void

&#x20;   }



&#x20;   class OfferStatus {

&#x20;       <<enumeration>>

&#x20;       Active

&#x20;       InBarter

&#x20;       Deactivated

&#x20;       Closed

&#x20;   }



&#x20;   class DealStatus {

&#x20;       <<enumeration>>

&#x20;       Created

&#x20;       PendingConfirmations

&#x20;       Active

&#x20;       Completed

&#x20;       Cancelled

&#x20;   }



&#x20;   User "1" -- "\*" Offer : owns

&#x20;   User "1" -- "\*" BarterParticipant : participates

&#x20;   BarterDeal "1" -- "3..\*" BarterParticipant : contains

&#x20;   Offer "1" -- "0..1" BarterParticipant : assigned\_to

&#x20;   Offer --> OfferStatus : has

&#x20;   BarterDeal --> DealStatus : has

&#x20;   User ..> AuditLog : triggers

&#x20;   BarterDeal ..> AuditLog : triggers

```



\*\*Пояснення зв'язків:\*\*

\- `User (1) -- (\*) Offer` — один користувач має багато оголошень.

\- `User (1) -- (\*) BarterParticipant` — користувач бере участь у багатьох угодах.

\- `BarterDeal (1) -- (3..\*) BarterParticipant` — одна угода об'єднує від 3 учасників.

\- `Offer (1) -- (0..1) BarterParticipant` — одне оголошення задіяне максимум в одній угоді.



\---



\## 3. Модель станів (State Machine Diagram)



\*Відповідальна: Настя (QA / Reviewer)\*



Діаграма станів описує життєвий цикл основної сутності — \*\*`BarterDeal` (Бартерна угода)\*\*.



```mermaid

stateDiagram-v2

&#x20;   \[\*] --> Created : Пошук знайшов цикл A->B->C->A

&#x20;   Created --> PendingConfirmations : Надсилання сповіщень учасникам



&#x20;   PendingConfirmations --> Active : Усі учасники підтвердили (AllConfirmed)

&#x20;   PendingConfirmations --> Cancelled : Хтось відхилив або таймаут (Rejected / Expired)



&#x20;   Active --> Completed : Усі сторони підтвердили обмін (Complete)

&#x20;   Active --> Cancelled : Примусове скасування адміністратором



&#x20;   Completed --> \[\*]

&#x20;   Cancelled --> \[\*]



&#x20;   note right of Cancelled

&#x20;       Оголошення повертаються

&#x20;       в статус Active (виставлені).

&#x20;       Всі учасники отримують

&#x20;       сповіщення про скасування.

&#x20;   end note



&#x20;   note right of Completed

&#x20;       Угоду успішно закрито,

&#x20;       оголошення переведені

&#x20;       в статус Closed.

&#x20;   end note

```



\*\*Опис переходів:\*\*



| Перехід | Умова | Дія |

|---|---|---|

| `\[\*] → Created` | Алгоритм знайшов цикл довжиною ≥ 3 | Створення `BarterDeal`, логування |

| `Created → PendingConfirmations` | Автоматично після створення | Надсилання сповіщень |

| `PendingConfirmations → Active` | Усі учасники підтвердили | `ActivateDeal()` |

| `PendingConfirmations → Cancelled` | Відхилення або таймаут | `CancelDeal()`, повернення оголошень |

| `Active → Completed` | Усі сторони підтвердили обмін | Закриття угоди |

| `Active → Cancelled` | Примусове скасування адміном | `CancelDeal()`, логування |



\---



\## 4. Глосарій



| Термін | Опис |

|---|---|

| \*\*Циклічний ланцюжок\*\* | Замкнений ланцюг обміну A → B → C → A, де кожен учасник віддає один ресурс і отримує інший |

| \*\*BarterDeal\*\* | Сутність, що об'єднує учасників та їхні оголошення в один цикл обміну |

| \*\*BarterParticipant\*\* | Проміжна сутність, що зв'язує користувача, оголошення та угоду |

| \*\*OfferStatus\*\* | Статус оголошення: Active, InBarter, Deactivated, Closed |

| \*\*DealStatus\*\* | Статус угоди: Created, PendingConfirmations, Active, Completed, Cancelled |



\---



\## 5. Подієві сценарії дій (Activity Diagrams)



\*Відповідальна: Каріна (Integrator)\*



Детальний алгоритм виконання сценарію підтвердження участі в бартері знаходиться у файлі: \[Activity Diagram UC-03](../model/activity-uc-03.md).

