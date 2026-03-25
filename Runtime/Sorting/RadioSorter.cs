using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Calluna.DI;

namespace Calluna.Inventory
{
    public class RadioSorter : Sorter
    {
        private readonly List<Sorter> _sorters = new List<Sorter>();
        public override bool IsActive => base.IsActive && _sorters.Any() && _activeSorterIndex >= 0;

        private int _activeSorterIndex = -1;

        public override void Inject(Resolver resolver)
        {
            base.Inject(resolver);
            IEnumerable<Sorter> sorters = resolver.ResolveOptional<IEnumerable<Sorter>>();
            if(sorters != null)
                _sorters.AddRange(sorters);
        }

        public override void Clean()
        {
            base.Clean();
            CleanCurrent();
        }

        public void Activate(Sorter sorter)
        {
            int index = _sorters.IndexOf(sorter);
            if(index < 0)
                throw new Exception($"Sorter {sorter} is not part of this {nameof(RadioSorter)}");
            CleanCurrent();
            _activeSorterIndex = index;
            _sorters[_activeSorterIndex].OnChanged += InvokeOnChanged;
            InvokeOnChanged();
        }

        private void CleanCurrent()
        {
            if(_activeSorterIndex < 0)
                return;
            _sorters[_activeSorterIndex].OnChanged -= InvokeOnChanged;
        }

        public void Deactivate()
        {
            CleanCurrent();
            _activeSorterIndex = -1;
            InvokeOnChanged();
        }

        public void Add(Sorter sorter, bool active = false)
        {
            _sorters.Add(sorter);
            if (active)
                Activate(sorter);
        }

        public void Remove(Sorter sorter)
        {
            int index = _sorters.IndexOf(sorter);
            if(index < 0)
                throw new Exception($"Sorter {sorter} is not part of this {nameof(RadioSorter)}");
            _sorters.RemoveAt(index);
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