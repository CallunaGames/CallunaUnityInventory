namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class IdSorter : Sorter<int>
    {
        protected override int GetKey(Slot slot) =>
            slot.Item.HasValue && slot.Item.Value.TryGetProperty(out ItemId id)
                ? id.Value
                : int.MaxValue;
    }
}
