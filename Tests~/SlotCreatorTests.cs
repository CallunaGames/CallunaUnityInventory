using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    public class SlotCreatorTests
    {
        [Test]
        [Description("Get() => returns a non-null Slot?")]
        public void SlotCreator_Get_ReturnsNonNullSlot()
        {
            var creator = new SlotCreator();

            Slot slot = creator.Get();

            Assert.IsNotNull(slot);
        }

        [Test]
        [Description("Get() => returns a new distinct Slot on each call?")]
        public void SlotCreator_Get_ReturnsNewSlotEachCall()
        {
            var creator = new SlotCreator();

            Slot first = creator.Get();
            Slot second = creator.Get();

            Assert.AreNotSame(first, second);
        }

        [Test]
        [Description("Return() => does not throw for a valid slot?")]
        public void SlotCreator_Return_DoesNotThrow()
        {
            var creator = new SlotCreator();
            Slot slot = creator.Get();

            Assert.DoesNotThrow(() => creator.Return(slot));
        }

        [Test]
        [Description("Return() => does not throw for a null slot?")]
        public void SlotCreator_Return_DoesNotThrowForNull()
        {
            var creator = new SlotCreator();

            Assert.DoesNotThrow(() => creator.Return(null));
        }

        [Test]
        [Description("Get() => returned slot has null Item.Value by default?")]
        public void SlotCreator_Get_ReturnedSlotHasNullItem()
        {
            var creator = new SlotCreator();

            Slot slot = creator.Get();

            Assert.IsNull(slot.Item.Value);
        }
    }
}
