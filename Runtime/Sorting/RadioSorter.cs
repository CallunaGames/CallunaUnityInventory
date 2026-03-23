using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Calluna.Inventory
{
    public class RadioSorter : Sorter
    {
        private readonly List<Sorter> _sorters = new List<Sorter>();
        public override bool IsActive => base.IsActive && _sorters.Any() && _activeSorterIndex > 0;

        private int _activeSorterIndex = -1;

        public RadioSorter(IEnumerable<Sorter> sorters)
        {
            _sorters.AddRange(sorters);
        }

        public override void Clean()
        {
            base.Clean();
            if(_activeSorterIndex > -1)
                _sorters[_activeSorterIndex].OnChanged -= InvokeOnChanged;
        }

        public void Activate(Sorter sorter)
        {
            _activeSorterIndex = _sorters.IndexOf(sorter);
            if(_activeSorterIndex == -1)
                _sorters[_activeSorterIndex].OnChanged += InvokeOnChanged;
            InvokeOnChanged();
        }

        public void Deactivate()
        {
            if (_activeSorterIndex == -1)
                return;
            _sorters[_activeSorterIndex].OnChanged -= InvokeOnChanged;
            InvokeOnChanged();
            _activeSorterIndex = -1;
        }

        public void Add(Sorter sorter)
        {
            _sorters.Add(sorter);
        }

        public void Remove(Sorter sorter)
        {
            int index = _sorters.IndexOf(sorter);
            _sorters.Remove(sorter);
            if(_activeSorterIndex == index)
                Deactivate();
        }

        public override IOrderedEnumerable<Slot> Sort(IEnumerable<Slot> items)
        {
            return _sorters[_activeSorterIndex].Sort(items);
        }

        public override IOrderedEnumerable<Slot> ThenBy(IOrderedEnumerable<Slot> items)
        {
            return _sorters[_activeSorterIndex].ThenBy(items);
        }
    }
}