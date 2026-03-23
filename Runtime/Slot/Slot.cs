

namespace Calluna.Inventory
{
    public class Slot
    {
        public Observable<Item> Item { get; } = new Observable<Item>();
    }
}
