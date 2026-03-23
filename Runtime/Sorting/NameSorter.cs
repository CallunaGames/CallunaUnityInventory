using System;

namespace Calluna.Inventory
{
    public class NameSorter : Sorter<string>
    {
        protected override string GetKey(Slot slot)
        {
            if (!slot.Item.HasValue || !slot.Item.Value.TryGetProperty(out ItemName name))
                return string.Empty;
            return name.Name.Value;
        }
    }
}