using System;
using System.Collections.Generic;
using Calluna.DI;

namespace Calluna.Inventory
{
    public class Item : Injectable
    {
        private readonly Dictionary<Type, ItemProperty> _properties = new Dictionary<Type, ItemProperty>();
        private ContainerChangedSignal _signal;

        void Injectable.Inject(Resolver resolver)
        {
            _signal = resolver.ResolveOptional<ContainerChangedSignal>();
            foreach (ItemProperty property in _properties.Values)
                property.SetSignal(_signal);
        }

        public bool TryGetProperty<TProperty>(out TProperty property) where TProperty : ItemProperty
        {
            bool found = _properties.TryGetValue(typeof(TProperty), out ItemProperty rawProperty);
            property = found ? (TProperty)rawProperty : null;
            return found;
        }

        public void Add<TProperty>(TProperty property) where TProperty : ItemProperty
        {
            Type type = typeof(TProperty);
            if (_properties.ContainsKey(type))
                throw new Exception($"Property {type.FullName} is already added");
            property.SetSignal(_signal);
            _properties.Add(type, property);
        }

        public void Remove<TProperty>() where TProperty : ItemProperty
        {
            if (_properties.TryGetValue(typeof(TProperty), out ItemProperty property))
            {
                property.SetSignal(null);
                _properties.Remove(typeof(TProperty));
            }
        }
    }
}
