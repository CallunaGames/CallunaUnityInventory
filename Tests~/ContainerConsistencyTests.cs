using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Calluna.DI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Random = System.Random;

namespace Calluna.Inventory.Tests
{
    /// <summary>
    /// Makes sure <see cref="Container.ActiveSlots"/> always is the filtered and sorted view of the
    /// slots, whatever happens to them - and that a subscriber which only applies the reported
    /// individual changes (like a scroll view does with its cells) always ends up with exactly the
    /// same list.
    /// </summary>
    public class ContainerConsistencyTests
    {
        private ObservableList<Slot> _slots;
        private Container _container;
        private NameFilter _filter;
        private NameSorter _sorter;
        private ContainerChangedSignal _signal;
        private TestResolver _itemResolver;
        private List<Slot> _mirror;
        private IDisposable _mirrorSubscription;
        private int _nextName;

        private static readonly string[] SearchTexts = { "1", "2", "a", "v1", "v2" };

        [TearDown]
        public void TearDown()
        {
            _mirrorSubscription?.Dispose();
            if (_container != null)
                ((Cleanable)_container).Clean();
        }

        // ── Random sequences ─────────────────────────────────────────────────────

        // Without a scheduler the container updates ActiveSlots right after every change, so the
        // check runs after every single change. A failure names the seed and step to reproduce it.
        [Test]
        public void RandomChanges_ActiveSlotsMatchSlotsAndReportedChanges(
            [Values(false, true)] bool withSorter, [Values(1, 2, 3, 4, 5)] int seed)
        {
            var random = new Random(seed);
            Build(withSorter, scheduler: null);
            AddItems(random.Next(0, 20));
            AssertConsistent("after setup");

            for (int step = 0; step < 400; step++)
            {
                string description = ApplyRandomChange(random);
                AssertConsistent($"seed {seed}, step {step}, sorter {withSorter}: after {description}");
            }
        }

        // With a scheduler, all changes of a frame end up in one update of ActiveSlots.
        [UnityTest]
        public IEnumerator RandomChangesBatchedPerFrame_ActiveSlotsMatchSlotsAndReportedChanges(
            [Values(false, true)] bool withSorter, [Values(1, 2, 3)] int seed)
        {
            var random = new Random(seed);
            var host = new GameObject("scheduler");
            try
            {
                Build(withSorter, host.AddComponent<UpdateScheduler>());
                AddItems(random.Next(0, 20));
                yield return null;
                AssertConsistent("after setup");

                for (int frame = 0; frame < 60; frame++)
                {
                    var descriptions = new List<string>();
                    int changes = random.Next(1, 8);
                    for (int i = 0; i < changes; i++)
                        descriptions.Add(ApplyRandomChange(random));
                    yield return null;
                    AssertConsistent($"seed {seed}, frame {frame}, sorter {withSorter}: after {string.Join(", ", descriptions)}");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        // ── Changes ──────────────────────────────────────────────────────────────

        private string ApplyRandomChange(Random random)
        {
            int count = _slots.Count;
            int kind = count == 0 ? 0 : random.Next(100);
            if (kind < 25)
            {
                _container.Add(NewItem());
                return "Add";
            }
            if (kind < 45)
            {
                Item item = _slots[random.Next(count)].Item.Value;
                _container.Remove(item);
                return $"Remove('{NameOf(item)}')";
            }
            if (kind < 60)
            {
                int index1 = random.Next(count);
                int index2 = random.Next(count);
                _slots.Swap(index1, index2);
                return $"Slots.Swap({index1}, {index2})";
            }
            if (kind < 75)
            {
                Item item = _slots[random.Next(count)].Item.Value;
                string former = NameOf(item);
                item.GetProperty<ItemName>().Name.Value = NewName();
                return $"Rename('{former}' -> '{NameOf(item)}')";
            }
            if (kind < 83)
            {
                string text = SearchTexts[random.Next(SearchTexts.Length)];
                _filter.SetSearchText(text);
                return $"SetSearchText('{text}')";
            }
            if (kind < 91)
            {
                _filter.IsActive.Value = !_filter.IsActive.Value;
                return $"Filter.IsActive = {_filter.IsActive.Value}";
            }
            if (kind < 97)
            {
                List<Slot> reordered = _slots.OrderBy(_ => random.Next()).ToList();
                _slots.OverrideWith(reordered);
                return "Slots.OverrideWith(shuffled)";
            }
            _slots.Clear();
            return "Slots.Clear";
        }

        // ── Checks ───────────────────────────────────────────────────────────────

        private void AssertConsistent(string when)
        {
            IEnumerable<Slot> filtered = _filter.IsActive.Value
                ? _slots.Where(slot => _filter.ApplyTo(slot.Item.Value))
                : _slots;
            // NameSorter orders by name with a stable sort, so equal names keep the slots' order.
            List<Slot> expected = (_sorter != null ? filtered.OrderBy(NameOf) : filtered).ToList();

            CollectionAssert.AreEqual(Names(expected), Names(_container.ActiveSlots),
                $"ActiveSlots must be the filtered and sorted slots ({when}).");
            CollectionAssert.AreEqual(expected, _container.ActiveSlots.ToList(),
                $"ActiveSlots must hold the slots themselves, in the same order ({when}).");
            CollectionAssert.AreEqual(_container.ActiveSlots.ToList(), _mirror,
                $"Applying the reported changes must reproduce ActiveSlots ({when}).");
        }

        // A subscriber that only knows the reported changes - checks each change against its copy.
        private void OnActiveSlotsChanged(ListChange<Slot> change)
        {
            switch (change.Kind)
            {
                case ListChangeKind.Added:
                    Assert.LessOrEqual(change.Index, _mirror.Count, "Added beyond the end.");
                    _mirror.Insert(change.Index, change.Item);
                    break;
                case ListChangeKind.Removed:
                    Assert.AreSame(_mirror[change.Index], change.Item, $"Removed item doesn't match index {change.Index}.");
                    _mirror.RemoveAt(change.Index);
                    break;
                case ListChangeKind.Replaced:
                    Assert.AreSame(_mirror[change.Index], change.FormerItem, $"Former item doesn't match index {change.Index}.");
                    _mirror[change.Index] = change.Item;
                    break;
                case ListChangeKind.Swapped:
                    Assert.AreSame(_mirror[change.Index], change.OtherItem, $"Swapped item doesn't match index {change.Index}.");
                    Assert.AreSame(_mirror[change.OtherIndex], change.Item, $"Swapped item doesn't match index {change.OtherIndex}.");
                    _mirror[change.Index] = change.Item;
                    _mirror[change.OtherIndex] = change.OtherItem;
                    break;
                case ListChangeKind.Reset:
                    _mirror.Clear();
                    _mirror.AddRange(_container.ActiveSlots);
                    break;
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void Build(bool withSorter, UpdateScheduler scheduler)
        {
            _nextName = 0;
            _slots = new ObservableList<Slot>();
            _filter = new NameFilter();
            _filter.SetSearchText("1");
            _sorter = withSorter ? new NameSorter() : null;
            _signal = new ContainerChangedSignal();
            _itemResolver = new TestResolver();
            _itemResolver.Bind(_signal);

            var resolver = new TestResolver();
            resolver.Bind(_slots);
            resolver.Bind<ReadonlyObservableList<Slot>>(_slots);
            resolver.Bind<ContainerAccessor>(new TestEndlessContainerAccessor(_slots, new SlotCreator()));
            resolver.Bind<Filter>(_filter);
            if (_sorter != null)
                resolver.Bind<Sorter>(_sorter);
            resolver.Bind(_signal);
            if (scheduler != null)
                resolver.Bind(scheduler);

            _container = new Container();
            ((Injectable)_container).Inject(resolver);
            ((Initializable)_container).Initialize();

            _mirror = _container.ActiveSlots.ToList();
            _mirrorSubscription = _container.ActiveSlots.Subscribe(OnActiveSlotsChanged);
        }

        private void AddItems(int count)
        {
            for (int i = 0; i < count; i++)
                _container.Add(NewItem());
        }

        // Injected with the signal, so a rename re-filters and re-sorts the container.
        private Item NewItem()
        {
            var item = new Item();
            var name = new ItemName();
            name.Name.Value = NewName();
            item.Add(name);
            ((Injectable)item).Inject(_itemResolver);
            return item;
        }

        // Few distinct names, so equal names (and the sort's tie order) come up often.
        private string NewName() => $"v{_nextName++ % 25}";

        private static string NameOf(Slot slot) => NameOf(slot.Item.Value);
        private static string NameOf(Item item) => item.GetProperty<ItemName>().Name.Value;
        private static List<string> Names(IEnumerable<Slot> slots) => slots.Select(NameOf).ToList();

        private sealed class TestResolver : Resolver
        {
            private readonly Dictionary<Type, object> _bindings = new();

            public void Bind<T>(T instance) => _bindings[typeof(T)] = instance;

            public TContract Resolve<TContract>()
            {
                if (_bindings.TryGetValue(typeof(TContract), out object value))
                    return (TContract)value;
                throw new InvalidOperationException($"No binding for {typeof(TContract).Name}");
            }

            public TContract ResolveOptional<TContract>() =>
                _bindings.TryGetValue(typeof(TContract), out object value) ? (TContract)value : default;

            public TContract Resolve<TContract>(IComparable iD) => throw new NotImplementedException();
            public TContract Resolve<TContract>(BindingKey bindingKey) => throw new NotImplementedException();
            public TContract ResolveOptional<TContract>(IComparable iD) => throw new NotImplementedException();
            public TContract ResolveOptional<TContract>(BindingKey bindingKey) => throw new NotImplementedException();
            public bool IsResolvable(BindingKey bindingKey) => throw new NotImplementedException();
        }
    }
}
