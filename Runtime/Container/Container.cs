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
        private ObservableListChangeDetector<Slot> _slotsChangeDetector;
        
        void Injectable.Inject(Resolver resolver)
        {
            Slots = resolver.Resolve<ReadonlyObservableList<Slot>>();
            _filter = resolver.ResolveOptional<Filter>();
            _sorter = resolver.ResolveOptional<Sorter>();
            _slotsChangeDetector = new ObservableListChangeDetector<Slot>(Slots);
            Accessor = resolver.Resolve<ContainerAccessor>();
        }

        void Initializable.Initialize()
        {
            UpdateActiveSlots();
            if(_filter != null)
                _filter.OnChanged += UpdateActiveSlots;
            if(_sorter != null)
                _sorter.OnChanged += UpdateActiveSlots;
            _slotsChangeDetector.OnChanged += UpdateActiveSlots;
        }

        void Cleanable.Clean()
        {
            if(_filter != null)
                _filter.OnChanged -= UpdateActiveSlots;
            if(_sorter != null)
                _sorter.OnChanged -= UpdateActiveSlots;
            _slotsChangeDetector.OnChanged -= UpdateActiveSlots;
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
    }
}
