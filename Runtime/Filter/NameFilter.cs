using System;

namespace Calluna.Inventory
{
    public class NameFilter : Filter
    {
        private string _searchText;

        public override bool ApplyTo(Item item)
        {
            return item != null && item.TryGetProperty(out ItemName name) &&
                   name.Name.Value.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
        }

        public void SetSearchText(string searchText)
        {
            _searchText = searchText;
            InvokeOnChanged();
        }
    }
}