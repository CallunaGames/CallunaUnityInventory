using Calluna.DI;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class ItemId : ItemProperty
    {
        public int Value { get; private set; }

        public ItemId(int id)
        {
            Value = id;
        }
    }
}