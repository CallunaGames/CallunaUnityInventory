# Calluna Unity Inventory

A generic, DI-aware inventory system for Unity. Items are property bags; filtering and sorting are pluggable strategies; `Container` exposes a reactive, filtered and sorted view of its slots.

---

## Item

`Item` is a property bag. All item data is stored as typed `ItemProperty` subclasses. `Item` participates in the DI lifecycle (`Injectable`) so that injected `ContainerChangedSignal` is propagated to every property.

```
Item : Injectable
└─ Dictionary<Type, ItemProperty>
   ├─ ItemName        : ItemProperty, Initializable, Cleanable  (built-in)
   └─ (your custom ItemProperty subclasses...)
```

**Usage**

```csharp
// Build an item
var item = new Item();
item.Add(new ItemName { Name = { Value = "Iron Sword" } });

// Read a property — two options:
// Use GetProperty when the property is always expected to be present.
// Throws KeyNotFoundException if the type is absent.
ItemName itemName = item.GetProperty<ItemName>();
Debug.Log(itemName.Name.Value);

// Use TryGetProperty when absence is a valid state (no exception, no out-variable allocation).
if (item.TryGetProperty(out ItemName found))
    Debug.Log(found.Name.Value);

// Remove a property
item.Remove<ItemName>();
```

### ItemProperty

`ItemProperty` is the abstract base for all item data. Subclass it to add custom data. Call `NotifyChanged()` inside your subclass whenever a value changes so the container's `ActiveSlots` stays up to date. `NotifyChanged()` is `protected virtual`.

```csharp
public class ItemAmount : ItemProperty, Initializable, Cleanable
{
    public Observable<int> Amount { get; } = new Observable<int>();

    void Initializable.Initialize() => Amount.OnChanged += NotifyChanged;
    void Cleanable.Clean()          => Amount.OnChanged -= NotifyChanged;
}
```

`Initializable` and `Cleanable` are optional on custom properties — implement them only when you need to subscribe/unsubscribe observables or perform setup and teardown.

### ItemName

`ItemName : ItemProperty, Initializable, Cleanable` — built-in property that stores a display name.

| Member | Type | Notes |
|---|---|---|
| `Name` | `Observable<string>` | Fires `NotifyChanged` on every change |

---

## Container

`Container : Injectable, Initializable, Cleanable` is the central class. It maintains the raw slot list and rebuilds a filtered + sorted projection automatically.

| Member | Type | Notes |
|---|---|---|
| `Slots` | `ReadonlyObservableList<Slot>` | All slots, unfiltered and unsorted |
| `ActiveSlots` | `ReadonlyObservableList<Slot>` | Filtered + sorted projection, rebuilt reactively |
| `CanAdd(item)` | `bool` | Delegates to the injected `ContainerAccessor` |
| `CanRemove(item)` | `bool` | Delegates to the injected `ContainerAccessor` |
| `Add(item)` | `void` | Delegates to the injected `ContainerAccessor` |
| `Remove(item)` | `void` | Delegates to the injected `ContainerAccessor` |

`ActiveSlots` is rebuilt whenever `Filter.OnChanged`, `Sorter.OnChanged`, `ContainerChangedSignal.OnChanged`, or the raw `Slots` list changes.

When `Filter.OnChanged` or `Sorter.OnChanged` fires, the rebuild is routed through `ScheduleActiveSlotsUpdate`. If an `UpdateScheduler` (from `com.calluna.core`) is injected, the rebuild is deferred to end-of-frame via `UpdateScheduler.ScheduleOnce`. Multiple filter or sorter changes within the same frame are therefore collapsed into a single `ActiveSlots` rebuild. If no `UpdateScheduler` is bound, the rebuild happens immediately and synchronously, preserving the original behaviour.

**Usage**

```csharp
// Expose the readonly view to UI code.
// ActiveSlots is rebuilt via OverrideWith, which fires OnContentsReplaced once
// rather than per-item events. React to OnContentsReplaced and rebuild the whole view.
ReadonlyObservableList<Slot> view = container.ActiveSlots;
view.OnContentsReplaced += () =>
{
    DespawnAllSlotViews();
    foreach (Slot slot in view)
        SpawnSlotView(slot);
};

// Add / remove items
if (container.CanAdd(item))
    container.Add(item);

if (container.CanRemove(item))
    container.Remove(item);
```

---

## ContainerChangedSignal

`ContainerChangedSignal` is a lightweight signal object that item properties use to notify the container of in-place mutations (e.g. an amount change on an already-slotted item). Bind it as a singleton in the same DI context as the container.

| Member | Type |
|---|---|
| `OnChanged` | `event Action` |

When an `ItemProperty` calls `NotifyChanged()`, the signal fires and `Container` rebuilds `ActiveSlots`.

**DI binding**

```csharp
binder.BindToNewSelf<ContainerChangedSignal>().AsSingle();
```

---

## ContainerAccessor

`ContainerAccessor : Injectable` is the abstract CRUD layer between `Container` and the raw slot list. Subclass it to implement custom capacity rules or item stacking.

| Member | Notes |
|---|---|
| `CanAdd(item)` | Return `false` to reject an add |
| `CanRemove(item)` | Return `false` to reject a remove |
| `CanSetAt(item, index)` | Return `false` to reject a direct set |
| `Add(item)` | Insert item into the slot list |
| `Remove(item)` | Remove item from the slot list |
| `this[int index]` | Get or set the item at a slot index |

### EndlessContainerAccessor

`EndlessContainerAccessor : ContainerAccessor` — built-in implementation with no capacity limit. `CanAdd` always returns `true`; `CanRemove` returns `true` when the item is present. Slots are allocated and recycled via the injected `SlotProvider`.

---

## Slot and SlotProvider

`Slot` wraps one `Item` reference in an `Observable<Item>`.

| Member | Type |
|---|---|
| `Item` | `Observable<Item>` |

`SlotProvider` is an abstract factory for `Slot` instances. Subclass it to implement object pooling.

| Member | Notes |
|---|---|
| `Get()` | Return a slot ready for use |
| `Return(slot)` | Reclaim a slot after removal |

### SlotCreator

`SlotCreator : SlotProvider` — built-in implementation that allocates a new `Slot` on every `Get()` and discards it on `Return()`.

---

## Filter

`Filter : Injectable, Initializable, Cleanable` — abstract base for item predicates. The container resolves `Filter` optionally; the inventory works without one.

| Member | Type | Notes |
|---|---|---|
| `IsActive` | `Observable<bool>` | Starts `false`; toggling triggers `OnChanged` indirectly via `FilterGroup` |
| `OnChanged` | `event Action` | Raise with `InvokeOnChanged()` |
| `ApplyTo(item)` | `abstract bool` | Implement your predicate here |

**Usage**

```csharp
public class RarityFilter : Filter
{
    public Rarity Required { get; set; }

    public override bool ApplyTo(Item item)
    {
        return item.TryGetProperty(out ItemRarity r) && r.Value == Required;
    }
}

// Activate / deactivate
rarityFilter.IsActive.Value = true;
```

### NameFilter

`NameFilter : Filter` — built-in filter. Performs a case-insensitive substring match against `ItemName`.

| Member | Notes |
|---|---|
| `SetSearchText(text)` | Updates the search string and raises `OnChanged` |

### FilterGroup

`FilterGroup : Filter` — composes multiple filters with AND logic. Only active filters participate.

| Member | Notes |
|---|---|
| `FilterGroup()` | Empty group |
| `FilterGroup(IEnumerable<Filter>)` | Pre-populate filters |
| `Add(filter)` | Add a filter and raise `OnChanged` |
| `Remove(filter)` | Remove a filter and raise `OnChanged` |

---

## Sorter

`Sorter : Injectable, Initializable, Cleanable` — abstract base for slot ordering. The container resolves `Sorter` optionally.

| Member | Type | Notes |
|---|---|---|
| `IsActive` | `virtual bool` | Returns `true` by default |
| `OnChanged` | `event Action` | Raise with `InvokeOnChanged()` |
| `Sort(slots)` | `abstract IOrderedEnumerable<Slot>` | Produce the initial ordering |
| `ThenBy(ordered)` | `virtual IOrderedEnumerable<Slot>` | Default pass-through; override to add a secondary key |

`Sorter<TKey> : Sorter` — convenience generic base that implements `Sort` and `ThenBy` via LINQ `OrderBy`/`ThenBy`. Implement a single `GetKey(Slot)` method.

```csharp
public class ItemAmountSorter : Sorter<int>
{
    protected override int GetKey(Slot slot) =>
        slot.Item.HasValue && slot.Item.Value.TryGetProperty(out ItemAmount a)
            ? a.Amount.Value
            : 0;
}
```

### NameSorter

`NameSorter : Sorter<string>` — built-in sorter. Orders slots alphabetically by `ItemName`. Slots with no name sort first.

### LayeredSorter

`LayeredSorter : Sorter` — chains child sorters sequentially (first sorter's `Sort`, then each subsequent sorter's `ThenBy`). Child sorters are resolved from the DI context as `IEnumerable<Sorter>` at inject time; additional sorters can be added at runtime. `IsActive` returns `true` only when at least one child sorter is active.

| Member | Notes |
|---|---|
| `Add(sorter)` | Append a sorter and raise `OnChanged` |
| `Remove(sorter)` | Remove a sorter and raise `OnChanged` |
| `SetSorters(list)` | Replace all sorters at once and raise `OnChanged` |

### RadioSorter

`RadioSorter : Sorter` — at most one child sorter is active at a time. Child sorters are resolved from the DI context as `IEnumerable<Sorter>` at inject time; additional sorters can be added at runtime. `IsActive` is `false` until `Activate` is called.

| Member | Notes |
|---|---|
| `Add(sorter, active)` | Register a sorter; pass `active: true` to select it immediately |
| `Remove(sorter)` | Unregister; deactivates automatically if it was the active one |
| `Activate(sorter)` | Make the given sorter the sole active one |
| `Deactivate()` | Clear the active selection; `IsActive` becomes `false` |

---

## DI wiring

Bind these types in the same DI context (typically a `SceneContext`). `Filter` and `Sorter` are both optional.

```csharp
// Required
// The slot list must be bound as both its readonly interface (used by Container)
// and its concrete type (used by ContainerAccessor).
var slots = new ObservableList<Slot>();
binder.Bind<ReadonlyObservableList<Slot>>().And<ObservableList<Slot>>().ToInstance(slots);

binder.BindToNewSelf<Container>().AsSingle();
binder.Bind<ContainerAccessor>().ToNew<EndlessContainerAccessor>().AsSingle();
binder.Bind<SlotProvider>().ToNew<SlotCreator>().AsSingle();

// Optional — reactive in-place mutations
binder.BindToNewSelf<ContainerChangedSignal>().AsSingle();

// Optional — defer filter/sorter-triggered rebuilds to end-of-frame,
// batching multiple changes in one frame into a single ActiveSlots rebuild.
// UpdateScheduler is a MonoBehaviour; attach it to a GameObject and bind the instance.
binder.BindToSelf<UpdateScheduler>().FromInstance(updateSchedulerInstance).AsSingle();

// Optional — filtering
binder.Bind<Filter>().And<NameFilter>().ToNew<NameFilter>().AsSingle();

// Optional — sorting (choose one or compose)
binder.BindToNewSelf<NameSorter>().AsSingle();
binder.Bind<Sorter>().ToNew<LayeredSorter>().AsSingle();
// or
binder.Bind<Sorter>().ToNew<RadioSorter>().AsSingle();
```

---

## Extension points

- **Custom item properties** — subclass `ItemProperty`; call `NotifyChanged()` when values change; implement `Initializable`/`Cleanable` to wire observable subscriptions.
- **Custom filters** — subclass `Filter`, implement `ApplyTo(Item)`.
- **Custom sorters** — subclass `Sorter<TKey>`, implement `GetKey(Slot)`.
- **Slot capacity rules** — subclass `ContainerAccessor` and override `CanAdd`/`CanRemove`/`CanSetAt`.
- **Slot pooling** — subclass `SlotProvider` instead of using `SlotCreator`.

---

## Samples

### Endless Inventory

Demonstrates a complete inventory setup with a `NameFilter` for live search and a `RadioSorter` wired inside a `LayeredSorter` for sorting. The `RadioSorter` provides mutually exclusive toggle behaviour across three child sorters — `NameSorter`, `IdSorter`, and `NameCharacterCountSorter` — so only one sort key is active at a time. Import via **Package Manager > Calluna Unity Inventory > Samples > Endless Inventory**.
