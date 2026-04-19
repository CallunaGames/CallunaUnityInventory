using Calluna.DI;
using UnityEngine;
using UnityEngine.UI;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class RemoveItemButton : MonoBehaviour, Injectable, Initializable, Cleanable
    {
        [SerializeField] private Button _button;

        private Container _container;
        private Slot _slot;

        public void Inject(Resolver resolver)
        {
            _container = resolver.Resolve<Container>();
            _slot = resolver.Resolve<Slot>();
        }

        public void Initialize()
        {
            _button.onClick.AddListener(OnRemoveClicked);
            _slot.Item.OnChanged += OnSlotItemChanged;
            UpdateInteractable();
        }

        public void Clean()
        {
            _button.onClick.RemoveListener(OnRemoveClicked);
            _slot.Item.OnChanged -= OnSlotItemChanged;
        }

        private void OnSlotItemChanged()
        {
            UpdateInteractable();
        }

        private void UpdateInteractable()
        {
            _button.interactable = _slot.Item.HasValue && _container.CanRemove(_slot.Item.Value);
        }

        private void OnRemoveClicked()
        {
            _container.Remove(_slot.Item.Value);
        }
    }
}
