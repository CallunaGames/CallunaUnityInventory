using System;
using System.Collections.Generic;
using System.Linq;
using Calluna.DI;

namespace Calluna.Inventory
{
    public abstract class Filter : Injectable, Initializable, Cleanable
    {
        public event Action OnChanged;
        public Observable<bool> IsActive { get; } = new() { Value = false };
        
        public virtual void Inject(Resolver resolver){}
        public virtual void Initialize(){}
        public virtual void Clean(){}

        public IEnumerable<Slot> ApplyTo(IEnumerable<Slot> items)
        {
            if (!IsActive.Value)
                return items;
            return items.Where(slot => ApplyTo(slot.Item.Value));
        }

        public abstract bool ApplyTo(Item item);

        protected void InvokeOnChanged()
        {
            OnChanged?.Invoke();
        }
    }
}