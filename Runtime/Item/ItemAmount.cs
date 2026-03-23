namespace Calluna.Inventory
{
    public class ItemAmount : ItemProperty
    {
        public Observable<int> Value { get; } = new Observable<int>();
    }
}