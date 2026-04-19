using System;

namespace Calluna.Inventory
{
    public class ContainerChangedSignal
    {
        public event Action OnChanged;

        internal void Invoke() => OnChanged?.Invoke();
    }
}
