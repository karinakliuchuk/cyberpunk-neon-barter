 HEAD
﻿# Cyberpunk Neon Barter`n`n> REST API сервіс для організації багатостороннього бартерного обміну.`n`n---`n`n## Опис проєкту`n`nПроєкт розроблено в межах лабораторної роботи №2 з дисципліни Технології розробки програмного забезпечення (група ТР-41).`n`nОсновна системна логіка реалізує алгоритм циклічного бартерного обміну послугами та ресурсами за схемою A -> B -> C -> A.`n`n---`n`n## Технологічний стек`n`n| Компонент | Технологія / Інструмент |`n| :--- | :--- |`n| Платформа | .NET 8 / C# |`n| Тип застосунку | ASP.NET Core Web API |`n| Тестування | xUnit |`n| CI/CD | GitHub Actions |`n| Контроль версій | Git / GitHub |`n`n---`n`n## Склад команди (Група ТР-41)`n`n| Учасник | Роль у проєкті |`n| :--- | :--- |`n| Ключук Каріна | Координатор, CI Setup |`n| Штепа Артем | Модульне тестування (xUnit) |`n| Поляков Ян | API Конфігурація та OpenAPI |`n| Лясковець Анастасія | Документація та README |`n`n---`n`n## Інструкція з локального запуску`n`n1. Клонувати репозиторій:`n   ```bash`n   git clone [https://github.com/karinakliuchuk/cyberpunk-neon-barter.git](https://github.com/karinakliuchuk/cyberpunk-neon-barter.git)`n   cd cyberpunk-neon-barter`n   ````n`n2. Відновити залежності:`n   ```bash`n   dotnet restore`n   ````n`n3. Зібрати проєкт:`n   ```bash`n   dotnet build --no-restore`n   ````n`n4. Запустити модульні тести:`n   ```bash`n   dotnet test tests/CyberpunkNeonBarter.Tests/CyberpunkNeonBarter.Tests.csproj`n   ````n`n5. Запустити Web API:`n   ```bash`n   dotnet run --project src/CyberpunkNeonBarter.Api`n   ```

# Cyberpunk Neon Barter

> REST API сервіс для організації багатостороннього бартерного обміну в неоновому світі майбутнього.

---

## Опис проєкту

Основна системна логіка реалізує алгоритм циклічного бартерного обміну послугами та ресурсами за схемою **A → B → C → A**, що дозволяє користувачам знаходити оптимальні ланцюжки обміну без безпосереднього використання грошового еквівалента.

---

## Технологічний стек

| Компонент               | Технологія / Інструмент |
| :---------------------- | :---------------------- |
| **Платформа / Мова**    | .NET 8 / C#             |
| **Тип застосунку**      | ASP.NET Core Web API    |
| **Модульне тестування** | xUnit                   |
| **Автоматизація (CI)**  | GitHub Actions          |
| **Контроль версій**     | Git / GitHub            |

---

## Склад команди (Група ТР-41)

| Учасник                 | GitHub / Role             | Зона відповідальності                           |
| :---------------------- | :------------------------ | :---------------------------------------------- |
| **Каріна Ключук**       | `@karinakliuchuk`         | Координатор проєкту, налаштування CI пайплайнів |
| **Артем Штепа**         | `@shtepaartem021`         | Розробка модульних тестів (xUnit)               |
| **Ян Поляков**          | `@poliakov-yan-iate`      | Конфігурація Web API та Swagger/OpenAPI         |
| **Анастасія Лясковець** | `@liaskovetsanastasia-ui` | Документація проєкту та супровід README         |

---

## Інструкція з локального запуску

### 1. Клонувати репозиторій

```bash
git clone https://github.com/karinakliuchuk/cyberpunk-neon-barter.git
cd cyberpunk-neon-barter
```

### 2. Відновити залежності

```bash
dotnet restore
```

### 3. Зібрати проєкт

```bash
dotnet build --no-restore
```

### 4. Запустити модульні тести

```bash
dotnet test tests/CyberpunkNeonBarter.Tests/CyberpunkNeonBarter.Tests.csproj
```

### 5. Запустити Web API

```bash
dotnet run --project src/CyberpunkNeonBarter.Api
```

---

## Неперервна інтеграція (CI)

У репозиторії налаштовано автоматичний CI-пайплайн за допомогою **GitHub Actions** (`.github/workflows/build-and-test.yml`).

Пайплайн запускається при кожному `push` або `pull_request` у гілку `main` та виконує такі кроки:

1. Перевіряє та відновлює залежності .NET 8.
2. Виконує автоматичне збирання проєкту (`dotnet build`).
3. Запускає повний набір модульних тестів (`dotnet test`).
 f0bcaa502bfd9b2fa01af1c9eba44b32fdf05da7
