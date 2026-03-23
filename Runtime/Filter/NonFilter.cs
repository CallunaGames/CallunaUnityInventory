using System;

namespace Calluna.Inventory
{
    public class NonFilter : Filter
    {
        public override bool ApplyTo(Item item)
        {
            return true;
        }
    }
}