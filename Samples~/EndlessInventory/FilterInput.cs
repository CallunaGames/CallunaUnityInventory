using Calluna.DI;
using TMPro;
using UnityEngine;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class FilterInput : MonoBehaviour, Injectable, Initializable, Cleanable
    {
        [SerializeField] private TMP_InputField _inputField;
        
        private NameFilter _filter;
        
        public void Inject(Resolver resolver)
        {
            _filter = resolver.Resolve<NameFilter>();
        }

        public void Initialize()
        {
            _inputField.onSubmit.AddListener(OnSubmit);
            OnSubmit(_inputField.text);
        }

        public void Clean()
        {
            _inputField.onSubmit.RemoveListener(OnSubmit);
        }

        private void OnSubmit(string value)
        {
            _filter.IsActive.Value = !string.IsNullOrEmpty(value);
            _filter.SetSearchText(value);
        }
    }
}