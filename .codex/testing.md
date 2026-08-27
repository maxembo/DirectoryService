# DirectoryService — правила тестирования

## Цель тестов

Тест должен доказывать конкретное observable behavior или инвариант. Количество тестов и coverage
сами по себе не доказывают корректность.

Перед написанием теста сформулируй:

- какое поведение проверяется;
- какой дефект тест должен поймать;
- на каком уровне этот сценарий дешевле и надёжнее проверить;
- что останется непроверенным после успешного прохождения теста.

Для bug fix regression test должен падать на старом поведении и проходить после исправления.

## Выбор уровня теста

Используй самый низкий уровень, который реалистично воспроизводит проверяемый риск:

- `Domain/unit test` — чистое бизнес-правило, Value Object или Entity без I/O.
- `Application test` — orchestration без PostgreSQL-specific поведения, если зависимости можно
  изолировать без большого количества mocks.
- `Integration test` — EF Core mapping, Dapper SQL, PostgreSQL constraints, `ltree`, transaction,
  cache и взаимодействие Handler с реальной инфраструктурой.
- `HTTP contract test` — routing, model binding, serialization, middleware, status code и response envelope.
- `Frontend unit test` — чистый formatter, selector или helper.
- `Frontend component/integration test` — пользовательское действие, состояние UI, query/mutation
  и взаимодействие нескольких компонентов.

Не заменяй PostgreSQL/Testcontainers тестом с EF Core InMemory или SQLite, если проверяются SQL,
constraints, locking, transaction или `ltree`.

## Текущее состояние проекта

Backend integration tests используют:

- xUnit;
- `DirectoryTestWebFactory` на базе `WebApplicationFactory<Program>`;
- реальный PostgreSQL через Testcontainers;
- migrations перед тестами;
- Respawn для очистки базы;
- `DirectoryBaseTests` с seed helpers и выполнением services/handlers через DI.

Большинство существующих backend tests вызывают Handler напрямую через DI. Они проверяют
Application + Infrastructure + PostgreSQL, но не доказывают корректность HTTP route, model binding,
middleware, serialization, status code и response envelope. Для этих рисков используй
`factory.CreateClient()` и отдельный HTTP contract test.

Frontend использует Vitest, `jsdom`, Testing Library, `jest-dom` и `user-event`.
Инфраструктура настроена, но текущее UI-покрытие минимально: наличие зависимостей не означает,
что пользовательские сценарии уже протестированы.

## Backend: структура теста

Следуй Arrange → Act → Assert. Комментарии секций допустимы, если тест остаётся читаемым.

Именование:

`<MethodOrScenario>_When<Condition>_Should<ExpectedBehavior>`

Примеры:

- `MoveDepartment_WhenTargetIsDescendant_ShouldReturnCycleError`
- `MoveDepartment_WhenParentIsNull_ShouldMoveToRoot`
- `GetPositions_WhenPageIsEmpty_ShouldPreserveTotalCount`

Используй `ShouldFail`, а не грамматически неверное `ShouldFailed`.

Правила:

- Один тест проверяет один логический сценарий; несколько assertions допустимы для одного инварианта.
- Сразу после Act проверь `IsSuccess`/`IsFailure`, затем обращайся к `.Value` или проверяй состояние БД.
- Для failure проверяй конкретный error code/type/field, а не только `IsFailure` или `NotEmpty`.
- Для command проверяй не только Result, но и сохранённое состояние, связи и побочные эффекты.
- Для query проверяй items, totals, order, фильтры, отсутствие дублей и empty page.
- Для дерева проверяй path/depth всего затронутого поддерева, parentId и защиту от циклов.
- Для cache проверяй поведение после mutation, а не только вызов метода invalidation.
- Для transaction/concurrency проверяй отсутствие частично сохранённого состояния.
- Используй `[Theory]`, когда один контракт проверяется на наборе входных данных.
- Не делай тест зависимым от порядка запуска или данных другого теста.

## Backend: данные и инфраструктура

- Используй helpers из `DirectoryBaseTests`, если они создают именно нужное состояние.
- Не расширяй общий helper параметрами для одного редкого теста; локальный builder/helper может быть яснее.
- Данные должны быть минимальными, но достаточными для различения правильного и неправильного результата.
- Для сортировки создавай данные, которые однозначно показывают направление и tie-breaker.
- Не используй случайные значения, если тест не сохраняет seed и не нуждается в property-based сценарии.
- Не вызывай production handler для Arrange, если тест проверяет этот же handler и setup можно безопасно
  выполнить через DbContext/domain factory.
- Не отключай Respawn и не полагайся на остаточные данные.
- Не используй общий mutable static state.
- Помни, что Testcontainers требует доступный Docker. Если Docker недоступен, сообщи о блокере
  и не утверждай, что integration tests прошли.

## HTTP contract tests

Добавляй HTTP test, когда изменение затрагивает:

- route или HTTP method;
- request/response DTO;
- model binding и query parameters;
- validation/error middleware;
- HTTP status code;
- serialization, nullable поля или response envelope;
- CORS/authentication/authorization, когда они появятся.

HTTP test должен отправлять реальный запрос через `HttpClient`, а не напрямую вызывать Controller.
Не дублируй через HTTP все Handler tests: выбери минимальный набор, доказывающий transport contract.

## Frontend: размещение и стиль

- Размещай `*.test.ts` или `*.test.tsx` рядом с тестируемым модулем, если проект не ввёл
  отдельное соглашение для integration tests.
- Проверяй поведение через доступные пользователю role, label, name и text.
- Используй `user-event` для кликов, ввода и keyboard interaction.
- Не вызывай внутренние event handlers напрямую.
- Не проверяй private state hook/store, если результат можно увидеть в UI или request parameters.
- Не используй snapshot как замену проверке взаимодействия.
- Не привязывайся к случайным CSS-классам, глубине DOM и внутренней структуре shadcn/ui.
- Для accessibility предпочитай `getByRole`, `getByLabelText` и проверку focus/disabled state.

## Frontend: TanStack Query и Zustand

- Создавай новый QueryClient для каждого теста и отключай retries, если они мешают детерминированности.
- Очищай Query cache и Zustand state между тестами.
- Mock располагай на согласованной API boundary, а не внутри TanStack Query.
- Не добавляй новую mocking-библиотеку только ради одного теста; сначала используй существующие
  возможности Vitest и текущую архитектуру API client.
- Проверяй, что query key учитывает параметры через observable request/cache behavior.
- Для mutation проверяй success, failure, disabled/loading state и требуемую invalidation/update.
- Для debounce используй fake timers только если проверяется именно временное поведение.

## Frontend: критические сценарии DirectoryService

Для дерева подразделений при соответствующем изменении проверь:

- первоначально запрашиваются только roots;
- expand загружает только прямых детей выбранного parent;
- повторный expand не создаёт лишний запрос к уже загруженной странице;
- `Показать ещё` запрашивает следующую страницу конкретного parent;
- новые страницы не создают дубликаты;
- leaf с `hasChildren = false` не раскрывается;
- initial loading, next-page loading, empty и error/retry отображаются отдельно;
- collapse/reopen сохраняет ожидаемое cache behavior;
- быстрые повторные клики не добавляют страницу дважды.

Для списков и фильтров проверь:

- request содержит выбранные filters, pagination и sort;
- изменение filter сбрасывает или пересчитывает pagination согласно контракту;
- empty result не запускает бесконечный `fetchNextPage`;
- элемент, связанный с несколькими выбранными сущностями, не дублируется.

Для карточек и dialogs проверь:

- активное/архивное состояние и доступные действия;
- длинные значения не меняют смысловую доступность элементов;
- submit нельзя повторить во время pending mutation;
- закрытие/повторное открытие даёт ожидаемое состояние формы;
- success и server error видимы пользователю;
- destructive или структурное изменение требует явного подтверждения, когда это предусмотрено UX.

## Время, даты и environment

- Не делай assertion зависимым от locale/timezone машины.
- Если поведение зависит от текущего времени, зафиксируй system time/fake timers.
- Не используй реальную сеть и внешние сервисы в обычных automated tests.
- Не добавляй произвольные задержки для устранения flaky test; найди ожидаемое событие или состояние.

## Команды

Из корня репозитория:

Backend integration test project:

- все: `dotnet test backend/DirectoryService/tests/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj`
- класс: `dotnet test backend/DirectoryService/tests/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj --filter "FullyQualifiedName~MoveDepartmentTests"`
- тест: `dotnet test backend/DirectoryService/tests/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj --filter "FullyQualifiedName~MoveDepartmentTests.MoveDepartment_WhenTargetIsDescendant_ShouldReturnCycleError"`

Frontend:

- все: `npm --prefix frontend run test:run`
- файл: `npm --prefix frontend run test:run -- src/path/component.test.tsx`
- watch во время локальной разработки: `npm --prefix frontend test`

После узкого теста запускай более широкий набор, если изменение затрагивает общий helper, contract,
cache, shared UI или несколько slices.

## Завершение тестовой задачи

Сообщи:

- какое поведение доказано;
- какой уровень теста выбран и почему;
- какая команда запускалась и её результат;
- что тест намеренно не проверяет;
- остался ли риск flaky/environment-dependent поведения.

Не называй Handler integration test «end-to-end» и не называй наличие mock assertion доказательством
реального SQL, HTTP или browser behavior.
