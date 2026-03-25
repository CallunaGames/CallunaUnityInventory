using System.Collections.Generic;
using System.Linq;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class NameCharacterCountSorter : Sorter
    {
        public override IOrderedEnumerable<Slot> Sort(IEnumerable<Slot> items)
        {
            return items.OrderBy(GetKey);
        }

        public override IOrderedEnumerable<Slot> ThenBy(IOrderedEnumerable<Slot> items)
        {
            return items.ThenBy(GetKey);
        }

        private int GetKey(Slot slot)
        {
            if (slot.Item.HasValue && slot.Item.Value.TryGetProperty(out ItemName name))
                return name.Name.HasValue ? name.Name.Value.Length : 0;
            return int.MaxValue;
        }
    }
}