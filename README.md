# Inventory Reservation API

Учебный ASP.NET Core API на .NET 8, EF Core, SQLite / PostgreSQL и Swagger.

## Объектная модель

Warehouse содержит read-only коллекцию StockItems и создаёт остаток через
GetOrCreateStock(product). Коллекция остатков принадлежит только Warehouse.
StockItem связан с конкретными Product и Warehouse, хранит историю Movements,
управляет поступлением, резервом, освобождением и списанием.
Связи поддерживаются и для обычных объектов в памяти, и при загрузке EF Core.

SalesOrder владеет Items и Reservations, объединяет повторяющиеся позиции товара,
проверяет полноту резервирования и допустимые состояния.
StockReservation связывает позицию заказа с остатком, освобождает резерв при отмене
и выполняет операцию IStockOperation ровно один раз, проверяя фактическое списание.
InventoryMovement — неизменяемая запись истории с проверкой количества и ссылки на заказ.
Неизменяемая запись истории не обязана иметь изменяющие методы.

## Зависимости

- Domain не зависит от Application, EF Core, ASP.NET или БД.
- Application зависит от Domain и определяет прикладные сценарии.
- Infrastructure реализует порты Domain/Application через EF Core.
- API содержит HTTP-адаптеры и composition root (Program.Main).

Контроллер зависит от CatalogService и InventoryService, не получает DbContext.
CatalogService обслуживает каталог и запросы поиска.
InventoryService загружает объекты, вызывает их поведение и сохраняет изменения
одним SaveChangesAsync; реляционный провайдер сохраняет их в одной транзакции.

## Инверсия зависимостей и паттерны

ISalesOrderRepository загружает заказ вместе с позициями, резервами и нужными
остатками. IStockRepository загружает склад с остатками и кандидатов резервирования.
Это границы доступа к данным, а не generic repository для каждой таблицы:
Application не знает Include, SQL, SQLite или PostgreSQL.

IInventoryCatalog отделяет чтение/регистрацию каталога от EF.
IInventoryUnitOfWork задаёт границу атомарного сохранения.

IWarehouseSelectionStrategy имеет две реализации: максимум доступного остатка
и максимальный приоритет склада. В стратегии участвует актуальный Warehouse.Priority,
а не его дубликат в StockItem. Кандидаты сначала ограничиваются нужным количеством.

IInventoryOperationFactory выполняет изменение остатка напрямую и создаёт связанное движение.
InboundOperationFactory создаёт поступление, OutboundOperationFactory — отгрузку.
InventoryOperationExecutor получает IEnumerable<IInventoryOperationFactory> через DI
и выбирает фабрику по MovementType. Подмена регистрации заменяет поведение;
InventoryService не зависит от конкретных фабрик. Обёртка InventoryOperation удалена.

StockItem и SalesOrder имеют application-managed concurrency tokens.
Два контекста с одним старым токеном не могут успешно сохранить конкурирующие изменения.

## Локальный запуск с SQLite

~~~powershell
dotnet restore
dotnet run --project InventoryReservation.Api
~~~

Провайдер и строка подключения задаются в appsettings.json:
Database:Provider = Sqlite, ConnectionStrings:Inventory = Data Source=inventory_reservation.db.
Swagger доступен по адресу запуска из консоли с суффиксом /swagger.

SQLite использует EnsureCreated, поэтому старый SQLite-файл автоматически не обновляется.
После изменения модели используйте новую строку подключения, например
Data Source=inventory_reservation_v2.db. Старый файл сохраняется; данные не удаляются.
EnsureCreated и PostgreSQL-миграции нельзя применять к одному SQLite-файлу.

## PostgreSQL через Docker

~~~powershell
docker compose up --build
~~~

Compose переопределяет Database__Provider=Postgres и строку подключения.
Swagger: http://localhost:8080/swagger.
При запуске API применяет включённые PostgreSQL-миграции.
Для ручного применения:

~~~powershell
dotnet tool restore
dotnet ef database update --project InventoryReservation.Infrastructure --startup-project InventoryReservation.Api
~~~

Новые внешние ключи требуют корректных ссылок в уже существующей PostgreSQL-базе.
Миграция не удаляет историю или заказы; удаляется дублирующее поле WarehousePriority.

## HTTP endpoints

- POST /api/products, GET /api/products, GET /api/products/{id}
- POST /api/warehouses
- GET /api/stock, POST /api/stock/receipts
- POST /api/orders, GET /api/orders/{id}
- POST /api/orders/{id}/items, /confirm, /reserve, /cancel, /ship
- GET /api/inventory-movements

Product и Warehouse принимаются JSON-объектами с доменными конструкторами.
Операционные параметры передаются в query string.
DTO и публичных setters состояния заказа/остатка нет.
Циклические навигационные ссылки обрабатываются JSON IgnoreCycles.

## Проверка

~~~powershell
dotnet test InventoryReservation.sln
~~~

Доменные тесты проверяют остатки, состояния, полноту резерва, отмену и состав склада.
API-тесты используют SQLite in-memory для полного жизненного цикла и проверки
optimistic concurrency двумя независимыми DbContext.

