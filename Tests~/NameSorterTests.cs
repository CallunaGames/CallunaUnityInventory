using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    public class NameSorterTests
    {
        // ── helpers ──────────────────────────────────────────────────────────

        private static Slot MakeNamedSlot(string name)
        {
            var item = new Item();
            var itemName = new ItemName();
            itemName.Name.Value = name;
            item.Add(itemName);
            var slot = new Slot();
            slot.Item.Value = item;
            return slot;
        }

        private static Slot MakeUnnamedSlot()
        {
            var slot = new Slot();
            slot.Item.Value = new Item(); // item without ItemName
            return slot;
        }

        private static Slot MakeEmptySlot()
        {
            return new Slot(); // no item set at all
        }

        // ── Sort ─────────────────────────────────────────────────────────────

        [Test]
        [Description("Sort() => slots ordered alphabetically ascending by name?")]
        public void NameSorter_Sort_OrderedAlphabetically()
        {
            var sorter = new NameSorter();
            var slots = new List<Slot>
            {
                MakeNamedSlot("Sword"),
                MakeNamedSlot("Axe"),
                MakeNamedSlot("Potion"),
            };

            IOrderedEnumerable<Slot> result = sorter.Sort(slots);
            List<string> names = result
                .Select(s => s.Item.Value.TryGetProperty(out ItemName n) ? n.Name.Value : string.Empty)
                .ToList();

            Assert.AreEqual("Axe", names[0]);
            Assert.AreEqual("Potion", names[1]);
            Assert.AreEqual("Sword", names[2]);
        }

        [Test]
        [Description("Sort() => unnamed slots (item has no ItemName) sort before named slots?")]
        public void NameSorter_Sort_UnnamedSlotsSortBeforeNamed()
        {
            var sorter = new NameSorter();
            var unnamed = MakeUnnamedSlot();
            var named = MakeNamedSlot("Axe");
            var slots = new List<Slot> { named, unnamed };

            List<Slot> result = sorter.Sort(slots).ToList();

            Assert.AreSame(unnamed, result[0]);
            Assert.AreSame(named, result[1]);
        }

        [Test]
        [Description("Sort() => empty slots (no item) sort before named slots?")]
        public void NameSorter_Sort_EmptySlotsSortBeforeNamed()
        {
            var sorter = new NameSorter();
            var empty = MakeEmptySlot();
            var named = MakeNamedSlot("Sword");
            var slots = new List<Slot> { named, empty };

            List<Slot> result = sorter.Sort(slots).ToList();

            Assert.AreSame(empty, result[0]);
            Assert.AreSame(named, result[1]);
        }

        [Test]
        [Description("Sort() with single slot => returns that single slot?")]
        public void NameSorter_Sort_SingleSlot_ReturnsThatSlot()
        {
            var sorter = new NameSorter();
            var slot = MakeNamedSlot("Helmet");
            var slots = new List<Slot> { slot };

            List<Slot> result = sorter.Sort(slots).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreSame(slot, result[0]);
        }

        // ── ThenBy ───────────────────────────────────────────────────────────

        [Test]
        [Description("ThenBy() => applies secondary sort by name when primary keys are equal?")]
        public void NameSorter_ThenBy_AppliesSecondarySortByName()
        {
            var sorter = new NameSorter();
            // Primary sort: all slots have null item (same key), secondary is name
            var swordSlot = MakeNamedSlot("Sword");
            var axeSlot = MakeNamedSlot("Axe");
            var slots = new List<Slot> { swordSlot, axeSlot };

            // Sort first with no-op primary key, then by name via ThenBy
            IOrderedEnumerable<Slot> primary = slots.OrderBy(_ => 0);
            IOrderedEnumerable<Slot> result = sorter.ThenBy(primary);

            List<Slot> ordered = result.ToList();
            Assert.AreSame(axeSlot, ordered[0]);
            Assert.AreSame(swordSlot, ordered[1]);
        }

        // ── IsActive ─────────────────────────────────────────────────────────

        [Test]
        [Description("IsActive => always true for NameSorter (base default)?")]
        public void NameSorter_IsActive_DefaultIsTrue()
        {
            var sorter = new NameSorter();

            Assert.IsTrue(sorter.IsActive);
        }
    }
}
