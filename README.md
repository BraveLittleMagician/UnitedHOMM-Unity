# UnitedHOMM — Core Game Logic + Unity

Учебный проект по мотивам Heroes of Might and Magic: слой игровой логики на C# (.NET 6), отделённый от Unity, и Unity-слой на MVP.

## О проекте

Третья итерация проекта. Первые две версии были переписаны после анализа проблем с расширяемостью архитектуры. Текущая версия — чистая доменная логика без привязки к рендерингу и вводу, что позволяет тестировать её независимо от Unity. Unity-слой подключается через интерфейсы и реализует MVP.

## Архитектура

- **Слои**: доменная логика (`Logic`) ↔ инфраструктура (`EventBus`, `Logger`) ↔ Unity-слой (`Presentation`).
- **Сервисы**: `AbilityService`, `CombatService`.
- **Оркестратор**: `Flow` — единая точка входа для действий (`Move`, `MeleeAttack`, `ActivateModifier`, `EnPassantAttack`, `AmbushAttack`) с undo/redo.
- **Событийная модель**: `EventBus` (`PieceMovedEvent`, `PieceAttackedEvent`, `PieceDiedEvent`, `TurnStartedEvent` и др.).
- **Result-паттерн**: `Result` / `Result<T>` вместо исключений для ожидаемых ошибок (невалидный ход, нет зарядов и т.п.).
- **Движение**: иммутабельные `Pattern`, `RelativeArea`, `RelativeGraph`, `PathSetPattern`, `StepBudgetPattern`; `FacingEnumerator` — поворот формы относительно направления фигуры; `CombinedEnumerator` объединяет move- и attack-последовательности.
- **Модификаторы**: `ModifierHost`, `IActiveModifier`, `ITargetedModifier`, `IPureShapeModifier`, `IShapeTransformModifier`; отдельные модификаторы формы для `Pattern`, `RelativeArea`, `RelativeGraph`, `PathSetPattern`, `StepBudgetPattern`.
- **4D-координаты** (`Square`: X, Y, Z, W) — поддержка многослойных досок.
- **Undo/Redo**: `IUndoableOperation`, `MoveUndo`, `WorldDelta`, `WorldState.Capture/Restore`.

## Unity-слой (Presentation)

- **MVP**: Presenters / Views / State, контроллеры выбора (`SelectionController`, `TargetSelectionController`), фабрики представлений (`BoardViewFactory`, `PieceViewRegistry`).
- **DI**: композиционный корень `GameCompositionRoot` и модули `DomainModule`, `InputModule`, `PresentationModule`, `UiModule`, `DebugModule`.
- **UI (UGUI)**: `BoardUI`, `ActionChoiceUI`, `LayerPanelUI`, `PlanUI`, `PromotionUI`.
- **Камера и слои**: `CameraWork`, `LayerCameraWork`, `LayerContainer`, `LayerPositioner`, `LayerVisibilityPresenter`.
- **Маркеры и анимации**: `MarkerOrchestrator`, `AttackMarker`, `MoveTargetMarker`, `TargetTargetMarker`, `MoveAnimator`.
- **Данные**: загрузка конфигураций из JSON (`LoaderOfDataFromJSON`, `BoardLayoutLoader`, `PieceLibraryLoader`).
- **Debug**: `BoardDebugDumper`, `PieceDebugLogger`.

## Реализованные механики

- Перемещение фигур с проверкой пути, занятости клеток и границ доски.
- Атака в ближнем бою с автоматическим подходом к цели (BFS-поиск пути).
- Способности: En Passant, Ambush.
- Модификаторы с зарядами и триггерами (Attack Boost, Blind, Counter Attack, Fireball, Opening Double Move, Rotate, Self Heal, Stun и др.).
- Смерть фигур и автоматическое удаление из комнат (`Board`, `Decks`, `Graveyard`).
- Повышение фигур (Promotion) с выбором варианта.
- Undo/Redo и Commit плана действий.
- Управление ходами (`TurnManager`), проверка конца матча (`MatchEndChecker`).
- Загрузка конфигураций доски и библиотеки фигур из JSON.

## Тестирование

NUnit. 11 тестовых классов, покрывают:

- **Инварианты** (`InvariantsTests`): `Attackable ⊆ PositionsAll`, `Stayables ⊆ PositionsAll`, `PositionsStayables == {cell | PositionsAll[cell] == Stay}` — проверяются для всех типов энумераторов.
- **Round-trip свойства** (`FacingRotationTests`, `FacingTests`): `Rotate→Unrotate == Identity` для всех направлений и сэмплов.
- **Поведение** (`ApproachAndStrikeTests`): подход→удар, убийство→прыжок, undo/rollback, атака по диагонали без движения.
- **Граничные случаи** (`MovementBoundsTests`, `MovementBlockingTests`, `BoardIsInsideTests`): углы доски, блокировка союзником/врагом, окружённый король.
- **Параметризованные таблицы** (`ConfigOfBoardWWOffsetTests`): `[TestCase]` для `WWOffset` при разных `W` и `SignWNegative`.
- **Семантика движка** (`PatternTests`, `RelativeAreaTests`, `CombinedEnumeratorTests`): трансляция координат, объединение move/attack, `SupportsExplicitAttackable`.

Запуск: `dotnet test`.

## Технологии

C# 10+, .NET 6, nullable reference types, `System.Collections.Immutable`, `ArrayPool<T>`, кэширование (иерархия типов событий, хэши паттернов), `IDisposable`-паттерн, NUnit, Unity 6, UGUI, Shader Graph, ClosedXML (для утилиты TruthTable).

## Структура решения

```
Scripts/
├── Logic/
│   ├── Abstractions/     — интерфейсы, enum'ы, события
│   ├── Application/      — сервисы, Flow, оркестрация
│   ├── Domain/           — доменные модели (Piece, Properties, Abilities, Modifiers, Reactive)
│   ├── Geometry/         — Square, Square2D/3D, FacingRotation, Paths
│   ├── Infrastructure/    — EventBus, Logging, JSON-загрузка
│   ├── Movement/         — Movement, ModifierHost, модификаторы формы
│   ├── Rooms/            — Board, Decks, Graveyard, House, Registry
│   └── Sequence/         — Pattern, RelativeArea, RelativeGraph, PathSetPattern, Rules
└── Presentation/
    ├── Camera/           — работа с камерой и слоями
    ├── Core/             — GameCompositionRoot, DI-модули, контексты
    ├── Debug/            — отладочные дамперы и логгеры
    ├── Infrastructure/   — Unity-логгер, реестр префабов
    ├── Interaction/      — ввод, выделение, маркеры, сцена
    ├── Presenters/       — MVP-презентеры (Board, Layers, Markers, Piece)
    ├── State/            — состояние представления
    ├── UI/               — UGUI-панели
    └── Views/            — визуализация фигур
```

## Как запустить

- **Тесты**: `dotnet test`.
- **Unity-проект**: открыть в Unity 6, сцена `[укажи имя сцены]`, запустить Play.

## Статус

Слой логики реализован и покрыт юнит-тестами. Unity-слой работает: реализованы MVP-презентеры, UGUI, камера, маркеры, анимации и JSON-загрузка. В проекте есть функциональные баги, заметные в игровом процессе; ведётся доработка.

## Лицензия / ассеты

[Укажи лицензию, если есть. Если использовались сторонние ассеты — перечисли и укажи их лицензии.]