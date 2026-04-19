using System.Collections.Generic;
using Calluna.DI;

namespace Calluna.Inventory
{
    public class Container : Injectable, Initializable, Cleanable
    {
        private ContainerAccessor _accessor;
        private ContainerChangedSignal _signal;
        public ReadonlyObservableList<Slot> Slots { get; private set; }

        public ReadonlyObservableList<Slot> ActiveSlots => _activeSlots;
        private readonly ObservableList<Slot> _activeSlots = new ObservableList<Slot>();

        public bool CanAdd(Item item) => _accessor.CanAdd(item);
        public bool CanRemove(Item item) => _accessor.CanRemove(item);
        public void Add(Item item) => _accessor.Add(item);
        public void Remove(Item item) => _accessor.Remove(item);

        private Filter _filter;
        private Sorter _sorter;

        void Injectable.Inject(Resolver resolver)
        {
            Slots = resolver.Resolve<ReadonlyObservableList<Slot>>();
            _filter = resolver.ResolveOptional<Filter>();
            _sorter = resolver.ResolveOptional<Sorter>();
            _accessor = resolver.Resolve<ContainerAccessor>();
            _signal = resolver.ResolveOptional<ContainerChangedSignal>();
        }

        void Initializable.Initialize()
        {
            UpdateActiveSlots();
            if (_filter != null)
                _filter.OnChanged += UpdateActiveSlots;
            if (_sorter != null)
                _sorter.OnChanged += UpdateActiveSlots;
            if (_signal != null)
                _signal.OnChanged += UpdateActiveSlots;
            Slots.OnItemRemoved += OnSlotChanged;
            Slots.OnItemAdded += OnSlotChanged;
            Slots.OnItemReplaced += OnSlotReplaced;
        }

        void Cleanable.Clean()
        {
            if (_filter != null)
                _filter.OnChanged -= UpdateActiveSlots;
            if (_sorter != null)
                _sorter.OnChanged -= UpdateActiveSlots;
            if (_signal != null)
                _signal.OnChanged -= UpdateActiveSlots;
            Slots.OnItemRemoved -= OnSlotChanged;
            Slots.OnItemAdded -= OnSlotChanged;
            Slots.OnItemReplaced -= OnSlotReplaced;
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

        private void OnSlotChanged(Slot slot, int index) => UpdateActiveSlots();

        private void OnSlotReplaced(Slot newSlot, Slot formerSlot, int index) => UpdateActiveSlots();
    }
}
