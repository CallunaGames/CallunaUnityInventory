namespace Calluna.Inventory
{
    public class ItemName : ItemProperty
    {
        public Observable<string> Name { get; } = new Observable<string>();

        public ItemName()
        {
            // Subscribed to its own observable, which lives exactly as long as this property - so it
            // never has to be unsubscribed, and works without DI.
            Name.OnChanged += NotifyChanged;
        }
    }
}
