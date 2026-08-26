# DirectoryService Backend — правила разработки

Применяй эти правила для файлов внутри `backend/`.

## Архитектурные границы

- `Domain` содержит бизнес-состояние и инварианты и не зависит от Application,
  Infrastructure, Presentation или Web.
- `Application` координирует use cases, но не содержит PostgreSQL-, HTTP- или UI-specific деталей.
- `Infrastructure.Postgres` реализует persistence и PostgreSQL-specific поведение.
- `Presentation` преобразует HTTP input в commands/queries и формирует HTTP response.
- `Contracts` содержит внешний API contract, а не внутренние domain entities.
- `Web` является composition root: DI, middleware, endpoints и запуск приложения.

Перед добавлением нового слоя или сервиса проверь, нельзя ли разместить поведение в существующем
vertical slice без новой абстракции.

## Commands и queries

- Command изменяет состояние; query только читает его.
- Handler отвечает за orchestration, а не за сокрытие бизнес-правил в длинном procedural-коде.
- Бизнес-инварианты, относящиеся к состоянию domain entity, защищай внутри Domain.
- Input validation проверяет форму запроса. Domain validation защищает допустимость изменения
  бизнес-состояния. Ограничение БД является последней защитой целостности.
- Не запускай write-operation из query.
- Для read-сценария разрешён прямой EF Core/Dapper projection в DTO, если Domain behavior не нужен.
- Не загружай полные entities только для формирования read DTO без конкретной причины.

## Result-типы и ошибки

- Обработай failure до доступа к `.Value`.
- Не преобразовывай ожидаемую бизнес-ошибку в исключение.
- Не скрывай неожиданное исключение общим `try/catch` без обработки или полезного контекста.
- Сохраняй единый error contract и корректное соответствие HTTP status codes.
- Не раскрывай внутренние exception details и секреты во внешнем ответе.

## Persistence и PostgreSQL

- Для EF Core учитывай tracking и границу транзакции; для read-only queries используй
  `AsNoTracking`, когда tracking действительно не нужен.
- Dapper-запросы должны быть параметризованы. Не собирай SQL из пользовательских значений.
- Для пагинации используй детерминированную сортировку с уникальным tie-breaker.
- Не предполагай, что EF Core InMemory или SQLite воспроизводят поведение PostgreSQL, `ltree`,
  locking и constraints.
- Изменения дерева подразделений должны сохранять корректность `ltree` path, запрещать циклы
  и учитывать конкурентные перемещения.
- Операции над несколькими связанными записями выполняй атомарно, когда частичный результат
  нарушит инварианты.
- Не редактируй уже применённую migration ради изменения схемы. Создавай новую migration.
- Не изменяй вручную model snapshot без подтверждённой необходимости.

## Cache

- Cache не является источником истины.
- Перед добавлением cache определи key, срок жизни, допустимость stale data и стратегию invalidation.
- После command инвалидируй только ключи, поведение которых действительно изменилось.
- Не маскируй ошибку базы возвратом устаревших данных без явного требования.
- Проверяй race conditions между изменением данных, invalidation и повторным чтением.

## API и совместимость

- При изменении contract проверь Controller, Contracts, frontend API client и integration tests.
- Не меняй значение существующего поля или HTTP status code молча.
- Для коллекций и pagination сохраняй согласованность `items`, `totalCount`, `page`, `pageSize`
  и `totalPages`.
- Пробрасывай `CancellationToken` через Handler, repository/DbContext, Dapper и внешние I/O.
- Логируй идентификаторы операции и полезный контекст, но не пароли, токены и персональные данные.

## Тестирование

- Чистые domain-инварианты проверяй unit/domain tests.
- EF Core mappings, PostgreSQL constraints, `ltree`, Dapper, транзакции и HTTP contract
  проверяй integration tests с PostgreSQL/Testcontainers.
- Для дефекта сначала добавляй regression test, если он воспроизводим и не требует
  непропорционально большой инфраструктуры.
- Тест должен доказывать observable behavior, а не повторять внутреннюю реализацию Handler.
- Проверяй как success path, так и наиболее опасный failure/edge case.

## Команды

Из корня репозитория:

- сборка: `dotnet build backend/DirectoryService/DirectoryService.sln`
- все тесты: `dotnet test backend/DirectoryService/DirectoryService.sln`
- узкий тест: `dotnet test backend/DirectoryService/DirectoryService.sln --filter "FullyQualifiedName~<TestName>"`

После изменения сначала запускай тест затронутого поведения, затем сборку или полный набор тестов,
если изменение затрагивает несколько проектов или общие contracts.

## Code Review Rules

- Отмечай handler, который одновременно валидирует input, реализует domain behavior,
  формирует SQL и управляет HTTP response.
- Отмечай read-query с побочным изменением состояния.
- Отмечай доступ к result `.Value` без доказанной success-ветки.
- Отмечай mutation без анализа транзакции, constraints и cache invalidation.
- Отмечай недетерминированную pagination и N+1 queries.
- Для каждого замечания показывай конкретный сценарий отказа и минимальный безопасный путь.
