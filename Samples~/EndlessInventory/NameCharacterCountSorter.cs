namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class NameCharacterCountSorter : Sorter<int>
    {
        protected override int GetKey(Slot slot) =>
            slot.Item.HasValue && slot.Item.Value.TryGetProperty(out ItemName name)
                ? (name.Name.HasValue ? name.Name.Value.Length : 0)
                : int.MaxValue;
    }
}
