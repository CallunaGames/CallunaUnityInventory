using System.Collections.Generic;
using Calluna.DI;

namespace Calluna.Inventory
{
    public class Container : Injectable, Initializable, Cleanable
    {
        public ItemsAccessor ItemsAccessor { get; private set; }
        public ReadonlyObservableList<Slot> Slots => _slots;
        private ObservableList<Slot> _slots;

        public ReadonlyObservableList<Slot> ActiveSlots => _activeSlots;
        private readonly ObservableList<Slot> _activeSlots = new ObservableList<Slot>();

        private Filter _filter;
        private Sorter _sorter;
        private ObservableListChangeDetector<Slot> _slotsChangeDetector;
        
        void Injectable.Inject(Resolver resolver)
        {
            _slots = resolver.Resolve<ObservableList<Slot>>();
            _filter = resolver.Resolve<Filter>();
            _sorter = resolver.Resolve<Sorter>();
            _slotsChangeDetector = new ObservableListChangeDetector<Slot>(_slots);
            ItemsAccessor = resolver.Resolve<ItemsAccessor>();
        }

        void Initializable.Initialize()
        {
            UpdateActiveSlots();
            _filter.OnChanged += UpdateActiveSlots;
            _sorter.OnChanged += UpdateActiveSlots;
            _slotsChangeDetector.OnChanged += UpdateActiveSlots;
        }

        void Cleanable.Clean()
        {
            _filter.Dispose();
            _sorter.Dispose();
            _filter.OnChanged -= UpdateActiveSlots;
            _sorter.OnChanged -= UpdateActiveSlots;
            _slotsChangeDetector.OnChanged -= UpdateActiveSlots;
        }

        private void UpdateActiveSlots()
        {
            _activeSlots.OverrideWith(ApplySorting(ApplyFilters(_slots)));
        }

        private IEnumerable<Slot> ApplyFilters(IEnumerable<Slot> slots)
        {
            return _filter.IsActive.Value ? _filter.ApplyTo(slots) : slots;
        }

        private IEnumerable<Slot> ApplySorting(IEnumerable<Slot> slots)
        {
            return _sorter.IsActive ? _sorter.Sort(slots) : slots;
        }
    }
}
