using System.Collections.Generic;
using Calluna.DI;

namespace Calluna.Inventory
{
    public class Container : Injectable, Initializable, Cleanable
    {
        public ContainerAccessor Accessor { get; private set; }
        public ReadonlyObservableList<Slot> Slots { get; private set; }

        public ReadonlyObservableList<Slot> ActiveSlots => _activeSlots;
        private readonly ObservableList<Slot> _activeSlots = new ObservableList<Slot>();

        private Filter _filter;
        private Sorter _sorter;
        
        void Injectable.Inject(Resolver resolver)
        {
            Slots = resolver.Resolve<ReadonlyObservableList<Slot>>();
            _filter = resolver.ResolveOptional<Filter>();
            _sorter = resolver.ResolveOptional<Sorter>();
            Accessor = resolver.Resolve<ContainerAccessor>();
        }

        void Initializable.Initialize()
        {
            UpdateActiveSlots();
            if(_filter != null)
                _filter.OnChanged += UpdateActiveSlots;
            if(_sorter != null)
                _sorter.OnChanged += UpdateActiveSlots;
            Slots.OnItemRemoved += OnSlotRemoved;
            Slots.OnItemAdded += OnItemAdded;
            Slots.OnItemReplaced += OnItemReplaced;
        }

        void Cleanable.Clean()
        {
            if(_filter != null)
                _filter.OnChanged -= UpdateActiveSlots;
            if(_sorter != null)
                _sorter.OnChanged -= UpdateActiveSlots;
            Slots.OnItemRemoved -= OnSlotRemoved;
            Slots.OnItemAdded -= OnItemAdded;
            Slots.OnItemReplaced -= OnItemReplaced;
        }

        private void UpdateActiveSlots()
        {
            _activeSlots.OverrideWith(ApplySorting(ApplyFilters(Slots)));
        }

        private IEnumerable<Slot> ApplyFilters(IEnumerable<Slot> slots)
        {
            return _filter != null && _filter.IsActive.Value ? _filter.ApplyTo(slots) : slots;
        }

        private IEnumerable<Slot> ApplySorting(IEnumerable<Slot> slots)
        {
            return _sorter is { IsActive: true } ? _sorter.Sort(slots) : slots;
        }

        private void OnSlotRemoved(Slot item, int index)
        {
            _activeSlots.Remove(item);
        }

        private void OnItemAdded(Slot slot, int index)
        {
            if(_filter.IsActive.Value && !_filter.ApplyTo(slot.Item.Value))
                return;
            _activeSlots.Add(slot);
            _activeSlots.OverrideWith(ApplySorting(ActiveSlots));
        }

        private void OnItemReplaced(Slot newitem, Slot formeritem, int index)
        {
            OnSlotRemoved(formeritem, index);
            OnItemAdded(newitem, index);
        }
    }
}
