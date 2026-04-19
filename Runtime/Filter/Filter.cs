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

        internal IEnumerable<Slot> ApplyTo(IEnumerable<Slot> slots)
        {
            if (!IsActive.Value)
                return slots;
            return slots.Where(slot => ApplyTo(slot.Item.Value));
        }

        public abstract bool ApplyTo(Item item);

        protected virtual void InvokeOnChanged() => OnChanged?.Invoke();
    }
}