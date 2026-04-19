using System;
using System.Collections.Generic;
using Calluna.DI;
using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    public class ContainerTests
    {
        // ── TestResolver ─────────────────────────────────────────────────────────

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

            public TContract ResolveOptional<TContract>()
            {
                return _bindings.TryGetValue(typeof(TContract), out object value)
                    ? (TContract)value
                    : default;
            }

            public TContract Resolve<TContract>(IComparable iD) => throw new NotImplementedException();
            public TContract Resolve<TContract>(BindingKey bindingKey) => throw new NotImplementedException();
            public TContract ResolveOptional<TContract>(IComparable iD) => throw new NotImplementedException();
            public TContract ResolveOptional<TContract>(BindingKey bindingKey) => throw new NotImplementedException();
            public bool IsResolvable(BindingKey bindingKey) => throw new NotImplementedException();
        }

        // ── StubFilter ───────────────────────────────────────────────────────────

        private sealed class StubFilter : Filter
        {
            private bool _passes = true;

            public void SetPasses(bool passes)
            {
                _passes = passes;
                InvokeOnChanged();
            }

            public override bool ApplyTo(Item item) => _passes;
        }

        // ── helpers ──────────────────────────────────────────────────────────────

        private static Container BuildContainer(Filter filter = null, Sorter sorter = null)
        {
            var slots = new ObservableList<Slot>();
            var accessor = new TestEndlessContainerAccessor(slots, new SlotCreator());

            var resolver = new TestResolver();
            resolver.Bind<ObservableList<Slot>>(slots);
            resolver.Bind<ReadonlyObservableList<Slot>>(slots);
            resolver.Bind<ContainerAccessor>(accessor);
            if (filter != null) resolver.Bind<Filter>(filter);
            if (sorter != null) resolver.Bind<Sorter>(sorter);

            var container = new Container();
            ((Injectable)container).Inject(resolver);
            ((Initializable)container).Initialize();
            return container;
        }

        private static Item MakeNamedItem(string name)
        {
            var item = new Item();
            var itemName = new ItemName();
            itemName.Name.Value = name;
            item.Add(itemName);
            return item;
        }

        private static string GetItemName(Slot slot)
        {
            slot.Item.Value.TryGetProperty(out ItemName name);
            return name?.Name.Value;
        }

        // ── Add ──────────────────────────────────────────────────────────────────

        [Test]
        public void Container_Add_ItemAppearsInSlotsAndActiveSlots()
        {
            Container container = BuildContainer();
            Item item = MakeNamedItem("A");

            container.Add(item);

            Assert.AreEqual(1, container.Slots.Count);
            Assert.AreEqual(1, container.ActiveSlots.Count);
            Assert.AreSame(item, container.Slots[0].Item.Value);
        }

        [Test]
        public void Container_Add_MultipleItems_AllPresentInActiveSlots()
        {
            Container container = BuildContainer();

            container.Add(MakeNamedItem("A"));
            container.Add(MakeNamedItem("B"));
            container.Add(MakeNamedItem("C"));

            Assert.AreEqual(3, container.ActiveSlots.Count);
        }

        // ── Remove ───────────────────────────────────────────────────────────────

        [Test]
        public void Container_Remove_ItemRemovedFromSlotsAndActiveSlots()
        {
            Container container = BuildContainer();
            Item item = MakeNamedItem("A");
            container.Add(item);

            container.Remove(item);

            Assert.AreEqual(0, container.Slots.Count);
            Assert.AreEqual(0, container.ActiveSlots.Count);
        }

        [Test]
        public void Container_Remove_OnlyTargetRemovedOtherItemsRemain()
        {
            Container container = BuildContainer();
            Item a = MakeNamedItem("A");
            Item b = MakeNamedItem("B");
            container.Add(a);
            container.Add(b);

            container.Remove(a);

            Assert.AreEqual(1, container.ActiveSlots.Count);
            Assert.AreSame(b, container.ActiveSlots[0].Item.Value);
        }

        // ── CanAdd / CanRemove ───────────────────────────────────────────────────

        [Test]
        public void Container_CanAdd_ReturnsTrue()
        {
            Container container = BuildContainer();

            Assert.IsTrue(container.CanAdd(new Item()));
        }

        [Test]
        public void Container_CanRemove_ReturnsFalse_WhenItemNotInContainer()
        {
            Container container = BuildContainer();

            Assert.IsFalse(container.CanRemove(new Item()));
        }

        [Test]
        public void Container_CanRemove_ReturnsTrue_WhenItemInContainer()
        {
            Container container = BuildContainer();
            Item item = MakeNamedItem("A");
            container.Add(item);

            Assert.IsTrue(container.CanRemove(item));
        }

        // ── Filtering ────────────────────────────────────────────────────────────

        [Test]
        public void Container_ActiveSlots_ExcludesItems_WhenFilterIsActive()
        {
            var filter = new StubFilter();
            Container container = BuildContainer(filter: filter);
            container.Add(MakeNamedItem("A"));
            container.Add(MakeNamedItem("B"));

            filter.IsActive.Value = true;
            filter.SetPasses(false);

            Assert.AreEqual(0, container.ActiveSlots.Count);
            Assert.AreEqual(2, container.Slots.Count);
        }

        [Test]
        public void Container_ActiveSlots_RestoresItems_WhenFilterDeactivated()
        {
            var filter = new StubFilter();
            Container container = BuildContainer(filter: filter);
            container.Add(MakeNamedItem("A"));

            filter.IsActive.Value = true;
            filter.SetPasses(false);
            Assert.AreEqual(0, container.ActiveSlots.Count);

            filter.IsActive.Value = false;
            filter.SetPasses(false);

            Assert.AreEqual(1, container.ActiveSlots.Count);
        }

        [Test]
        public void Container_ActiveSlots_OnlyIncludesMatchingItems_WhenFilterPartiallyMatches()
        {
            var filter = new NameFilter();
            Container container = BuildContainer(filter: filter);
            container.Add(MakeNamedItem("Apple"));
            container.Add(MakeNamedItem("Banana"));
            container.Add(MakeNamedItem("Apricot"));

            filter.IsActive.Value = true;
            filter.SetSearchText("ap");

            Assert.AreEqual(2, container.ActiveSlots.Count);
        }

        // ── Sorting ──────────────────────────────────────────────────────────────

        [Test]
        public void Container_ActiveSlots_ReflectsSortOrder_WhenSorterActive()
        {
            var sorter = new NameSorter();
            Container container = BuildContainer(sorter: sorter);

            container.Add(MakeNamedItem("Charlie"));
            container.Add(MakeNamedItem("Alice"));
            container.Add(MakeNamedItem("Bob"));

            Assert.AreEqual("Alice",   GetItemName(container.ActiveSlots[0]));
            Assert.AreEqual("Bob",     GetItemName(container.ActiveSlots[1]));
            Assert.AreEqual("Charlie", GetItemName(container.ActiveSlots[2]));
        }

        // ── Clean ────────────────────────────────────────────────────────────────

        [Test]
        public void Container_Clean_StopsRespondingToFilterChanges()
        {
            var filter = new StubFilter();
            Container container = BuildContainer(filter: filter);
            container.Add(MakeNamedItem("A"));

            ((Cleanable)container).Clean();
            filter.IsActive.Value = true;
            filter.SetPasses(false);

            Assert.AreEqual(1, container.ActiveSlots.Count);
        }

        [Test]
        public void Container_ActiveSlots_EmptyOnStart()
        {
            Container container = BuildContainer();

            Assert.AreEqual(0, container.ActiveSlots.Count);
            Assert.AreEqual(0, container.Slots.Count);
        }
    }
}
