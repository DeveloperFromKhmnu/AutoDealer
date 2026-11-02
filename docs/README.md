# Документація проєкту AutoDealerCRM

## Загальний опис

**AutoDealerCRM** — навчальний комерційний веб-сервіс (CRM) для автодилера, що спеціалізується на доставці вживаних автомобілів зі США з аукціонів Copart та IAAI. Система реалізована як ASP.NET Core MVC-додаток з базою даних Microsoft SQL Server та демонструє практичне застосування механізмів кібербезпеки.

## Структура документації

```
docs/
├── README.md                        — цей файл, навігація по документації
├── requirements/
│   ├── specification.md             — специфікація вимог (функціональні та нефункціональні)
│   └── scope.md                     — межі та область відповідальності проєкту
├── architecture/
│   └── architecture.md              — архітектурний опис системи
├── business-logic/
│   └── business-logic.md            — опис бізнес-логіки та правил предметної області
└── security/
    ├── security-policy.md           — політика безпеки проєкту
    ├── privacy-policy.md            — політика конфіденційності
    └── threat-model.md              — модель загроз (STRIDE)
```

## Навігація

| Документ | Призначення |
|---|---|
| [Специфікація вимог](requirements/specification.md) | Функціональні та нефункціональні вимоги до системи |
| [Межі проєкту](requirements/scope.md) | Що входить і що не входить до системи |
| [Архітектура](architecture/architecture.md) | Структура компонентів, шари, взаємодії |
| [Бізнес-логіка](business-logic/business-logic.md) | Правила предметної області автодилера |
| [Політика безпеки](security/security-policy.md) | Обов'язкові правила захисту під час розробки та експлуатації |
| [Політика конфіденційності](security/privacy-policy.md) | Правила обробки персональних та конфіденційних даних |
| [Модель загроз](security/threat-model.md) | STRIDE-аналіз загроз системи |

## Технологічний стек

- **Платформа:** .NET 8, ASP.NET Core MVC
- **База даних:** Microsoft SQL Server (LocalDB / SQL Express)
- **ORM:** Entity Framework Core 8 (Code-First)
- **Безпека:** ASP.NET Core Identity, Cookie Authentication, `System.Security.Cryptography`
- **IDE:** Visual Studio 2022 / Visual Studio Code
- **Контроль версій:** Git / GitHub
