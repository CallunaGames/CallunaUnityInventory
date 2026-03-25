using Calluna.DI;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class ItemId : ItemProperty, Injectable
    {
        public int Value { get; private set; }
        
        public void Inject(Resolver resolver)
        {
            Value = resolver.Resolve<int>();
        }
    }
}