

namespace Calluna.Inventory
{
    public class Slot
    {
        public ReadonlyObservable<Item> Item => _item;
        private Observable<Item> _item = new Observable<Item>();
        
        public void Set(Item item) => _item = item;
    }
}
