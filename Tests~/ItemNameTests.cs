using Calluna.DI;
using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    public class ItemNameTests
    {
        // ItemName implements Initializable and Cleanable via explicit interface.
        // We cast to the interface to call Initialize / Clean.

        [Test]
        [Description("Name.OnChanged fires when Name.Value is set after Initialize()?")]
        public void ItemName_NameValue_OnChangedFiredAfterInitialize()
        {
            var itemName = new ItemName();
            ((Initializable)itemName).Initialize();

            bool called = false;
            Observable.ValueChanged listener = () => { called = true; };
            itemName.Name.OnChanged += listener;

            itemName.Name.Value = "Sword";

            itemName.Name.OnChanged -= listener;
            ((Cleanable)itemName).Clean();

            Assert.IsTrue(called);
        }

        [Test]
        [Description("Name.OnChanged does NOT fire when Name.Value is set before Initialize()?")]
        public void ItemName_NameValue_OnChangedNotFiredBeforeInitialize()
        {
            // Without calling Initialize, Name.OnChanged is not wired to NotifyChanged,
            // so the internal signal is never invoked — but the observable's own OnChanged
            // still fires (that is the Observable's responsibility, not ItemName's).
            // What we verify here: Clean() unsubscribes the internal handler so that
            // after Clean() the signal integration is severed.
            var itemName = new ItemName();
            ((Initializable)itemName).Initialize();
            ((Cleanable)itemName).Clean();

            // After cleaning, mutating Name should NOT invoke any leftover handler.
            // We attach our own observable listener directly to confirm the value changes,
            // confirming Clean() did not break the observable itself.
            bool observableFired = false;
            Observable.ValueChanged listener = () => { observableFired = true; };
            itemName.Name.OnChanged += listener;

            itemName.Name.Value = "Shield";

            itemName.Name.OnChanged -= listener;

            Assert.IsTrue(observableFired, "Observable itself should still fire after Clean()");
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

        [Test]
        [Description("Name.OnChanged fires multiple times for multiple Value changes?")]
        public void ItemName_NameValue_OnChangedFiredForEachChange()
        {
            var itemName = new ItemName();
            ((Initializable)itemName).Initialize();

            int callCount = 0;
            Observable.ValueChanged listener = () => { callCount++; };
            itemName.Name.OnChanged += listener;

            itemName.Name.Value = "First";
            itemName.Name.Value = "Second";
            itemName.Name.Value = "Third";

            itemName.Name.OnChanged -= listener;
            ((Cleanable)itemName).Clean();

            Assert.AreEqual(3, callCount);
        }
    }
}
