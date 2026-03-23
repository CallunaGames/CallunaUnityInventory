using System.Linq;

namespace Calluna.Inventory
{
    public class EndlessContainerAccessor : ItemsAccessor
    {
        public override void Add(Item item)
        {
            Slot slot = _slotProvider.Get();
            slot.Set(item);
            _slots.Add(slot);
        }

        public override bool CanAdd(Item item) => true;

        public override void Remove(Item item)
        {
            Slot slot = _slots.First(s => s.Item.Value == item);
            _slots.Remove(slot);
            _slotProvider.Return(slot);
        }

        public override bool CanRemove(Item item) => _slots.Any(s => s.Item.Value == item);

        public override bool CanSetAt(Item item, int index) => index < _slots.Count;

        public override Item this[int index]
        {
            get => _slots[index].Item.Value;
            set => _slots[index].Set(value);
        }
    }
}