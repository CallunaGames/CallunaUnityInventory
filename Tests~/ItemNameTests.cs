using Calluna.DI;
using Moq;
using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    public class ItemNameTests
    {
        private static (Item item, ContainerChangedSignal signal) ItemWithSignal()
        {
            var signal = new ContainerChangedSignal();
            var resolver = new Mock<Resolver>();
            resolver.Setup(r => r.ResolveOptional<ContainerChangedSignal>()).Returns(signal);
            var item = new Item();
            ((Injectable)item).Inject(resolver.Object);
            return (item, signal);
        }

        [Test]
        [Description("Name changed => Signal raised, without any lifecycle call?")]
        public void ItemName_NameChanged_RaisesSignalWithoutLifecycle()
        {
            (Item item, ContainerChangedSignal signal) = ItemWithSignal();
            var itemName = new ItemName();
            item.Add(itemName);
            int raised = 0;
            signal.OnChanged += () => raised++;

            itemName.Name.Value = "Sword";
            itemName.Name.Value = "Shield";

            Assert.AreEqual(2, raised);
        }

        [Test]
        [Description("Name set to its current value => Signal not raised?")]
        public void ItemName_NameUnchanged_DoesNotRaiseSignal()
        {
            (Item item, ContainerChangedSignal signal) = ItemWithSignal();
            var itemName = new ItemName();
            itemName.Name.Value = "Sword";
            item.Add(itemName);
            int raised = 0;
            signal.OnChanged += () => raised++;

            itemName.Name.Value = "Sword";

            Assert.AreEqual(0, raised);
        }

        [Test]
        [Description("Name changed after the property was removed => Signal not raised?")]
        public void ItemName_RemovedFromItem_NoLongerRaisesSignal()
        {
            (Item item, ContainerChangedSignal signal) = ItemWithSignal();
            var itemName = new ItemName();
            item.Add(itemName);
            item.Remove<ItemName>();
            int raised = 0;
            signal.OnChanged += () => raised++;

            itemName.Name.Value = "Sword";

            Assert.AreEqual(0, raised);
        }

        [Test]
        [Description("Name changed on an item without signal => No exception?")]
        public void ItemName_WithoutSignal_ChangeDoesNotThrow()
        {
            var item = new Item();
            var itemName = new ItemName();
            item.Add(itemName);

            Assert.DoesNotThrow(() => itemName.Name.Value = "Sword");
        }

        [Test]
        [Description("Name property => Value is stored and retrievable?")]
        [TestCase("Sword")]
        [TestCase("Potion")]
        [TestCase("")]
        public void ItemName_NameValue_StoredAndRetrievable(string name)
        {
            var itemName = new ItemName();
            itemName.Name.Value = name;

            Assert.AreEqual(name, itemName.Name.Value);
        }
    }
}
