# Архітектурний опис — AutoDealerCRM

**Версія:** 1.0  
**Дата:** 2026-11-02  
**Статус:** Актуальна  
**Стандарти:** ISO/IEC/IEEE 42010:2022

---

## 1. Загальна архітектура

Система реалізована за шаблоном **MVC (Model-View-Controller)** на платформі ASP.NET Core 8. Застосовується **трирівнева логічна архітектура**:

```
┌─────────────────────────────────────┐
│         Presentation Layer          │  ← Controllers, Views (Razor), ViewModels
├─────────────────────────────────────┤
│          Business Logic Layer        │  ← Services, Domain Models, Validators
├─────────────────────────────────────┤
│          Data Access Layer           │  ← EF Core DbContext, Repositories
├─────────────────────────────────────┤
│         Microsoft SQL Server         │  ← LocalDB / SQL Express
└─────────────────────────────────────┘
```

---

## 2. Структура проєкту

```
AutoDealerCRM/
├── Controllers/
│   ├── HomeController.cs           — публічний каталог, заявки
│   ├── AccountController.cs        — вхід / вихід
│   ├── ManagerController.cs        — управління лотами, перегляд заявок
│   └── AdminController.cs          — управління акаунтами, ліцензія, аудит
│
├── Models/
│   ├── CarLot.cs                   — сутність лоту автомобіля
│   ├── BuyerRequest.cs             — сутність заявки покупця
│   ├── AppUser.cs                  — сутність користувача
│   └── AuditLogEntry.cs            — сутність запису журналу
│
├── ViewModels/
│   ├── CarLotViewModel.cs
│   ├── BuyerRequestViewModel.cs
│   ├── LoginViewModel.cs
│   └── LicenseViewModel.cs
│
├── Services/
│   ├── IEncryptionService.cs       — інтерфейс шифрування
│   ├── EncryptionService.cs        — AES-256 шифрування/розшифрування
│   ├── ILicenseService.cs          — інтерфейс перевірки ліцензії
│   ├── LicenseService.cs           — HMAC/RSA перевірка dealer.lic
│   ├── IAuditLogService.cs
│   └── AuditLogService.cs          — запис подій у журнал безпеки
│
├── Data/
│   ├── AppDbContext.cs             — EF Core DbContext
│   └── Migrations/                 — EF Core міграції
│
├── Views/
│   ├── Home/                       — публічний каталог
│   ├── Account/                    — Login
│   ├── Manager/                    — управління лотами
│   └── Admin/                      — адмін-панель
│
├── wwwroot/                        — статичні файли (CSS, JS)
├── appsettings.json
├── appsettings.Development.json    — рядок підключення, ключ шифрування (не в git)
└── Program.cs                      — точка входу, DI-контейнер, Middleware
```

---

## 3. Шар безпеки (Security Layer)

Безпека реалізована як наскрізний компонент, що пронизує всі шари:

```
HTTP Request
    │
    ▼
[Middleware: SecurityHeaders]       ← X-Frame-Options, X-Content-Type-Options
    │
    ▼
[Middleware: Authentication]        ← Cookie-сесія, перевірка Identity
    │
    ▼
[Controller: [Authorize(Roles=)]]   ← RBAC перевірка ролей
    │
    ▼
[Service: EncryptionService]        ← AES-256 шифр/розшифр полів
    │
    ▼
[Data: AppDbContext]                ← Зберігання зашифрованих даних
```

### 3.1. EncryptionService

- Алгоритм: AES-256-CBC з випадковим IV для кожної операції шифрування.
- IV зберігається разом із шифртекстом (prefix перед Base64).
- Ключ зчитується з `IConfiguration` → `Encryption:Key`.
- Інтерфейс: `string Encrypt(string plaintext)` / `string Decrypt(string ciphertext)`.

### 3.2. LicenseService

- Формат файлу `dealer.lic`: JSON із полями `Owner`, `ExpiresAt`, `Signature`.
- Підпис: HMAC-SHA256 від `Owner + ExpiresAt` з секретним ключем.
- Ключ підпису зчитується з конфігурації середовища.

### 3.3. AuditLogService

- Метод: `Task LogAsync(EventType, string actor, string target, string ip)`.
- Запис завжди виконується незалежно від результату операції.
- Читання доступне лише адміністратору.

---

## 4. Модель бази даних

```
┌──────────────┐      ┌──────────────────┐
│   CarLots    │      │  BuyerRequests   │
│──────────────│      │──────────────────│
│ Id (PK)      │      │ Id (PK)          │
│ Make         │      │ FullName         │
│ Model        │      │ Phone (encrypted)│
│ Year         │      │ DesiredModel     │
│ Mileage      │      │ CreatedAt        │
│ PublicPrice  │      │ IsProcessed      │
│ Status       │      └──────────────────┘
│ VinCode (enc)│
│ AuctionCost  │      ┌──────────────────┐
│  (encrypted) │      │    AppUsers      │
│ AuctionName  │      │──────────────────│
│ AddedAt      │      │ Id (PK)          │
└──────────────┘      │ UserName         │
                      │ PasswordHash     │
┌──────────────┐      │ Role             │
│ AuditLogs    │      │ IsBlocked        │
│──────────────│      │ CreatedAt        │
│ Id (PK)      │      └──────────────────┘
│ Timestamp    │
│ EventType    │
│ ActorName    │
│ TargetName   │
│ IpAddress    │
│ Details      │
└──────────────┘
```

---

## 5. Залежності та технологічний стек

| Компонент | Версія | Призначення |
|-----------|--------|-------------|
| .NET | 8.0 | Платформа |
| ASP.NET Core MVC | 8.0 | Веб-фреймворк |
| Entity Framework Core | 8.x | ORM, Code-First |
| Microsoft.EntityFrameworkCore.SqlServer | 8.x | SQL Server провайдер |
| ASP.NET Core Identity | 8.x | Хешування паролів, User Management |
| System.Security.Cryptography | BCL (.NET 8) | AES-256, HMAC-SHA256 |
| Microsoft.Extensions.Configuration | BCL (.NET 8) | Читання ключів з appsettings |

Зовнішніх NuGet-пакетів для криптографії не використовується — лише вбудовані бібліотеки .NET.
