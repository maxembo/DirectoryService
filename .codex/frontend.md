# DirectoryService Frontend и FSD — правила разработки

Применяй эти правила для файлов внутри `frontend/`.

## Feature-Sliced Design

Используй существующие layers проекта и направление зависимостей:

`app → pages → widgets → features → entities → shared`

- Слой может импортировать только нижележащие layers.
- Не создавай импорт из нижнего слоя в верхний.
- Не связывай два независимых slice одного layer прямыми внутренними импортами.
- Между slices импортируй через публичный API `index.ts`.
- Внутри одного slice разрешены относительные импорты его собственных сегментов.
- Не добавляй реэкспорт во внешний public API, если символ не используется за пределами slice.
- Не отключай правило Steiger/ESLint для нового файла вместо исправления границы.
- Существующее точечное исключение не является разрешением расширять нарушение.

Назначение layers:

- `app` — providers, routing, global styles и composition приложения;
- `pages` — сборка страницы и page-specific orchestration;
- `widgets` — самостоятельные крупные блоки интерфейса;
- `features` — пользовательское действие или законченный use case;
- `entities` — domain data, entity API/model и представление сущности;
- `shared` — domain-agnostic UI, API infrastructure, helpers и configuration.

Не помещай domain-specific код в `shared` только ради повторного использования.

## Размещение кода внутри slice

- `ui` содержит отображение и обработчики UI-событий.
- `model` содержит UI/client state, selectors, hooks и orchestration slice.
- `api` содержит API functions, request types и query options.
- `lib` содержит внутренние чистые helpers slice, если они действительно нужны.
- Публичный API slice должен экспортировать минимальный набор, используемый другими slices.

Не создавай сегмент или отдельный файл только ради формального соответствия FSD.

## Server state и client state

- TanStack Query является источником истины для данных backend.
- Zustand хранит client/UI state: выбор, раскрытие, фильтры и локальные режимы представления.
- Не копируй response целиком из TanStack Query в Zustand.
- Если UI state формирует request, сначала получи параметры из Zustand/local state,
  затем передай их в query options.
- Query key должен включать все значения, влияющие на response.
- Query key и query function должны использовать один и тот же нормализованный request.
- Для mutations явно определи, какие queries нужно invalidate, update или remove.
- Не используй broad invalidation без причины, если можно точно определить затронутые данные.
- Не применяй optimistic update, пока не определены rollback и конфликт с server response.

## Дерево подразделений

- Загружай root-узлы отдельно от детей.
- Загружай только прямых детей раскрываемого узла.
- Храни pagination, loading и loaded-state отдельно по `parentId`.
- Не считай пустой массив достаточным признаком того, что данные уже загружены:
  различай `not loaded` и `loaded but empty`.
- Не запрашивай уже загруженную страницу повторно без invalidation или явного refresh.
- При добавлении страницы исключай дубликаты по `id`.
- `hasChildren` из API определяет отображение возможности раскрытия.
- Collapse не должен удалять уже загруженных детей из server cache без отдельной причины.
- Race между повторными кликами не должен добавлять одну страницу несколько раз.

## React и Next.js

- Добавляй `"use client"` только компонентам, которым нужны hooks, browser API или события.
- Держи client boundary как можно уже, но не дроби компонент без практической пользы.
- Не выполняй side effect во время render.
- Derived data вычисляй из источника истины, а не синхронизируй отдельным state через `useEffect`.
- Не используй `useMemo` и `useCallback` без измеримой причины или требования стабильной ссылки.
- Сохраняй корректные loading, empty, error и success states.
- Для списка используй устойчивый domain `id` как `key`, а не индекс массива.
- UI-компонент не должен знать детали Axios response envelope, если их может скрыть API/model layer.

## API и contracts

- Не дублируй одинаковые request/response types в нескольких slices.
- При изменении DTO проверь backend contract, API client, query options, UI и тесты.
- Не проглатывай `EnvelopeError`; преобразуй ошибку на согласованной границе.
- Не собирай URL или query string вручную в UI-компоненте.
- Для pagination явно определяй условие следующей страницы и не полагайся на длину списка,
  если API возвращает `page`, `totalPages` или `totalCount`.

## UI и формы

- Сохраняй существующие shadcn/ui и Tailwind-соглашения проекта.
- Не создавай второй generic UI-компонент, если существующий можно расширить без нарушения API.
- Accessibility является поведением: label, keyboard navigation, focus и disabled state должны
  сохраняться после изменений.
- Не прячь бизнес-правило только в disabled-состоянии кнопки; backend всё равно должен
  защищать соответствующий инвариант.

## Карточки сущностей

Перед созданием новой карточки изучи ближайшую карточку той же сущности и общие primitives
из `shared/components/ui/card`. Не создавай собственный базовый `Card`, если существующий
компонент покрывает сценарий.

Текущие визуальные соглашения:

- внешний контейнер использует `Card` с `min-w-0 transition-shadow hover:shadow-md`;
- заголовок и действия располагаются через grid `minmax(0, 1fr) auto`, чтобы длинный текст
  не выталкивал кнопки;
- название использует `CardTitle`, `min-w-0` и `truncate`;
- кнопки и badges имеют `shrink-0`, если их размер не должен зависеть от длины текста;
- статус отображается через существующий `Badge` и согласованные иконки активного/архивного состояния;
- metadata оформляется `text-muted-foreground`, а иконки — `shrink-0`;
- для смысловых групп используй `CardHeader`, `CardContent`, при необходимости `Separator`;
- длинные `id`, `path`, address и даты не должны ломать ширину карточки;
- действия редактирования, удаления и восстановления остаются features, а карточка только
  компонует их и передаёт необходимые identifiers/callbacks.

Не копируй карточку целиком ради изменения одного поля. Сначала проверь, является ли повторение
устойчивым общим UI-паттерном. Выноси общий компонент только после появления как минимум двух
реальных одинаковых сценариев и не смешивай в нём domain-specific поведение разных сущностей.

При добавлении карточки проверь:

- active и archived состояния;
- длинное название и длинные metadata;
- отсутствие optional полей;
- keyboard/focus behavior действий;
- loading/disabled state mutations;
- поведение в узкой колонке;
- пользовательский сценарий через Testing Library, если карточка содержит действия.

## Тестирование

- Проверяй observable behavior через Testing Library и `user-event`.
- Не привязывай тест к внутреннему state, приватному hook или структуре DOM без необходимости.
- Для query/mutation проверяй request parameters, отображаемые состояния и cache behavior,
  относящиеся к сценарию.
- Для дефекта добавляй regression test на пользовательский сценарий.
- Не заменяй component/integration test snapshot-тестом, если важны взаимодействия.

## Команды

Из корня репозитория:

- общая статическая проверка: `npm --prefix frontend run check`
- тесты: `npm --prefix frontend run test:run`
- конкретный тест: `npm --prefix frontend run test:run -- <test-file>`
- проверка форматирования: `npm --prefix frontend run format:check`
- production build: `npm --prefix frontend run build`

Сначала запускай конкретный тест или затронутую проверку. `build` запускай при изменении
Next.js boundaries, routing, configuration или общей сборки.

## Code Review Rules

- Отмечай нарушение направления импортов FSD и обход public API.
- Отмечай server state, продублированный в Zustand или local state.
- Отмечай query key, который не отражает все параметры запроса.
- Отмечай `useEffect`, синхронизирующий derived state без необходимости.
- Отмечай бесконечную pagination без детерминированного `getNextPageParam`.
- Отмечай отсутствие различия между initial loading, fetch next page и empty state.
- Для каждого замечания показывай пользовательский сценарий отказа и минимальное исправление.
