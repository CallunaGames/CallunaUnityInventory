using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    public class SlotTests
    {
        [Test]
        [Description("Item property => reflects value assigned to it?")]
        public void Slot_Item_ReflectsAssignedValue()
        {
            var slot = new Slot();
            var item = new Item();

            slot.Item.Value = item;

            Assert.AreSame(item, slot.Item.Value);
        }

        [Test]
        [Description("Item property => default value is null?")]
        public void Slot_Item_DefaultValueIsNull()
        {
            var slot = new Slot();

            Assert.IsNull(slot.Item.Value);
        }

        [Test]
        [Description("Item.OnChanged fires when Item.Value is assigned?")]
        public void Slot_Item_OnChangedFiredOnAssignment()
        {
            var slot = new Slot();
            bool called = false;
            Observable.ValueChanged listener = () => { called = true; };
            slot.Item.OnChanged += listener;

            slot.Item.Value = new Item();

            slot.Item.OnChanged -= listener;

            Assert.IsTrue(called);
        }

        [Test]
        [Description("Item.HasValue => false when no item assigned, true after assignment?")]
        public void Slot_Item_HasValue_FalseBeforeAssignment_TrueAfter()
        {
            var slot = new Slot();

            Assert.IsFalse(slot.Item.HasValue);

            slot.Item.Value = new Item();

            Assert.IsTrue(slot.Item.HasValue);
        }

        [Test]
        [Description("Item.Value can be overwritten with a different item?")]
        public void Slot_Item_CanBeReassigned()
        {
            var slot = new Slot();
            var first = new Item();
            var second = new Item();

            slot.Item.Value = first;
            slot.Item.Value = second;

            Assert.AreSame(second, slot.Item.Value);
        }
    }
}
