using Calluna.DI;
using UnityEngine;
using UnityEngine.UI;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class SorterToggles : MonoBehaviour, Injectable, Initializable, Cleanable
    {
        [SerializeField] private Toggle _id;
        [SerializeField] private Toggle _name;
        [SerializeField] private Toggle _nameCharacterCount;
        
        private RadioSorter _radioSorter;
        private NameSorter _nameSorter;
        private NameCharacterCountSorter _nameCharacterCountSorter;
        private IdSorter _idSorter;
        
        public void Inject(Resolver resolver)
        {
            _radioSorter = resolver.Resolve<RadioSorter>();
            _nameSorter = resolver.Resolve<NameSorter>();
            _nameCharacterCountSorter = resolver.Resolve<NameCharacterCountSorter>();
            _idSorter = resolver.Resolve<IdSorter>();
        }

        public void Initialize()
        {
            _id.onValueChanged.AddListener(OnToggle);
            _name.onValueChanged.AddListener(OnToggle);
            _nameCharacterCount.onValueChanged.AddListener(OnToggle);
            UpdateSorter();
        }

        public void Clean()
        {
            _id.onValueChanged.RemoveListener(OnToggle);
            _name.onValueChanged.RemoveListener(OnToggle);
            _nameCharacterCount.onValueChanged.RemoveListener(OnToggle);
        }

        private void OnToggle(bool arg0)
        {
            UpdateSorter();
        }

        private void UpdateSorter()
        {
            if(_id.isOn)
                _radioSorter.Activate(_idSorter);
            else if (_name.isOn)
                _radioSorter.Activate(_nameSorter);
            else if (_nameCharacterCount.isOn)
                _radioSorter.Activate(_nameCharacterCountSorter);
            else
                _radioSorter.Deactivate();
        }
    }
}
