using System;
using System.Collections.Generic;
using System.Linq;
using Calluna.DI;

namespace Calluna.Inventory
{
    public abstract class Sorter : Injectable, Initializable, Cleanable
    {
        public event Action OnChanged;
        public virtual bool IsActive => true;
        
        public virtual void Inject(Resolver resolver){}
        public virtual void Initialize(){}
        public virtual void Clean(){}
        
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