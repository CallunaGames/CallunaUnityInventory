using System;
using System.Collections.Generic;
using System.Linq;

namespace Calluna.Inventory
{
    public abstract class Sorter : IDisposable
    {
        public event Action OnChanged;
        public virtual bool IsActive => true;
        public virtual void Dispose(){}
        public abstract IOrderedEnumerable<Slot> Sort(IEnumerable<Slot> items);
        public abstract IOrderedEnumerable<Slot> ThenBy(IOrderedEnumerable<Slot> items);

        protected void InvokeOnChanged() => OnChanged?.Invoke();
    }

    public abstract class Sorter<TKey> : Sorter
    {
        public override IOrderedEnumerable<Slot> Sort(IEnumerable<Slot> items)
        {
            return items.OrderBy(GetKey);
        }

        public override IOrderedEnumerable<Slot> ThenBy(IOrderedEnumerable<Slot> items)
        {
            return items.ThenBy(GetKey);
        }

        protected abstract TKey GetKey(Slot slot);
    }
}