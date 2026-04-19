using Calluna.DI;
using NUnit.Framework;

namespace Calluna.Inventory.Tests
{
    /// <summary>
    /// Concrete Filter subclass that always returns a fixed result.
    /// IsActive must be set to true manually to engage the filter.
    /// </summary>
    internal class StubFilter : Filter
    {
        private bool _result;

        public StubFilter(bool result)
        {
            _result = result;
        }

        public void SetResult(bool result)
        {
            _result = result;
            InvokeOnChanged();
        }

        public override bool ApplyTo(Item item) => _result;
    }

    public class FilterGroupTests
    {
        private FilterGroup MakeInitializedGroup()
        {
            var group = new FilterGroup();
            ((Initializable)group).Initialize();
            return group;
        }

        // ── Add ──────────────────────────────────────────────────────────────

        [Test]
        [Description("Add() => OnChanged fires once?")]
        public void FilterGroup_Add_OnChangedFired()
        {
            var group = MakeInitializedGroup();
            int callCount = 0;
            System.Action listener = () => { callCount++; };
            group.OnChanged += listener;

            group.Add(new StubFilter(true));

            group.OnChanged -= listener;
            ((Cleanable)group).Clean();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("Add() => newly added active filter participates in ApplyTo?")]
        public void FilterGroup_Add_ActiveFilter_ParticipatesInApply()
        {
            var group = MakeInitializedGroup();
            var filter = new StubFilter(false);
            filter.IsActive.Value = true;
            group.Add(filter);

            bool result = group.ApplyTo(new Item());

            ((Cleanable)group).Clean();

            Assert.IsFalse(result);
        }

        // ── Remove ───────────────────────────────────────────────────────────

        [Test]
        [Description("Remove() => OnChanged fires once?")]
        public void FilterGroup_Remove_OnChangedFired()
        {
            var group = MakeInitializedGroup();
            var filter = new StubFilter(true);
            group.Add(filter);

            int callCount = 0;
            System.Action listener = () => { callCount++; };
            group.OnChanged += listener;

            group.Remove(filter);

            group.OnChanged -= listener;
            ((Cleanable)group).Clean();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("Remove() => removed filter no longer affects ApplyTo?")]
        public void FilterGroup_Remove_FilterNoLongerApplied()
        {
            var group = MakeInitializedGroup();
            var filter = new StubFilter(false);
            filter.IsActive.Value = true;
            group.Add(filter);

            // Before remove: group should fail
            Assert.IsFalse(group.ApplyTo(new Item()));

            group.Remove(filter);

            // After remove: nothing left to reject
            Assert.IsTrue(group.ApplyTo(new Item()));

            ((Cleanable)group).Clean();
        }

        // ── ApplyTo AND logic ─────────────────────────────────────────────────

        [Test]
        [Description("ApplyTo(Item) AND logic => false when any active filter returns false?")]
        public void FilterGroup_ApplyTo_AndLogic_FalseWhenAnyActiveFails()
        {
            var group = MakeInitializedGroup();
            var pass = new StubFilter(true);
            var fail = new StubFilter(false);
            pass.IsActive.Value = true;
            fail.IsActive.Value = true;
            group.Add(pass);
            group.Add(fail);

            bool result = group.ApplyTo(new Item());

            ((Cleanable)group).Clean();

            Assert.IsFalse(result);
        }

        [Test]
        [Description("ApplyTo(Item) AND logic => true when all active filters pass?")]
        public void FilterGroup_ApplyTo_AndLogic_TrueWhenAllActivePass()
        {
            var group = MakeInitializedGroup();
            var a = new StubFilter(true);
            var b = new StubFilter(true);
            a.IsActive.Value = true;
            b.IsActive.Value = true;
            group.Add(a);
            group.Add(b);

            bool result = group.ApplyTo(new Item());

            ((Cleanable)group).Clean();

            Assert.IsTrue(result);
        }

        [Test]
        [Description("ApplyTo(Item) => inactive filters are skipped (do not affect result)?")]
        public void FilterGroup_ApplyTo_InactiveFiltersSkipped()
        {
            var group = MakeInitializedGroup();
            var inactiveReject = new StubFilter(false);
            // IsActive defaults to false — filter is inactive

            group.Add(inactiveReject);

            bool result = group.ApplyTo(new Item());

            ((Cleanable)group).Clean();

            // Inactive filter should not reject
            Assert.IsTrue(result);
        }

        // ── OnChanged propagation ─────────────────────────────────────────────

        [Test]
        [Description("Child filter OnChanged => group's OnChanged propagates?")]
        public void FilterGroup_ChildFilterChanged_GroupOnChangedFires()
        {
            var group = MakeInitializedGroup();
            var filter = new StubFilter(true);
            group.Add(filter);

            int callCount = 0;
            System.Action listener = () => { callCount++; };
            group.OnChanged += listener;

            filter.SetResult(false);

            group.OnChanged -= listener;
            ((Cleanable)group).Clean();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("Child IsActive change => group's OnChanged propagates?")]
        public void FilterGroup_ChildIsActiveChanged_GroupOnChangedFires()
        {
            var group = MakeInitializedGroup();
            var filter = new StubFilter(true);
            group.Add(filter);

            int callCount = 0;
            System.Action listener = () => { callCount++; };
            group.OnChanged += listener;

            filter.IsActive.Value = true;

            group.OnChanged -= listener;
            ((Cleanable)group).Clean();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Description("After Clean(), child filter changes do NOT propagate to group?")]
        public void FilterGroup_AfterClean_ChildChangesDoNotPropagate()
        {
            var group = MakeInitializedGroup();
            var filter = new StubFilter(true);
            group.Add(filter);
            ((Cleanable)group).Clean();

            int callCount = 0;
            System.Action listener = () => { callCount++; };
            group.OnChanged += listener;

            filter.SetResult(false);

            group.OnChanged -= listener;

            Assert.AreEqual(0, callCount);
        }
    }
}
