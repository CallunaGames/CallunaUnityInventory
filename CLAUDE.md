# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this package is

`com.calluna.inventory` is a generic, DI-aware inventory system for Unity. Items are property bags; filtering and sorting are pluggable strategies; the Container exposes a reactive, filtered+sorted view of its slots.

## Running tests

Tests live in `Tests~/`. Make them visible by renaming to `Tests/`, then run via Unity Test Runner or:

```bash
Unity -runTests -testPlatform EditMode -projectPath "C:/Users/sebas/CallunaPackages"
```

The test folder currently contains only a template — no tests are implemented yet.

## Architecture

### Item system (property-bag composition)

`Item` is a `Dictionary<Type, ItemProperty>` at its core. All item data is added as typed `ItemProperty` subclasses (e.g. `ItemName`, `ItemAmount`, or custom ones like `ItemId` in the sample). This allows any domain to extend items without modifying `Item` itself.

```
Item
└─ Dictionary<Type, ItemProperty>
   ├─ ItemName  → Observable<string>
   ├─ ItemAmount → Observable<int>
   └─ (custom properties...)
```

### Container and slots

`Container` is the central class (full DI lifecycle: Injectable → Initializable → Cleanable). It maintains:
- `Slots` — raw `ObservableList<Slot>`, each `Slot` wraps one `Item` in an `Observable<Item>`
- `ActiveSlots` — read-only filtered + sorted projection, rebuilt reactively whenever Filter or Sorter signals `OnChanged`

CRUD is delegated to `ContainerAccessor` (injected). `EndlessContainerAccessor` is the provided implementation; it uses `SlotProvider` (injected) to create/return slots.

### Filtering

`Filter` is an abstract base (full lifecycle). Key members: `IsActive` (`Observable<bool>`), `OnChanged` event, and `ApplyTo(IEnumerable<Slot>)` which calls the abstract `ApplyTo(Item)` predicate.

`FilterGroup` composes multiple filters with AND logic. Container resolves Filter optionally — it works without any filter.

### Sorting

`Sorter` is an abstract base (full lifecycle). `Sorter<TKey>` adds LINQ-based ordering via `GetKey(Slot)`. Two composition strategies:

| Type | Behaviour |
|---|---|
| `LayeredSorter` | Chains sorters sequentially (Sort → ThenBy → …) |
| `RadioSorter` | Only one child sorter is active at a time (mutually exclusive) |

Container resolves Sorter optionally.

### Reactive flow

State changes propagate via CallunaCore observables and C# events:

```
UI action → Filter/Sorter mutation → OnChanged fires
→ Container.UpdateActiveSlots() → ActiveSlots rebuilt
→ UI listeners on ActiveSlots.OnItemAdded/Removed react
```

## DI wiring (see EndlessInventory sample)

```csharp
binder.Bind<Filter>().And<NameFilter>().ToNew<NameFilter>().AsSingle();
binder.BindToNewSelf<Container>().AsSingle();
binder.Bind<ContainerAccessor>().ToNew<EndlessContainerAccessor>().AsSingle();
binder.Bind<SlotProvider>().ToNew<SlotCreator>().AsSingle();
binder.BindToNewSelf<RadioSorter>().AsSingle();
binder.BindToNewSelf<LayeredSorter>().AsSingle();
```

`Container` resolves `Filter` and `Sorter` via `ResolveOptional` — both are optional.

## Extension points

- **Custom item properties**: subclass `ItemProperty`, add to an `Item` instance.
- **Custom filters**: subclass `Filter`, implement `ApplyTo(Item)`.
- **Custom sorters**: subclass `Sorter<TKey>`, implement `GetKey(Slot)`.
- **Slot constraints**: subclass `SlotRule` (currently empty base) and `ContainerAccessor` (override `CanAdd`/`CanRemove`).
- **Slot pooling**: subclass `SlotProvider` instead of using `SlotCreator` (sample's `EntryCreator` shows the pattern).
