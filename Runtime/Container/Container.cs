using System;
using System.Collections.Generic;
using Calluna.DI;

namespace Calluna.Inventory
{
    public class Container : Injectable, Initializable, Cleanable
    {
        private ContainerAccessor _accessor;
        private ContainerChangedSignal _signal;
        private UpdateScheduler _scheduler;

        public ReadonlyObservableList<Slot> Slots { get; private set; }

        /// <summary>
        /// The filtered and sorted view of <see cref="Slots"/>. It is updated with the individual
        /// changes (added, removed, replaced, swapped slots) rather than replaced as a whole, so
        /// subscribers - e.g. a scroll view - only update what actually changed.
        /// </summary>
        public ReadonlyObservableList<Slot> ActiveSlots => _activeSlots;
        private readonly ObservableList<Slot> _activeSlots = new ObservableList<Slot>();

        public bool CanAdd(Item item) => _accessor.CanAdd(item);
        public bool CanRemove(Item item) => _accessor.CanRemove(item);
        public void Add(Item item) => _accessor.Add(item);
        public void Remove(Item item) => _accessor.Remove(item);

        private Filter _filter;
        private Sorter _sorter;
        private IDisposable _slotsSubscription;

        // Cached delegates: reused for every (un)subscribe and every ScheduleOnce, which also keys
        // the scheduled update by this delegate - so several changes in a frame update once.
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
            // Any change - including a swap, Clear or OverrideWith of the slots - can change the view.
            _slotsSubscription = Slots.SubscribeAny(_scheduleUpdateActiveSlotsAction);
        }

        void Cleanable.Clean()
        {
            // Only this container's update - the scheduler may be shared with other classes.
            if (_scheduler)
                _scheduler.Cancel(_updateActiveSlotsAction);
            if (_filter != null)
                _filter.OnChanged -= _scheduleUpdateActiveSlotsAction;
            if (_sorter != null)
                _sorter.OnChanged -= _scheduleUpdateActiveSlotsAction;
            if (_signal != null)
                _signal.OnChanged -= _scheduleUpdateActiveSlotsAction;
            _slotsSubscription?.Dispose();
            _slotsSubscription = null;
        }

        private void ScheduleActiveSlotsUpdate()
        {
            if (_scheduler != null)
                _scheduler.ScheduleOnce(_updateActiveSlotsAction);
            else
                UpdateActiveSlots();
        }

        private void UpdateActiveSlots()
        {
            if (_scheduler)
                _scheduler.Cancel(_updateActiveSlotsAction);
            _activeSlots.OverrideWithEvents(ApplySorting(ApplyFilters(Slots)));
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
