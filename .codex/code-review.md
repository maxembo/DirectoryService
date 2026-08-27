# DirectoryService — правила code review

## Цель review

Найти дефекты и риски до merge, а не переписать код под личные предпочтения reviewer.
Review проверяет observable behavior, contracts и архитектурные границы.

По умолчанию анализируй diff относительно указанной base branch. Не расширяй review на весь
репозиторий, если окружающий код не нужен для проверки изменения.

Не изменяй файлы в режиме review без прямого запроса.

## Перед review

1. Определи цель изменения по задаче, PR или commit history.
2. Изучи diff и затронутые contracts.
3. Найди вызывающий и вызываемый код, необходимый для проверки поведения.
4. Проверь связанные тесты и выполненные проверки.
5. Отдели дефект текущего diff от уже существующей проблемы.

Если цель изменения неизвестна, сначала сформулируй предполагаемый contract и явно отметь
предположение.

## Порядок приоритетов

Проверяй findings в таком порядке:

1. `Blocker` — потеря данных, нарушение безопасности, несовместимый contract или гарантированный сбой.
2. `High` — неправильное поведение основного сценария, concurrency bug, нарушение транзакции.
3. `Medium` — edge case, некорректная invalidation, деградация производительности, слабая тестовая защита.
4. `Low` — локальная сопровождаемость, которая с высокой вероятностью приведёт к ошибке позже.
5. `Suggestion` — необязательное улучшение без текущего дефекта.

Не создавай finding только из-за форматирования, naming или личного вкуса, если это уже проверяется
formatter/linter либо не создаёт технического риска.

## Формат finding

Для каждого finding укажи:

- severity;
- конкретный файл и минимальный диапазон строк;
- фактическую причину;
- воспроизводимый сценарий или входные данные;
- ожидаемое и текущее поведение;
- минимальный безопасный вариант исправления.

Не пиши абстрактно «может сломаться». Покажи путь от входа до неправильного результата.
Если уверенность недостаточна, оформи это как вопрос или область проверки, а не как подтверждённый дефект.

## Backend review checklist

- Command/query semantics и границы Handler/Domain/Infrastructure.
- Input validation, domain invariants и database constraints.
- Result failure обработан до `.Value`.
- Correct cancellation и отсутствие sync-over-async.
- Transaction boundary, concurrency, idempotency и частичные изменения.
- PostgreSQL/`ltree` semantics, параметризация Dapper и отсутствие N+1.
- Детерминированная pagination и корректные totals.
- Cache key, invalidation, stale data и race conditions.
- API/error contract, HTTP status и обратная совместимость.
- Логи не содержат secrets или лишние персональные данные.

## Frontend review checklist

- Направление зависимостей FSD и imports через public API.
- Server state не дублируется в Zustand/local state.
- Query key включает все параметры, влияющие на response.
- Mutation корректно обновляет или инвалидирует cache.
- Нет race, duplicate page или бесконечного `getNextPageParam`.
- Различаются initial loading, background fetching, next-page loading, empty и error states.
- `useEffect` не синхронизирует derived state без необходимости.
- Next.js client boundary не расширена без причины.
- UI сохраняет accessibility, focus, keyboard behavior и responsive layout.
- API contract и nullable/optional поля обработаны явно.

## Проверка тестов

- Тест доказывает изменённое поведение, а не детали реализации.
- Для bug fix существует regression case.
- Самый опасный failure/edge case покрыт либо явно указан как оставшийся риск.
- PostgreSQL-specific поведение не подменено нерепрезентативным mock/InMemory test.
- Frontend interaction проверяется через поведение пользователя.

Не требуй тест для чистого механического изменения, если существующая статическая проверка полностью
доказывает его корректность.

## Итог review

Сначала перечисли findings. Затем кратко укажи:

- `Decision`: approve / request changes / needs evidence;
- что было проверено;
- какие проверки запускались;
- что осталось вне review;
- один главный остаточный риск.

Если findings нет, не придумывай замечания ради наполнения ответа.
