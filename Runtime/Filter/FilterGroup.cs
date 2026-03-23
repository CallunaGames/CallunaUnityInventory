using System;
using System.Collections.Generic;
using System.Linq;

namespace Calluna.Inventory
{
    public class FilterGroup : Filter
    {
        private readonly List<Filter> _filters = new List<Filter>();

        public FilterGroup()
        {
        }

        public FilterGroup(IEnumerable<Filter> filters)
        {
            _filters.AddRange(filters);
            InitFilters();
        }

        public override void Clean()
        {
            base.Clean();
            RemoveListeners();
        }

        public override bool ApplyTo(Item item)
        {
            return _filters.All(filter => !filter.IsActive.Value || filter.ApplyTo(item));
        }

        public void Add(Filter filter)
        {
            _filters.Add(filter);
            AddListener(filter);
            InvokeOnChanged();
        }

        public void Remove(Filter filter)
        {
            _filters.Add(filter);
            RemoveListener(filter);
            InvokeOnChanged();
        }

        private void InitFilters()
        {
            foreach (Filter filter in _filters)
            {
                AddListener(filter);
            }
        }

        private void RemoveListeners()
        {
            foreach (Filter filter in _filters)
            {
                RemoveListener(filter);
            }
        }

        private void AddListener(Filter filter)
        {
            filter.OnChanged += InvokeOnChanged;
            filter.IsActive.OnChanged += InvokeOnChanged;
        }

        private void RemoveListener(Filter filter)
        {
            filter.OnChanged -= InvokeOnChanged;
            filter.IsActive.OnChanged -= InvokeOnChanged;
        }
    }
}