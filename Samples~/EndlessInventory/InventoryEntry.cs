using Calluna.DI;
using TMPro;
using UnityEngine;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class InventoryEntry : MonoBehaviour, Injectable, Initializable, Cleanable
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _id;
        
        private Slot _slot;

        public void Inject(Resolver resolver)
        {
            _slot = resolver.Resolve<Slot>();
        }

        public void Initialize()
        {
            _slot.Item.OnChangedWithValues += OnItemChanged;
            OnItemChanged(null, _slot.Item.Value);
        }

        public void Clean()
        {
            if(_slot.Item.HasValue && _slot.Item.Value.TryGetProperty(out ItemName itemName))
                itemName.Name.OnChangedWithValues -= OnNameChanged;
        }

        private void OnItemChanged(Item formerValue, Item newValue)
        {
            if (formerValue != null)
                Clean(formerValue);
            if (newValue != null)
                Init(newValue);
        }

        private void Init(Item item)
        {
            item.TryGetProperty(out ItemName itemName);
            _name.text = itemName.Name.Value;
            item.TryGetProperty(out ItemId id);
            _id.text ="#" + id.Value;
            itemName.Name.OnChangedWithValues += OnNameChanged;
        }

        private void Clean(Item item)
        {
            item.TryGetProperty(out ItemName itemName);
            itemName.Name.OnChangedWithValues -= OnNameChanged;
        }

        private void OnNameChanged(string formerValue, string newValue)
        {
            _name.text = newValue;
        }
    }
}
