using System.Collections.Generic;
using Calluna.DI;
using UnityEngine;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class EntryCreator : MonoBehaviour, Injectable, Initializable, Cleanable
    {
        [SerializeField] private RectTransform _hook;

        private Pool<InventoryEntry, Slot, PrefabInstantiationArguments> _itemPool;
        private Container _container;

        private List<InventoryEntry> _entries = new List<InventoryEntry>();
        private PrefabInstantiationArguments _prefabInstantiationArguments;

        public void Inject(Resolver resolver)
        {
            _itemPool = resolver.Resolve<Pool<InventoryEntry, Slot, PrefabInstantiationArguments>>();
            _container = resolver.Resolve<Container>();
            _prefabInstantiationArguments = PrefabInstantiationArguments.CreateUIArgs(_hook);
        }

        public void Initialize()
        {
            _container.ActiveSlots.OnContentsReplaced += OnContentsReplaced;
            CreateSlots();
        }

        public void Clean()
        {
            _container.ActiveSlots.OnContentsReplaced -= OnContentsReplaced;
            ClearSlots();
        }

        private void OnContentsReplaced()
        {
            ClearSlots();
            CreateSlots();
        }

        private void CreateSlots()
        {
            foreach (Slot slot in _container.ActiveSlots)
            {
                InventoryEntry entry = _itemPool.Request(slot, _prefabInstantiationArguments);
                _entries.Add(entry);
            }
        }

        private void ClearSlots()
        {
            foreach (InventoryEntry item in _entries)
            {
                _itemPool.Return(item);
            }

            _entries.Clear();
        }
    }
}