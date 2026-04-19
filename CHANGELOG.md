## [1.1.0] - 2026-04-19

### Breaking Changes
- `Container.Accessor` public property has been removed. Callers that previously called methods on `Container.Accessor` must now call `Container.Add`, `Container.Remove`, `Container.CanAdd`, and `Container.CanRemove` directly on the container.
- `SlotRule` abstract class has been removed. Subclasses of `SlotRule` used for slot constraints must be reworked; override `CanAdd`/`CanRemove` directly in a custom `ContainerAccessor` subclass instead.

### Added
- `ContainerChangedSignal` — bind and inject this optional signal to have item property mutations (e.g. name or amount changes) automatically invalidate and rebuild the filtered/sorted `ActiveSlots` view.
- `Container.Add`, `Container.Remove`, `Container.CanAdd`, `Container.CanRemove` — CRUD operations are now first-class members on `Container`, removing the need to hold a separate reference to `ContainerAccessor`.
- `Item`, `ItemName`, and `ItemAmount` now participate in the DI lifecycle (`Injectable`, `Initializable`, `Cleanable`), allowing them to wire up and tear down internal reactive subscriptions correctly.

### Fixed
- `FilterGroup.Remove()` was internally calling `Add` instead of `Remove`, so removing a filter from a group had no effect.
- `LayeredSorter.ThenBy()` was discarding previously accumulated sort steps, causing only the last-registered sorter to be applied.
- `Container.OnItemAdded()` crashed with a `NullReferenceException` when no `Filter` was injected.
