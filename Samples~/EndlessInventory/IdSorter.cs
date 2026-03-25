using System.Collections.Generic;
using System.Linq;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class IdSorter : Sorter
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
            if (slot.Item.HasValue && slot.Item.Value.TryGetProperty(out ItemId id))
                return id.Value;
            return int.MaxValue;
        }
    }
}