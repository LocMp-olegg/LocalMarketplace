# LocalMarketplace — «Районный»

Бэкенд гиперлокального маркетплейса для цифровизации купли-продажи товаров внутри ограниченного территориального образования (жилой комплекс, микрорайон). Позволяет локальным продавцам (самозанятые, малый бизнес) взаимодействовать с покупателями в радиусе прямой доступности.

Дипломная работа (ВКР). Стек: .NET 10 / ASP.NET Core / PostgreSQL / RabbitMQ / Redis.

---

## Архитектура

Система реализована в виде набора независимых микросервисов, каждый со своей базой данных (схемой PostgreSQL). Взаимодействие между сервисами — через брокер сообщений (RabbitMQ/MassTransit) и синхронные HTTP-вызовы для критичных операций.

Диаграммы (PlantUML):

- [Архитектурный обзор](docs/diagrams/system-overview.puml)
- [Интеграционные события](docs/diagrams/integration-events.puml)
- [IdentityService](docs/diagrams/identity-service.puml)
- [CatalogService](docs/diagrams/catalog-service.puml)
- [OrderService](docs/diagrams/order-service.puml)
- [ReviewService](docs/diagrams/review-service.puml)
- [NotificationService](docs/diagrams/notification-service.puml)
- [AnalyticsService](docs/diagrams/analytics-service.puml)

---

## Сервисы

| Сервис | Порт | Описание |
|---|---|---|
| Gateway | 5000 | Ocelot API Gateway, JWT-валидация, роутинг |
| IdentityService | 5001 | Пользователи, роли, аутентификация (Duende IdentityServer) |
| CatalogService | 5002 | Товары, категории, остатки, геофильтрация, избранное |
| OrderService | 5003 | Корзина, заказы, жизненный цикл, споры, курьер |
| ReviewService | 5004 | Отзывы, рейтинги, модерация |
| NotificationService | 5005 | In-App и email-уведомления (MailKit) |
| AnalyticsService | 5006 | Дашборды продавца и администратора, pre-computed агрегаты |

Документация API (Swagger): `http://localhost:{port}/swagger` для каждого сервиса в режиме `Development`.

---

## Технологический стек

| Компонент | Технология |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| ORM | Entity Framework Core 10 |
| CQRS | MediatR |
| Валидация | FluentValidation |
| Маппинг | AutoMapper |
| Аутентификация | Duende IdentityServer 7 + Microsoft Identity |
| Gateway | Ocelot |
| Message Broker | RabbitMQ + MassTransit |
| База данных | PostgreSQL + PostGIS (геопространственные данные) |
| Кэш | Redis (StackExchange.Redis) |
| Объектное хранилище | MinIO (S3-совместимое) |
| Email (dev) | MailHog (перехват SMTP) |
| Email (prod) | MailKit (SMTP) |
| Геопространство | NetTopologySuite |
| Логирование | Serilog |
| Контейнеризация | Docker / Docker Compose |

---

## Быстрый старт

### Требования

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (или Docker Engine + Compose)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) — только для локального запуска без Docker

### Запуск через Docker Compose

```bash
# Клонировать репозиторий
git clone <repo-url>
cd LocalMarketplace

# Поднять всю инфраструктуру и сервисы
docker compose up -d

# Посмотреть статус
docker compose ps
```

После запуска:

| Адрес | Что |
|---|---|
| http://localhost:5000/swagger | API Gateway |
| http://localhost:5001/swagger | IdentityService |
| http://localhost:5002/swagger | CatalogService |
| http://localhost:5003/swagger | OrderService |
| http://localhost:5004/swagger | ReviewService |
| http://localhost:5005/swagger | NotificationService |
| http://localhost:5006/swagger | AnalyticsService |
| http://localhost:15672 | RabbitMQ Management (guest / guest) |
| http://localhost:9001 | MinIO Console (minioadmin / minioadmin) |
| http://localhost:8025 | MailHog — перехват исходящей почты |

### Остановка

```bash
docker compose down

# С удалением volumes (база данных, очереди, файлы)
docker compose down -v
```

---

## Локальный запуск без Docker

Для локальной разработки отдельного сервиса запустите инфраструктуру через Docker Compose, а сам сервис — из IDE или командной строки.

```bash
# Поднять только инфраструктуру
docker compose up -d postgres rabbitmq redis minio mailhog

# Запустить конкретный сервис
cd IdentityService/src/LocMp.Identity.Api
dotnet run
```

---

## Миграции базы данных

EF Core миграции применяются автоматически при старте каждого сервиса (`app.ApplyMigrations()` в `Program.cs`). Для ручного управления:

```bash
# Пример для CatalogService (выполнять из папки src/ сервиса)
dotnet ef migrations add MigrationName \
  --project LocMp.Catalog.Infrastructure \
  --startup-project LocMp.Catalog.Api

dotnet ef database update \
  --project LocMp.Catalog.Infrastructure \
  --startup-project LocMp.Catalog.Api
```

---

## Тесты

Каждый сервис содержит проект с юнит-тестами в папке `tests/`. Используются xUnit, NSubstitute и EF Core InMemory.

### Запуск тестов одного сервиса

```bash
cd OrderService && dotnet test
cd CatalogService && dotnet test
cd IdentityService && dotnet test
cd ReviewService && dotnet test
cd NotificationService && dotnet test
cd AnalyticsService && dotnet test
```

### Запуск всех тестов

```bash
for svc in OrderService CatalogService ReviewService IdentityService NotificationService AnalyticsService; do
  (cd $svc && dotnet test -v q)
done
```

### Фильтрация

```bash
# Запустить конкретный тест
dotnet test --filter "FullyQualifiedName~Handle_SufficientStock"

# Запустить все тесты класса
dotnet test --filter "ClassName~ReserveStockCommandHandlerTests"
```

## Структура решения

```
LocalMarketplace/
├── BuildingBlocks/          # Переиспользуемые библиотеки (Domain, Application, Infrastructure)
├── Contracts/               # Интеграционные события (sealed record : IIntegrationEvent)
├── Gateway/                 # Ocelot API Gateway
├── IdentityService/         # Пользователи, аутентификация (порт 5001)
├── CatalogService/          # Товары, магазины и каталог (порт 5002)
├── OrderService/            # Заказы (порт 5003)
├── ReviewService/           # Отзывы (порт 5004)
├── NotificationService/     # Уведомления (порт 5005)
├── AnalyticsService/        # Аналитика (порт 5006)
├── docs/
│   └── diagrams/            # PlantUML-диаграммы
├── postman/                 # Коллекции Postman
└── docker-compose.yml
```

Каждый сервис следует Clean Architecture:

```
{Service}/
├── src/
│   ├── LocMp.{Service}.Api             # Контроллеры, Program.cs
│   ├── LocMp.{Service}.Application     # Commands, Queries, Handlers, DTOs, Validators
│   ├── LocMp.{Service}.Domain          # Сущности, перечисления, доменные события
│   └── LocMp.{Service}.Infrastructure  # EF Core, миграции, MassTransit, внешние сервисы
└── tests/
    └── LocMp.{Service}.Application.Tests
```

---

## Роли пользователей

| Роль | Возможности |
|---|---|
| `User` | Покупка товаров, корзина, история заказов, отзывы |
| `Seller` | Управление каталогом, просмотр своих заказов, аналитика |
| `Courier` | Просмотр назначенных заказов, обновление статуса доставки |
| `Admin` | Полный доступ, модерация пользователей и контента, административная аналитика |

---

## Жизненный цикл заказа

```
Pending → Confirmed → ReadyForPickup → Completed
                    → InDelivery     → Completed
        → Cancelled
        → Disputed
```

Переход `Completed` разблокирует возможность оставить отзыв в ReviewService.
Отмена заказа публикует `StockReleasedEvent` — CatalogService возвращает зарезервированный остаток.

---

## Конфигурация (переменные окружения)

Все сервисы принимают конфигурацию через переменные окружения. Ключевые параметры:

```
ConnectionStrings__LocalMarketplaceDb   # Строка подключения PostgreSQL
ConnectionStrings__RabbitMq             # AMQP URI RabbitMQ
ConnectionStrings__Redis                # Redis endpoint (host:port)
JwtAuthenticationOptions__Authority     # URL IdentityService для валидации JWT
Minio__Endpoint                         # MinIO endpoint
Smtp__Host / Smtp__Port                 # SMTP для email-уведомлений
```