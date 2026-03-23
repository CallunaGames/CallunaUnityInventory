using System;
using System.Collections.Generic;

namespace Calluna.Inventory
{
    public class Item
    {
        private Dictionary<Type, ItemProperty> _properties = new Dictionary<Type, ItemProperty>();
        
        public bool TryGetProperty<TProperty>(out TProperty property) where TProperty : ItemProperty
        {
            if (!_properties.TryGetValue(typeof(TProperty), out ItemProperty value))
            {
                property = null;
                return false;
            }
            property = (TProperty)value;
            return true;
        }

        public void Add<TProperty>(TProperty property) where TProperty : ItemProperty
        {
            Type type = typeof(TProperty);
            if(_properties.ContainsKey(type))
                throw new Exception($"Property {type.FullName} is already added");
            _properties.Add(typeof(TProperty), property);
        }

        public void Remove<TProperty>() where TProperty : ItemProperty
        {
            _properties.Remove(typeof(TProperty));
        }
    }
}