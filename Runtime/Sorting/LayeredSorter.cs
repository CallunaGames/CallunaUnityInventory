using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Calluna.DI;

namespace Calluna.Inventory
{
    public class LayeredSorter : Sorter
    {
        private readonly List<Sorter> _sorters = new List<Sorter>();
        public override bool IsActive => base.IsActive && _sorters.Any() && _sorters.Any(sorter => sorter.IsActive);

        public override void Inject(Resolver resolver)
        {
            base.Inject(resolver);
            IEnumerable<Sorter> sorters = resolver.ResolveOptional<IEnumerable<Sorter>>();
            if(sorters != null)
                _sorters.AddRange(sorters);
        }

        public override void Initialize()
        {
            base.Initialize();
            InitSorters();
        }

        public override void Clean()
        {
            base.Clean();
            RemoveListeners();
        }

        public override IOrderedEnumerable<Slot> Sort(IEnumerable<Slot> items)
        {
            IOrderedEnumerable<Slot> result = null;
            foreach (Sorter sorter in _sorters)
            {
                if(!sorter.IsActive)
                    continue;
                result = result == null ? sorter.Sort(items) : sorter.ThenBy(result);
            }
            return result;
        }

        public override IOrderedEnumerable<Slot> ThenBy(IOrderedEnumerable<Slot> items)
        {
            IOrderedEnumerable<Slot> result = items;
            foreach (Sorter sorter in _sorters)
            {
                if(!sorter.IsActive)
                    continue;
                items = sorter.ThenBy(items);
            }
            return result;
        }

        public void SetSorters(List<Sorter> sorters)
        {
            RemoveListeners();
            _sorters.Clear();
            _sorters.AddRange(sorters);
            InitSorters();
            InvokeOnChanged();
        }

        public void Add(Sorter sorter)
        {
            sorter.OnChanged += InvokeOnChanged;
            _sorters.Add(sorter);
            InvokeOnChanged();
        }

        public void Remove(Sorter sorter)
        {
            sorter.OnChanged -= InvokeOnChanged;
            _sorters.Remove(sorter);
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