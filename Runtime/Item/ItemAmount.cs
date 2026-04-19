using Calluna.DI;

namespace Calluna.Inventory
{
    public class ItemAmount : ItemProperty, Initializable, Cleanable
    {
        public Observable<int> Value { get; } = new Observable<int>();

        void Initializable.Initialize()
        {
            Value.OnChanged += NotifyChanged;
        }

        void Cleanable.Clean()
        {
            Value.OnChanged -= NotifyChanged;
        }
    }
}
