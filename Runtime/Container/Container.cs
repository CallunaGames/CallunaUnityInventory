using System;
using System.Collections.Generic;
using Calluna.DI;

namespace Calluna.Inventory
{
    public class Container : Injectable, Initializable, Cleanable
    {
        private const string SchedulerId = "active_slots";

        private ContainerAccessor _accessor;
        private ContainerChangedSignal _signal;
        private UpdateScheduler _scheduler;

        public ReadonlyObservableList<Slot> Slots { get; private set; }

        public ReadonlyObservableList<Slot> ActiveSlots => _activeSlots;
        private readonly ObservableList<Slot> _activeSlots = new ObservableList<Slot>();

        public bool CanAdd(Item item) => _accessor.CanAdd(item);
        public bool CanRemove(Item item) => _accessor.CanRemove(item);
        public void Add(Item item) => _accessor.Add(item);
        public void Remove(Item item) => _accessor.Remove(item);

        private Filter _filter;
        private Sorter _sorter;

        // Cached delegate — reused every time ScheduleActiveSlotsUpdate passes it to
        // ScheduleOnce, preventing a new Action allocation on each filter/sorter change.
        private readonly Action _updateActiveSlotsAction;
        private readonly Action _scheduleUpdateActiveSlotsAction;

        public Container()
        {
            _updateActiveSlotsAction = UpdateActiveSlots;
            _scheduleUpdateActiveSlotsAction = ScheduleActiveSlotsUpdate;
        }

        void Injectable.Inject(Resolver resolver)
        {
            Slots = resolver.Resolve<ReadonlyObservableList<Slot>>();
            _filter = resolver.ResolveOptional<Filter>();
            _sorter = resolver.ResolveOptional<Sorter>();
            _accessor = resolver.Resolve<ContainerAccessor>();
            _signal = resolver.ResolveOptional<ContainerChangedSignal>();
            _scheduler = resolver.ResolveOptional<UpdateScheduler>();
        }

        void Initializable.Initialize()
        {
            ScheduleActiveSlotsUpdate();
            if (_filter != null)
                _filter.OnChanged += _scheduleUpdateActiveSlotsAction;
            if (_sorter != null)
                _sorter.OnChanged += _scheduleUpdateActiveSlotsAction;
            if (_signal != null)
                _signal.OnChanged += _scheduleUpdateActiveSlotsAction;
            Slots.OnItemRemoved += OnSlotChanged;
            Slots.OnItemAdded += OnSlotChanged;
            Slots.OnItemReplaced += OnSlotReplaced;
        }

        void Cleanable.Clean()
        {
            if(_scheduler)
                _scheduler.CancelAll();
            if (_filter != null)
                _filter.OnChanged -= _scheduleUpdateActiveSlotsAction;
            if (_sorter != null)
                _sorter.OnChanged -= _scheduleUpdateActiveSlotsAction;
            if (_signal != null)
                _signal.OnChanged -= _scheduleUpdateActiveSlotsAction;
            Slots.OnItemRemoved -= OnSlotChanged;
            Slots.OnItemAdded -= OnSlotChanged;
            Slots.OnItemReplaced -= OnSlotReplaced;
        }

        private void ScheduleActiveSlotsUpdate()
        {
            if (_scheduler != null)
                _scheduler.ScheduleOnce(SchedulerId, _updateActiveSlotsAction);
            else
                UpdateActiveSlots();
        }

        private void UpdateActiveSlots()
        {
            if (_scheduler)
                _scheduler.Cancel(SchedulerId);
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

        private void OnSlotChanged(Slot slot, int index) => ScheduleActiveSlotsUpdate();

        private void OnSlotReplaced(Slot newSlot, Slot formerSlot, int index) => ScheduleActiveSlotsUpdate();
    }
}
