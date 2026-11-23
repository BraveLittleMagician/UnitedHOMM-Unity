# UnitedHOMM — Core Game Logic

Учебный проект по мотивам Heroes of Might and Magic: слой игровой логики на C# (.NET 6), отделённый от Unity.

## О проекте

Третья итерация проекта. Первые две версии были переписаны после анализа проблем с расширяемостью архитектуры. Текущая версия — чистая доменная логика без привязки к рендерингу и вводу, что позволяет тестировать её независимо от Unity.

## Архитектура

- **Слои**: доменная логика (Core) ↔ инфраструктура (EventBus, Logger) ↔ Unity-слой (рендеринг, ввод) — последний подключается через интерфейсы.
- **Сервисы**: `AbilityService`, `CombatService`.
- **Оркестратор**: `Flow` — единая точка входа (Move, MeleeAttack, ActivateModifier, EnPassantAttack, AmbushAttack) с undo/redo.
- **Событийная модель**: `EventBus` (`PieceMovedEvent`, `PieceAttackedEvent`, `PieceDiedEvent`, ...).
- **Result-паттерн**: `Result` / `Result<T>` вместо исключений для ожидаемых ошибок.
- **Движение**: иммутабельные `Pattern`, `RelativeArea`, `RelativeGraph`, `PathSetPattern`; `FacingEnumerator` — поворот формы относительно направления фигуры; `Movement` объединяет move- и attack-последовательности.
- **Модификаторы**: `ModifierHost`, `IActiveModifier`, `ITargetedModifier` (En Passant, Ambush).
- **4D-координаты** (`Square`: X, Y, Z, W) — поддержка многослойных досок.
- **Undo/Redo**: `IUndoableOperation`, `WorldState.Capture/Restore`.

## Unity-слой (Presentation)

- **MVP**: Presenters / Views / State, контроллеры выбора (`SelectionController`, `TargetSelectionController`), фабрики представлений (`BoardViewFactory`, `PieceViewRegistry`).
- **DI**: композиционный корень `GameCompositionRoot` и модули `DomainModule`, `InputModule`, `PresentationModule`, `UiModule`, `DebugModule`.
- **UI (UGUI)**: `BoardUI`, `ActionChoiceUI`, `LayerPanelUI`, `PlanUI`, `PromotionUI`.
- **Камера и слои**: `CameraWork`, `LayerCameraWork`, `LayerContainer`, `LayerPositioner`, `LayerVisibilityPresenter`.
- **Маркеры и анимации**: `MarkerOrchestrator`, `AttackMarker`, `MoveTargetMarker`, `TargetTargetMarker`, `MoveAnimator`.
- **Данные**: загрузка конфигураций из JSON (`LoaderOfDataFromJSON`, `BoardLayoutLoader`, `PieceLibraryLoader`).
- **Debug**: `BoardDebugDumper`, `PieceDebugLogger`.

## Реализованные механики

- Перемещение фигур с проверкой пути, занятости и границ доски.
- Атака в ближнем бою с автоматическим подходом к цели (BFS-поиск пути).
- Способности: En Passant, Ambush.
- Модификаторы с зарядами и триггерами.
- Смерть фигур и автоматическое удаление из комнат.
- Undo/Redo и Commit плана действий.

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

C# 10+, .NET 6, nullable reference types, `System.Collections.Immutable`, `ArrayPool<T>`, кэширование (иерархия типов событий, хэши паттернов), `IDisposable`-паттерн, NUnit.

## Структура решения

[скриншот Solution Explorer или список проектов: Core, Tests, Unity — что есть]

## Статус

Слой логики реализован и покрыт юнит-тестами. Unity-слой работает: реализованы MVP-презентеры, UGUI, камера, маркеры, анимации и JSON-загрузка. В проекте есть функциональные баги, заметные в игровом процессе; ведётся доработка.

## Лицензия / ассеты

[если использовались сторонние ассеты — указать]
