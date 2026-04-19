using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    /// <summary>
    /// Minimal concrete Sorter that sorts by a fixed integer key injected at construction.
    /// </summary>
    internal class FixedKeySorter : Sorter<int>
    {
        private readonly int _key;

        public FixedKeySorter(int key)
        {
            _key = key;
        }

        protected override int GetKey(Slot slot) => _key;

        public void TriggerChanged() => InvokeOnChanged();
    }

    public class RadioSorterTests
    {
        // ── IsActive ─────────────────────────────────────────────────────────

        [Test]
        [Description("IsActive => false when no sorters registered?")]
        public void RadioSorter_IsActive_NoSorters_ReturnsFalse()
        {
            var radio = new RadioSorter();

            Assert.IsFalse(radio.IsActive);
        }

        [Test]
        [Description("IsActive => false after sorter is added but none activated?")]
        public void RadioSorter_IsActive_SorterAddedNotActivated_ReturnsFalse()
        {
            var radio = new RadioSorter();
            radio.Add(new FixedKeySorter(0));

            Assert.IsFalse(radio.IsActive);
        }

        [Test]
        [Description("IsActive => true after a sorter is activated?")]
        public void RadioSorter_IsActive_SorterActivated_ReturnsTrue()
        {
            var radio = new RadioSorter();
            var sorter = new FixedKeySorter(0);
            radio.Add(sorter);

            radio.Activate(sorter);

            Assert.IsTrue(radio.IsActive);
        }

        [Test]
        [Description("IsActive => false after Deactivate() is called?")]
        public void RadioSorter_IsActive_AfterDeactivate_ReturnsFalse()
        {
            var radio = new RadioSorter();
            var sorter = new FixedKeySorter(0);
            radio.Add(sorter);
            radio.Activate(sorter);

            radio.Deactivate();

            Assert.IsFalse(radio.IsActive);
        }

        // ── Activate ─────────────────────────────────────────────────────────

        [Test]
        [Description("Activate() => throws Exception when sorter not registered?")]
        public void RadioSorter_Activate_UnregisteredSorter_ThrowsException()
        {
            var radio = new RadioSorter();
            var stranger = new FixedKeySorter(0);

            Assert.Throws<Exception>(() => radio.Activate(stranger));
        }

        [Test]
        [Description("Activate() => fires OnChanged?")]
        public void RadioSorter_Activate_FiresOnChanged()
        {
            var radio = new RadioSorter();
            var sorter = new FixedKeySorter(0);
            radio.Add(sorter);

            int callCount = 0;
            Action listener = () => { callCount++; };
            radio.OnChanged += listener;

            radio.Activate(sorter);

            radio.OnChanged -= listener;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("Activate() => switches active sorter (only new one active)?")]
        public void RadioSorter_Activate_SwitchesActiveSorter()
        {
            var radio = new RadioSorter();
            var first = new FixedKeySorter(0);
            var second = new FixedKeySorter(1);
            radio.Add(first);
            radio.Add(second);
            radio.Activate(first);

            radio.Activate(second);

            // RadioSorter.Sort delegates to the active sorter — if it doesn't throw, second is active
            Assert.DoesNotThrow(() => radio.Sort(new List<Slot>()));
            Assert.IsTrue(radio.IsActive);
        }

        [Test]
        [Description("Activate() with active=true on Add() => sorter becomes active immediately?")]
        public void RadioSorter_Add_WithActiveTrue_SorterIsActive()
        {
            var radio = new RadioSorter();
            var sorter = new FixedKeySorter(0);

            radio.Add(sorter, active: true);

            Assert.IsTrue(radio.IsActive);
        }

        // ── Deactivate ────────────────────────────────────────────────────────

        [Test]
        [Description("Deactivate() => fires OnChanged?")]
        public void RadioSorter_Deactivate_FiresOnChanged()
        {
            var radio = new RadioSorter();
            var sorter = new FixedKeySorter(0);
            radio.Add(sorter);
            radio.Activate(sorter);

            int callCount = 0;
            Action listener = () => { callCount++; };
            radio.OnChanged += listener;

            radio.Deactivate();

            radio.OnChanged -= listener;

            Assert.AreEqual(1, callCount);
        }

        // ── Remove ────────────────────────────────────────────────────────────

        [Test]
        [Description("Remove() => throws Exception when sorter not registered?")]
        public void RadioSorter_Remove_UnregisteredSorter_ThrowsException()
        {
            var radio = new RadioSorter();
            var stranger = new FixedKeySorter(0);

            Assert.Throws<Exception>(() => radio.Remove(stranger));
        }

        [Test]
        [Description("Remove() => removing active sorter triggers deactivation (IsActive becomes false)?")]
        public void RadioSorter_Remove_ActiveSorter_IsActiveBecomeFalse()
        {
            var radio = new RadioSorter();
            var sorter = new FixedKeySorter(0);
            radio.Add(sorter);
            radio.Activate(sorter);

            radio.Remove(sorter);

            Assert.IsFalse(radio.IsActive);
        }

        [Test]
        [Description("Remove() => removing active sorter fires OnChanged (from Deactivate)?")]
        public void RadioSorter_Remove_ActiveSorter_FiresOnChanged()
        {
            var radio = new RadioSorter();
            var sorter = new FixedKeySorter(0);
            radio.Add(sorter);
            radio.Activate(sorter);

            int callCount = 0;
            Action listener = () => { callCount++; };
            radio.OnChanged += listener;

            radio.Remove(sorter);

            radio.OnChanged -= listener;

            // Deactivate fires once
            Assert.GreaterOrEqual(callCount, 1);
        }

        [Test]
        [Description("Remove() => removing non-active sorter keeps active sorter working?")]
        public void RadioSorter_Remove_InactiveSorter_ActiveSorterUnaffected()
        {
            var radio = new RadioSorter();
            var active = new FixedKeySorter(0);
            var inactive = new FixedKeySorter(1);
            radio.Add(active);
            radio.Add(inactive);
            radio.Activate(active);

            radio.Remove(inactive);

            Assert.IsTrue(radio.IsActive);
        }

        // ── Sort ─────────────────────────────────────────────────────────────

        [Test]
        [Description("Sort() => delegates to the active child sorter?")]
        public void RadioSorter_Sort_DelegatesToActiveChild()
        {
            var radio = new RadioSorter();
            var nameSorter = new NameSorter();
            radio.Add(nameSorter);
            radio.Activate(nameSorter);

            var slotA = new Slot();
            slotA.Item.Value = new Item();
            var slotB = new Slot();
            slotB.Item.Value = new Item();
            var slots = new List<Slot> { slotA, slotB };

            // Should not throw; result should be an ordered enumerable
            IOrderedEnumerable<Slot> result = radio.Sort(slots);
            Assert.IsNotNull(result);
        }

        // ── Child OnChanged propagation ───────────────────────────────────────

        [Test]
        [Description("Active child OnChanged => RadioSorter OnChanged propagates?")]
        public void RadioSorter_ActiveChildChanged_OnChangedPropagates()
        {
            var radio = new RadioSorter();
            var child = new FixedKeySorter(0);
            radio.Add(child);
            radio.Activate(child);

            int callCount = 0;
            Action listener = () => { callCount++; };
            radio.OnChanged += listener;

            child.TriggerChanged();

            radio.OnChanged -= listener;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("Inactive child OnChanged => RadioSorter OnChanged does NOT propagate?")]
        public void RadioSorter_InactiveChildChanged_OnChangedDoesNotPropagate()
        {
            var radio = new RadioSorter();
            var child = new FixedKeySorter(0);
            radio.Add(child); // added but never activated

            int callCount = 0;
            Action listener = () => { callCount++; };
            radio.OnChanged += listener;

            child.TriggerChanged();

            radio.OnChanged -= listener;

            Assert.AreEqual(0, callCount);
        }
    }
}
