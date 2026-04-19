namespace Calluna.Inventory
{
    public abstract class ItemProperty
    {
        private ContainerChangedSignal _signal;

        internal void SetSignal(ContainerChangedSignal signal) => _signal = signal;

        protected void NotifyChanged() => _signal?.Invoke();
    }
}
