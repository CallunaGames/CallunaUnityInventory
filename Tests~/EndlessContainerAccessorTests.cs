using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    /// <summary>
    /// Minimal subclass that wires _slots and _slotProvider directly,
    /// bypassing the DI Resolver so tests have no DI dependency.
    /// </summary>
    internal class TestEndlessContainerAccessor : EndlessContainerAccessor
    {
        public TestEndlessContainerAccessor(ObservableList<Slot> slots, SlotProvider slotProvider)
        {
            _slots = slots;
            _slotProvider = slotProvider;
        }
    }

    public class EndlessContainerAccessorTests
    {
        private ObservableList<Slot> _slots;
        private SlotCreator _slotCreator;
        private TestEndlessContainerAccessor _accessor;

        [SetUp]
        public void SetUp()
        {
            _slots = new ObservableList<Slot>();
            _slotCreator = new SlotCreator();
            _accessor = new TestEndlessContainerAccessor(_slots, _slotCreator);
        }

        // ── CanAdd ───────────────────────────────────────────────────────────

        [Test]
        [Description("CanAdd() => always returns true regardless of item?")]
        public void EndlessContainerAccessor_CanAdd_AlwaysReturnsTrue()
        {
            Assert.IsTrue(_accessor.CanAdd(new Item()));
            Assert.IsTrue(_accessor.CanAdd(null));
        }

        // ── CanRemove ────────────────────────────────────────────────────────

        [Test]
        [Description("CanRemove() => true when item is present in slots?")]
        public void EndlessContainerAccessor_CanRemove_ItemPresent_ReturnsTrue()
        {
            var item = new Item();
            _accessor.Add(item);

            Assert.IsTrue(_accessor.CanRemove(item));
        }

        [Test]
        [Description("CanRemove() => false when item is not in slots?")]
        public void EndlessContainerAccessor_CanRemove_ItemAbsent_ReturnsFalse()
        {
            Assert.IsFalse(_accessor.CanRemove(new Item()));
        }

        // ── Add ──────────────────────────────────────────────────────────────

        [Test]
        [Description("Add() => slot count increases by one?")]
        public void EndlessContainerAccessor_Add_SlotCountIncreases()
        {
            _accessor.Add(new Item());

            Assert.AreEqual(1, _slots.Count);
        }

        [Test]
        [Description("Add() => added item is retrievable from the slot?")]
        public void EndlessContainerAccessor_Add_ItemStoredInSlot()
        {
            var item = new Item();
            _accessor.Add(item);

            Assert.AreSame(item, _slots[0].Item.Value);
        }

        [Test]
        [Description("Add() multiple items => all items present in slots?")]
        public void EndlessContainerAccessor_Add_MultipleItems_AllPresent()
        {
            var a = new Item();
            var b = new Item();
            var c = new Item();

            _accessor.Add(a);
            _accessor.Add(b);
            _accessor.Add(c);

            Assert.AreEqual(3, _slots.Count);
        }

        // ── Remove ───────────────────────────────────────────────────────────

        [Test]
        [Description("Remove() => slot count decreases after removal?")]
        public void EndlessContainerAccessor_Remove_SlotCountDecreases()
        {
            var item = new Item();
            _accessor.Add(item);

            _accessor.Remove(item);

            Assert.AreEqual(0, _slots.Count);
        }

        [Test]
        [Description("Remove() => item is no longer present after removal?")]
        public void EndlessContainerAccessor_Remove_ItemNoLongerPresent()
        {
            var item = new Item();
            _accessor.Add(item);

            _accessor.Remove(item);

            Assert.IsFalse(_accessor.CanRemove(item));
        }

        [Test]
        [Description("Remove() => only target item is removed; others remain?")]
        public void EndlessContainerAccessor_Remove_OnlyTargetRemoved()
        {
            var a = new Item();
            var b = new Item();
            _accessor.Add(a);
            _accessor.Add(b);

            _accessor.Remove(a);

            Assert.IsFalse(_accessor.CanRemove(a));
            Assert.IsTrue(_accessor.CanRemove(b));
        }

        // ── CanSetAt ─────────────────────────────────────────────────────────

        [Test]
        [Description("CanSetAt() => true when index is within bounds?")]
        public void EndlessContainerAccessor_CanSetAt_InBounds_ReturnsTrue()
        {
            _accessor.Add(new Item());
            _accessor.Add(new Item());

            Assert.IsTrue(_accessor.CanSetAt(new Item(), 0));
            Assert.IsTrue(_accessor.CanSetAt(new Item(), 1));
        }

        [Test]
        [Description("CanSetAt() => false when index equals or exceeds slot count?")]
        public void EndlessContainerAccessor_CanSetAt_OutOfBounds_ReturnsFalse()
        {
            _accessor.Add(new Item());

            Assert.IsFalse(_accessor.CanSetAt(new Item(), 1));
            Assert.IsFalse(_accessor.CanSetAt(new Item(), 5));
        }

        [Test]
        [Description("CanSetAt() => false when slots are empty?")]
        public void EndlessContainerAccessor_CanSetAt_EmptySlots_ReturnsFalse()
        {
            Assert.IsFalse(_accessor.CanSetAt(new Item(), 0));
        }

        // ── Indexer ──────────────────────────────────────────────────────────

        [Test]
        [Description("Indexer get => returns the item at the given index?")]
        public void EndlessContainerAccessor_Indexer_Get_ReturnsItemAtIndex()
        {
            var item = new Item();
            _accessor.Add(item);

            Assert.AreSame(item, _accessor[0]);
        }

        [Test]
        [Description("Indexer set => replaces the item at the given index?")]
        public void EndlessContainerAccessor_Indexer_Set_ReplacesItem()
        {
            var original = new Item();
            var replacement = new Item();
            _accessor.Add(original);

            _accessor[0] = replacement;

            Assert.AreSame(replacement, _accessor[0]);
        }
    }
}
