using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    public class NameFilterTests
    {
        // ── helpers ──────────────────────────────────────────────────────────

        private static Item MakeNamedItem(string name)
        {
            var item = new Item();
            var itemName = new ItemName();
            itemName.Name.Value = name;
            item.Add(itemName);
            return item;
        }

        // ── ApplyTo(Item) ────────────────────────────────────────────────────

        [Test]
        [Description("ApplyTo(Item) => true when item name contains search text (exact match)?")]
        [TestCase("Sword", "Sword")]
        [TestCase("Iron Shield", "Shield")]
        [TestCase("Health Potion", "Potion")]
        public void NameFilter_ApplyTo_NameContainsSearch_ReturnsTrue(string itemName, string search)
        {
            var filter = new NameFilter();
            filter.SetSearchText(search);
            var item = MakeNamedItem(itemName);

            Assert.IsTrue(filter.ApplyTo(item));
        }

        [Test]
        [Description("ApplyTo(Item) => true when search is a different case (case-insensitive)?")]
        [TestCase("Sword", "sword")]
        [TestCase("SHIELD", "shield")]
        [TestCase("potion", "POTION")]
        public void NameFilter_ApplyTo_CaseInsensitive_ReturnsTrue(string itemName, string search)
        {
            var filter = new NameFilter();
            filter.SetSearchText(search);
            var item = MakeNamedItem(itemName);

            Assert.IsTrue(filter.ApplyTo(item));
        }

        [Test]
        [Description("ApplyTo(Item) => false when item name does not contain search text?")]
        [TestCase("Sword", "Potion")]
        [TestCase("Iron Shield", "Bow")]
        [TestCase("Helmet", "Gloves")]
        public void NameFilter_ApplyTo_NameDoesNotContainSearch_ReturnsFalse(string itemName, string search)
        {
            var filter = new NameFilter();
            filter.SetSearchText(search);
            var item = MakeNamedItem(itemName);

            Assert.IsFalse(filter.ApplyTo(item));
        }

        [Test]
        [Description("ApplyTo(Item) => false when item is null?")]
        public void NameFilter_ApplyTo_NullItem_ReturnsFalse()
        {
            var filter = new NameFilter();
            filter.SetSearchText("Sword");

            Assert.IsFalse(filter.ApplyTo((Item)null));
        }

        [Test]
        [Description("ApplyTo(Item) => false when item lacks ItemName property?")]
        public void NameFilter_ApplyTo_ItemWithoutItemName_ReturnsFalse()
        {
            var filter = new NameFilter();
            filter.SetSearchText("Sword");
            var item = new Item(); // no ItemName added

            Assert.IsFalse(filter.ApplyTo(item));
        }

        [Test]
        [Description("ApplyTo(Item) => true when search text is empty string (matches everything)?")]
        public void NameFilter_ApplyTo_EmptySearch_ReturnsTrueForNamedItem()
        {
            var filter = new NameFilter();
            filter.SetSearchText(string.Empty);
            var item = MakeNamedItem("Sword");

            Assert.IsTrue(filter.ApplyTo(item));
        }

        // ── SetSearchText ────────────────────────────────────────────────────

        [Test]
        [Description("SetSearchText() => OnChanged fires once?")]
        [TestCase("sword")]
        [TestCase("")]
        [TestCase("health potion")]
        public void NameFilter_SetSearchText_OnChangedFired(string text)
        {
            var filter = new NameFilter();
            int callCount = 0;
            System.Action listener = () => { callCount++; };
            filter.OnChanged += listener;

            filter.SetSearchText(text);

            filter.OnChanged -= listener;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("SetSearchText() updates the filter predicate for subsequent ApplyTo calls?")]
        public void NameFilter_SetSearchText_UpdatesFilter()
        {
            var filter = new NameFilter();
            var item = MakeNamedItem("Sword");

            filter.SetSearchText("Potion");
            Assert.IsFalse(filter.ApplyTo(item));

            filter.SetSearchText("Sword");
            Assert.IsTrue(filter.ApplyTo(item));
        }

        // ── IsActive integration ──────────────────────────────────────────────

        [Test]
        [Description("IsActive defaults to false?")]
        public void NameFilter_IsActive_DefaultIsFalse()
        {
            var filter = new NameFilter();

            Assert.IsFalse(filter.IsActive.Value);
        }

        [Test]
        [Description("IsActive can be toggled to true?")]
        public void NameFilter_IsActive_CanBeSetToTrue()
        {
            var filter = new NameFilter();

            filter.IsActive.Value = true;

            Assert.IsTrue(filter.IsActive.Value);
        }
    }
}
