namespace Calluna.Inventory
{
    public class NameSorter : Sorter<string>
    {
        // Empty slots or items without a name sort before all named items.
        protected override string GetKey(Slot slot) =>
            slot.Item.HasValue && slot.Item.Value.TryGetProperty(out ItemName name)
                ? name.Name.Value
                : string.Empty;
    }
}