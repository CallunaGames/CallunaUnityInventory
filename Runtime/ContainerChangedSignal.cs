using System;

namespace Calluna.Inventory
{
    public class ContainerChangedSignal
    {
        public event Action OnChanged;

        public void Invoke() => OnChanged?.Invoke();
    }
}
