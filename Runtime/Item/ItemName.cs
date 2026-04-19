using Calluna.DI;

namespace Calluna.Inventory
{
    public class ItemName : ItemProperty, Initializable, Cleanable
    {
        public Observable<string> Name { get; } = new Observable<string>();

        void Initializable.Initialize()
        {
            Name.OnChanged += NotifyChanged;
        }

        void Cleanable.Clean()
        {
            Name.OnChanged -= NotifyChanged;
        }
    }
}
