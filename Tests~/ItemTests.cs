using System;
using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    public class ItemTests
    {
        // ── helpers ──────────────────────────────────────────────────────────

        private sealed class NameProperty : ItemProperty { }
        private sealed class AmountProperty : ItemProperty { }

        // ── TryGetProperty ───────────────────────────────────────────────────

        [Test]
        [Description("TryGetProperty<T> when property present => returns true and out-param is the instance?")]
        public void Item_TryGetProperty_Present_ReturnsTrueAndProperty()
        {
            var item = new Item();
            var prop = new NameProperty();
            item.Add(prop);

            bool result = item.TryGetProperty<NameProperty>(out NameProperty found);

            Assert.IsTrue(result);
            Assert.AreSame(prop, found);
        }

        [Test]
        [Description("TryGetProperty<T> when property absent => returns false and out-param is null?")]
        public void Item_TryGetProperty_Absent_ReturnsFalseAndNull()
        {
            var item = new Item();

            bool result = item.TryGetProperty<NameProperty>(out NameProperty found);

            Assert.IsFalse(result);
            Assert.IsNull(found);
        }

        // ── Add ──────────────────────────────────────────────────────────────

        [Test]
        [Description("Add<T> => property is retrievable immediately after add?")]
        public void Item_Add_PropertyRetrievableAfterAdd()
        {
            var item = new Item();
            var prop = new NameProperty();

            item.Add(prop);

            Assert.IsTrue(item.TryGetProperty<NameProperty>(out _));
        }

        [Test]
        [Description("Add<T> duplicate type => throws Exception?")]
        public void Item_Add_DuplicateType_ThrowsException()
        {
            var item = new Item();
            item.Add(new NameProperty());

            Assert.Throws<Exception>(() => item.Add(new NameProperty()));
        }

        // ── Remove ───────────────────────────────────────────────────────────

        [Test]
        [Description("Remove<T> after add => property no longer retrievable?")]
        public void Item_Remove_AfterAdd_PropertyNoLongerRetrievable()
        {
            var item = new Item();
            item.Add(new NameProperty());

            item.Remove<NameProperty>();

            Assert.IsFalse(item.TryGetProperty<NameProperty>(out _));
        }

        [Test]
        [Description("Remove<T> when property was never added => does not throw?")]
        public void Item_Remove_NeverAdded_DoesNotThrow()
        {
            var item = new Item();

            Assert.DoesNotThrow(() => item.Remove<NameProperty>());
        }

        [Test]
        [Description("Remove<T> only removes the targeted type, other properties remain?")]
        public void Item_Remove_OnlyTargetedTypeRemoved_OtherPropertyRetained()
        {
            var item = new Item();
            item.Add(new NameProperty());
            item.Add(new AmountProperty());

            item.Remove<NameProperty>();

            Assert.IsFalse(item.TryGetProperty<NameProperty>(out _));
            Assert.IsTrue(item.TryGetProperty<AmountProperty>(out _));
        }
    }
}
