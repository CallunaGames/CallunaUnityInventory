using System;

namespace Calluna.Inventory
{
    public class NameFilter : Filter
    {
        private string _filter;

        public override bool ApplyTo(Item item)
        {
            return item.TryGetProperty(out ItemName name) && 
                   name.Name.Value.Contains(_filter, StringComparison.OrdinalIgnoreCase);
        }

        public void SetFilterString(string filter)
        {
            _filter = filter;
            InvokeOnChanged();
        }
    }
}