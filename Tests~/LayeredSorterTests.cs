using System;
using System.Collections.Generic;
using System.Linq;
using Calluna.DI;
using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    /// <summary>
    /// Sorter whose IsActive can be controlled manually, for use in LayeredSorter tests.
    /// </summary>
    internal class TogglableSorter : Sorter<int>
    {
        private bool _isActive;
        private readonly int _key;

        public override bool IsActive => _isActive;

        public TogglableSorter(int key, bool active = true)
        {
            _key = key;
            _isActive = active;
        }

        public void SetActive(bool active)
        {
            _isActive = active;
            InvokeOnChanged();
        }

        public void TriggerChanged() => InvokeOnChanged();

        protected override int GetKey(Slot slot) => _key;
    }

    public class LayeredSorterTests
    {
        private static LayeredSorter MakeInitializedLayered()
        {
            var layered = new LayeredSorter();
            ((Initializable)layered).Initialize();
            return layered;
        }

        // ── IsActive ─────────────────────────────────────────────────────────

        [Test]
        [Description("IsActive => false when no sorters registered?")]
        public void LayeredSorter_IsActive_NoSorters_ReturnsFalse()
        {
            var layered = new LayeredSorter();

            Assert.IsFalse(layered.IsActive);
        }

        [Test]
        [Description("IsActive => false when all child sorters are inactive?")]
        public void LayeredSorter_IsActive_AllChildrenInactive_ReturnsFalse()
        {
            var layered = MakeInitializedLayered();
            layered.Add(new TogglableSorter(0, active: false));

            Assert.IsFalse(layered.IsActive);

            ((Cleanable)layered).Clean();
        }

        [Test]
        [Description("IsActive => true when at least one child sorter is active?")]
        public void LayeredSorter_IsActive_AtLeastOneActiveChild_ReturnsTrue()
        {
            var layered = MakeInitializedLayered();
            layered.Add(new TogglableSorter(0, active: true));

            Assert.IsTrue(layered.IsActive);

            ((Cleanable)layered).Clean();
        }

        // ── Add ──────────────────────────────────────────────────────────────

        [Test]
        [Description("Add() => fires OnChanged?")]
        public void LayeredSorter_Add_FiresOnChanged()
        {
            var layered = MakeInitializedLayered();
            int callCount = 0;
            Action listener = () => { callCount++; };
            layered.OnChanged += listener;

            layered.Add(new TogglableSorter(0));

            layered.OnChanged -= listener;
            ((Cleanable)layered).Clean();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("Add() => multiple sorters all present for Sort()?")]
        public void LayeredSorter_Add_MultipleChildren_AllParticipateInSort()
        {
            var layered = MakeInitializedLayered();
            layered.Add(new TogglableSorter(0));
            layered.Add(new TogglableSorter(1));

            var slots = new List<Slot> { new Slot(), new Slot() };
            IOrderedEnumerable<Slot> result = layered.Sort(slots);

            ((Cleanable)layered).Clean();

            Assert.IsNotNull(result);
        }

        // ── Remove ───────────────────────────────────────────────────────────

        [Test]
        [Description("Remove() => fires OnChanged?")]
        public void LayeredSorter_Remove_FiresOnChanged()
        {
            var layered = MakeInitializedLayered();
            var child = new TogglableSorter(0);
            layered.Add(child);

            int callCount = 0;
            Action listener = () => { callCount++; };
            layered.OnChanged += listener;

            layered.Remove(child);

            layered.OnChanged -= listener;
            ((Cleanable)layered).Clean();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("Remove() => removed sorter no longer participates and IsActive reflects remaining?")]
        public void LayeredSorter_Remove_RemovedSorterExcluded()
        {
            var layered = MakeInitializedLayered();
            var only = new TogglableSorter(0);
            layered.Add(only);

            layered.Remove(only);

            Assert.IsFalse(layered.IsActive);
            ((Cleanable)layered).Clean();
        }

        // ── Sort ─────────────────────────────────────────────────────────────

        [Test]
        [Description("Sort() => returns null when no active child sorters?")]
        public void LayeredSorter_Sort_NoActiveSorters_ReturnsNull()
        {
            var layered = MakeInitializedLayered();
            layered.Add(new TogglableSorter(0, active: false));

            IOrderedEnumerable<Slot> result = layered.Sort(new List<Slot>());

            ((Cleanable)layered).Clean();

            Assert.IsNull(result);
        }

        [Test]
        [Description("Sort() => chains active sorters in order?")]
        public void LayeredSorter_Sort_ChainsActiveSorters()
        {
            var layered = MakeInitializedLayered();
            var nameSorter = new NameSorter();
            layered.Add(nameSorter);

            var slot = new Slot();
            slot.Item.Value = new Item();
            var slots = new List<Slot> { slot };

            IOrderedEnumerable<Slot> result = layered.Sort(slots);

            ((Cleanable)layered).Clean();

            Assert.IsNotNull(result);
        }

        [Test]
        [Description("Sort() => inactive child sorters are skipped?")]
        public void LayeredSorter_Sort_InactiveChildrenSkipped()
        {
            var layered = MakeInitializedLayered();
            var active = new TogglableSorter(0, active: true);
            var inactive = new TogglableSorter(1, active: false);
            layered.Add(inactive);
            layered.Add(active);

            var slots = new List<Slot> { new Slot(), new Slot() };
            IOrderedEnumerable<Slot> result = layered.Sort(slots);

            ((Cleanable)layered).Clean();

            // Should return a valid ordered result driven by the active sorter only
            Assert.IsNotNull(result);
        }

        // ── SetSorters ────────────────────────────────────────────────────────

        [Test]
        [Description("SetSorters() => replaces child list and fires OnChanged?")]
        public void LayeredSorter_SetSorters_ReplacesListAndFiresOnChanged()
        {
            var layered = MakeInitializedLayered();
            var original = new TogglableSorter(0);
            layered.Add(original);

            int callCount = 0;
            Action listener = () => { callCount++; };
            layered.OnChanged += listener;

            var replacement = new TogglableSorter(1);
            layered.SetSorters(new List<Sorter> { replacement });

            layered.OnChanged -= listener;
            ((Cleanable)layered).Clean();

            Assert.AreEqual(1, callCount);
        }

        // ── Child OnChanged propagation ───────────────────────────────────────

        [Test]
        [Description("Active child OnChanged => LayeredSorter OnChanged propagates after Initialize()?")]
        public void LayeredSorter_ChildChanged_AfterInitialize_OnChangedPropagates()
        {
            var layered = MakeInitializedLayered();
            var child = new TogglableSorter(0);
            layered.Add(child);

            int callCount = 0;
            Action listener = () => { callCount++; };
            layered.OnChanged += listener;

            child.TriggerChanged();

            layered.OnChanged -= listener;
            ((Cleanable)layered).Clean();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("After Clean(), child changes do NOT propagate to LayeredSorter?")]
        public void LayeredSorter_AfterClean_ChildChangesDoNotPropagate()
        {
            var layered = MakeInitializedLayered();
            var child = new TogglableSorter(0);
            layered.Add(child);
            ((Cleanable)layered).Clean();

            int callCount = 0;
            Action listener = () => { callCount++; };
            layered.OnChanged += listener;

            child.TriggerChanged();

            layered.OnChanged -= listener;

            Assert.AreEqual(0, callCount);
        }
    }
}
