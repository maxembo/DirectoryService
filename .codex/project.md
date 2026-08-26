# DirectoryService — контекст и общие правила проекта

## Источники истины

Фактический код, тесты и конфигурационные файлы имеют приоритет над этим документом.

Перед изменением:

- изучи затронутый код и ближайший похожий vertical slice;
- найди связанные тесты;
- проверяй версии и команды в `.csproj`, `Directory.Packages.props`, `package.json`
  и конфигурационных файлах;
- если этот документ противоречит коду, явно сообщи о расхождении;
- не обновляй этот документ автоматически без фактического изменения решения проекта.

## Назначение проекта

DirectoryService — учебный full-stack сервис для управления организационной структурой:
подразделениями, должностями, локациями и связями между ними.

Цель разработки — получить работающий продукт и разобраться в архитектуре, границах
ответственности, работе с данными, тестировании и эксплуатации.

## Структура репозитория

- `backend/DirectoryService/src/DirectoryService.Domain` — domain model и бизнес-инварианты.
- `backend/DirectoryService/src/DirectoryService.Application` — commands, queries, handlers
  и application orchestration.
- `backend/DirectoryService/src/DirectoryService.Infrastructure.Postgres` — EF Core, Dapper,
  repositories, migrations и PostgreSQL-specific реализация.
- `backend/DirectoryService/src/DirectoryService.Presentation` — HTTP controllers и presentation layer.
- `backend/DirectoryService/src/DirectoryService.Contracts` — внешние request/response contracts.
- `backend/DirectoryService/src/DirectoryService.Web` — composition root и запуск приложения.
- `backend/DirectoryService/tests/DirectoryService.IntegrationTests` — integration tests.
- `frontend/src` — Next.js-приложение, организованное по Feature-Sliced Design.
- `compose.yaml` — PostgreSQL, Redis, Seq и backend-контейнер.

## Технологии

Backend:

- C# и ASP.NET Core;
- EF Core и Dapper;
- PostgreSQL, включая `ltree`;
- FluentValidation;
- Redis и HybridCache;
- CSharpFunctionalExtensions и result-типы;
- xUnit, Testcontainers и Respawn;
- Serilog и Seq.

Frontend:

- TypeScript, Next.js и React;
- TanStack Query для server state;
- Zustand для client/UI state;
- Axios;
- Feature-Sliced Design;
- Vitest и Testing Library;
- ESLint, Prettier и Steiger.

Infrastructure:

- Docker Compose;
- PostgreSQL;
- Redis;
- Seq.

Не полагайся на версии из текста: проверяй текущие manifest-файлы.

## Основные потоки данных

Backend write path:

`HTTP → Controller → Command → input validation → Handler → Domain → Repository/DbContext → PostgreSQL`

Backend read path:

`HTTP → Controller → Query → input validation → Handler → EF Core/Dapper → DTO → HTTP response`

Не предполагается, что read-query обязательно проходит через Domain или Repository.

Frontend query path:

`UI state → request parameters → TanStack Query → API client/Axios → HTTP → Query cache → render`

Frontend mutation path:

`UI → mutation → API → backend → invalidate/update Query cache → render`

Zustand и TanStack Query не являются взаимозаменяемыми: Zustand хранит client/UI state,
а TanStack Query — server state.

## Общие инженерные правила

- Следуй локальным соглашениям ближайшего похожего slice, если они не нарушают текущий контракт.
- Делай минимальные логически связанные изменения без побочного рефакторинга.
- Не добавляй framework, библиотеку, паттерн или инфраструктуру без конкретной потребности.
- Не создавай interface, generic abstraction, base class или service только «на будущее».
- Различай input validation, domain validation и ограничения базы данных.
- Не извлекай `.Value` из result-типа, пока failure не обработан на той же границе.
- Передавай `CancellationToken` до реальных I/O-операций.
- При изменении данных учитывай транзакции, concurrency, idempotency, ограничения БД
  и cache invalidation, когда они относятся к сценарию.
- Для API учитывай error contract, HTTP status codes и обратную совместимость.
- Не дублируй server state в Zustand без явной причины.
- Не смешивай обновление зависимостей или архитектурную миграцию с функциональной задачей.

## Текущие инварианты дерева подразделений

- При открытии дерева загружаются только root-узлы.
- При раскрытии узла загружаются только его прямые дети.
- Уже загруженная страница детей не запрашивается повторно без осознанной invalidation.
- `hasChildren` из backend определяет возможность раскрытия узла.
- Пагинация и состояние загрузки детей разделены по `parentId`.
- Добавление следующей страницы не должно создавать дубликаты детей.
- Выбор подразделения определяет запрос списка его должностей.
- Полное дерево не собирается и не загружается заранее.

Если реализация или требования изменились, сначала зафиксируй новый инвариант и только потом
удаляй старый.

## Базовые команды проверки

Из корня репозитория:

Backend:

- `dotnet build backend/DirectoryService/DirectoryService.sln`
- `dotnet test backend/DirectoryService/DirectoryService.sln`
- для узкой проверки использовать `dotnet test ... --filter "FullyQualifiedName~..."`

Frontend:

- `npm --prefix frontend run check`
- `npm --prefix frontend run test:run`
- `npm --prefix frontend run build`
- `npm --prefix frontend run format:check`

Сначала запускай наиболее узкую релевантную проверку. `build` и полный набор тестов запускай,
когда изменение затрагивает соответствующие границы или узкие проверки недостаточны.
