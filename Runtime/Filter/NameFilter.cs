using System;

namespace Calluna.Inventory
{
    public class NameFilter : Filter
    {
        private string _searchText;

        // Without a search text every item matches.
        public override bool ApplyTo(Item item)
        {
            if (string.IsNullOrEmpty(_searchText))
                return true;
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