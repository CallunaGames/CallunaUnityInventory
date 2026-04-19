using System.Collections.Generic;
using System.Linq;
using Calluna.DI;

namespace Calluna.Inventory
{
    public class LayeredSorter : Sorter
    {
        private readonly List<Sorter> _sorters = new List<Sorter>();
        public override bool IsActive => GetIsActive();

        private bool GetIsActive()
        {
            if (!base.IsActive || _sorters.Count == 0)
                return false;
            for (int i = 0; i < _sorters.Count; i++)
            {
                if (_sorters[i].IsActive)
                    return true;
            }
            return false;
        }

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
            SubscribeAll();
        }

        public override void Clean()
        {
            base.Clean();
            UnsubscribeAll();
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
                result = sorter.ThenBy(result);
            }
            return result;
        }

        public void SetSorters(List<Sorter> sorters)
        {
            UnsubscribeAll();
            _sorters.Clear();
            _sorters.AddRange(sorters);
            SubscribeAll();
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

        private void SubscribeAll()
        {
            foreach (Sorter sorter in _sorters)
            {
                sorter.OnChanged += InvokeOnChanged;
            }
        }

        private void UnsubscribeAll()
        {
            foreach (Sorter sorter in _sorters)
            {
                sorter.OnChanged -= InvokeOnChanged;
            }
        }
    }
}