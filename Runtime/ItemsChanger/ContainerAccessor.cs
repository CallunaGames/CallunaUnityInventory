using System;
using Calluna.DI;

namespace Calluna.Inventory
{
    public abstract class ContainerAccessor : Injectable
    {
        protected ObservableList<Slot> _slots;
        protected SlotProvider _slotProvider;

        public virtual void Inject(Resolver resolver)
        {
            _slots = resolver.Resolve<ObservableList<Slot>>();
            _slotProvider = resolver.Resolve<SlotProvider>();
        }

        public abstract bool CanRemove(Item item);
        public abstract bool CanAdd(Item item);

        public abstract void Add(Item item);
        /// <summary>Removes <paramref name="item"/>; check <see cref="CanRemove"/> first if it may be absent.</summary>
        public abstract void Remove(Item item);

        [Obsolete("Not used by Container. Will be removed in 2.0.0.")]
        public virtual bool CanSetAt(Item item, int index) => throw new NotSupportedException();

        [Obsolete("Not used by Container. Will be removed in 2.0.0.")]
        public virtual Item this[int index]
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}
