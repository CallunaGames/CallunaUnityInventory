namespace Calluna.Inventory
{
    public abstract class ItemProperty
    {
        private ContainerChangedSignal _signal;

        internal void SetSignal(ContainerChangedSignal signal) => _signal = signal;

        protected virtual void NotifyChanged() => _signal?.Invoke();
    }
}
