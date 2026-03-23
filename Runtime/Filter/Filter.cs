using System;
using System.Collections.Generic;
using System.Linq;

namespace Calluna.Inventory
{
    public abstract class Filter : IDisposable
    {
        public event Action OnChanged;
        public Observable<bool> IsActive { get; } = new() { Value = false };

        public IEnumerable<Slot> ApplyTo(IEnumerable<Slot> items)
        {
            if (!IsActive.Value)
                return items;
            return items.Where(slot => slot.Item.HasValue && ApplyTo(slot.Item.Value));
        }

        public abstract bool ApplyTo(Item item);
        public virtual void Dispose(){}

        protected void InvokeOnChanged()
        {
            OnChanged?.Invoke();
        }
    }
}