using Calluna.DI;

namespace Calluna.Inventory
{
    public abstract class ItemsAccessor : Injectable
    {
        protected ObservableList<Slot> _slots;
        protected SlotProvider _slotProvider;
        
        public virtual void Inject(Resolver resolver)
        {
            _slots = resolver.Resolve<ObservableList<Slot>>();
            _slotProvider = resolver.Resolve<SlotProvider>();
        }
        
        public abstract bool CanRemove(Item item);
        public abstract bool CanSetAt(Item item, int index);
        public abstract bool CanAdd(Item item);

        public abstract void Add(Item item);
        public abstract void Remove(Item item);
        public abstract Item this[int index] { get; set; }
    }
}