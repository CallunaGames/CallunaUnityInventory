using System;
using System.Collections.Generic;
using System.Linq;

namespace Calluna.Inventory
{
    public class NonSorter : Sorter
    {
        public override bool IsActive => false;

        public override IOrderedEnumerable<Slot> Sort(IEnumerable<Slot> items)
        {
            throw new InvalidOperationException();
        }

        public override IOrderedEnumerable<Slot> ThenBy(IOrderedEnumerable<Slot> items)
        {
            return items;
        }
    }
}