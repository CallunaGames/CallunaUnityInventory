using System;
using Calluna.DI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class AddItemButton : MonoBehaviour, Injectable, Initializable, Cleanable
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_InputField _nameInput;

        private Container _container;
        private Resolver _resolver;

        public void Inject(Resolver resolver)
        {
            _resolver = resolver;
            _container = resolver.Resolve<Container>();
        }

        public void Initialize()
        {
            _button.onClick.AddListener(OnAddClicked);
            _nameInput.onValueChanged.AddListener(OnNameChanged);
            _button.interactable = false;
        }

        public void Clean()
        {
            _button.onClick.RemoveListener(OnAddClicked);
            _nameInput.onValueChanged.RemoveListener(OnNameChanged);
        }

        private void OnNameChanged(string value)
        {
            _button.interactable = !string.IsNullOrEmpty(value);
        }

        private void OnAddClicked()
        {
            if(string.IsNullOrEmpty(_nameInput.text))
                return;

            Item item = _resolver.Resolve<Item>();
            if (!item.TryGetProperty(out ItemName itemName))
                throw new ArgumentException();
            itemName.Name.Value = _nameInput.text.Trim();
            _nameInput.text = string.Empty;

            if(_container.CanAdd(item))
                _container.Add(item);
        }
    }
}
