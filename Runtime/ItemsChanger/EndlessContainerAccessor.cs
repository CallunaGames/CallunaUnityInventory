using System;
using System.Linq;

namespace Calluna.Inventory
{
    public class EndlessContainerAccessor : ContainerAccessor
    {
        public override void Add(Item item)
        {
            Slot slot = _slotProvider.Get();
            slot.Item.Value = item;
            _slots.Add(slot);
        }

        public override bool CanAdd(Item item) => true;

        /// <exception cref="InvalidOperationException"><paramref name="item"/> isn't in the container.</exception>
        public override void Remove(Item item)
        {
            Slot slot = _slots.First(s => s.Item.Value == item);
            _slots.Remove(slot);
            _slotProvider.Return(slot);
        }

        public override bool CanRemove(Item item) => _slots.Any(s => s.Item.Value == item);

        [Obsolete("Not used by Container. Will be removed in 2.0.0.")]
        public override bool CanSetAt(Item item, int index) => index < _slots.Count;

        [Obsolete("Not used by Container. Will be removed in 2.0.0.")]
        public override Item this[int index]
        {
            get => _slots[index].Item.Value;
            set => _slots[index].Item.Value = value;
        }
    }
}
