using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Calluna.Inventory
{
    public class LayeredSorter : Sorter
    {
        private List<Sorter> _sorters = new List<Sorter>();
        public override bool IsActive => base.IsActive && _sorters.Any();

        public LayeredSorter(IEnumerable<Sorter> sorters)
        {
            _sorters.AddRange(sorters);
            InitSorters();
        }

        public override void Dispose()
        {
            base.Dispose();
            RemoveListeners();
            foreach (Sorter sorter in _sorters)
            {
                sorter.Dispose();
            }
        }

        public override IOrderedEnumerable<Slot> Sort(IEnumerable<Slot> items)
        {
            IOrderedEnumerable<Slot> result = null;
            foreach (Sorter sorter in _sorters)
            {
                result = result == null ? sorter.Sort(items) : sorter.ThenBy(result);
            }
            return result;
        }

        public override IOrderedEnumerable<Slot> ThenBy(IOrderedEnumerable<Slot> items)
        {
            IOrderedEnumerable<Slot> result = items;
            foreach (Sorter sorter in _sorters)
            {
                items = sorter.ThenBy(items);
            }
            return result;
        }

        public void SetSorters(List<Sorter> sorters)
        {
            RemoveListeners();
            _sorters = sorters;
            InitSorters();
            InvokeOnChanged();
        }

        private void InitSorters()
        {
            foreach (Sorter sorter in _sorters)
            {
                sorter.OnChanged += InvokeOnChanged;
            }
        }

        private void RemoveListeners()
        {
            foreach (Sorter sorter in _sorters)
            {
                sorter.OnChanged -= InvokeOnChanged;
            }
        }
    }
}