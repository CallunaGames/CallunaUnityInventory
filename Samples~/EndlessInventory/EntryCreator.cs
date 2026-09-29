using System;
using System.Collections.Generic;
using Calluna.DI;
using UnityEngine;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    /// <summary>
    /// Keeps one entry per active slot. ActiveSlots reports its individual changes, so only the
    /// affected entries are created, returned or moved - the rest stay untouched.
    /// </summary>
    public class EntryCreator : MonoBehaviour, Injectable, Initializable, Cleanable
    {
        [SerializeField] private RectTransform _hook;

        private Pool<InventoryEntry, Slot, PrefabInstantiationArguments> _itemPool;
        private Container _container;
        private IDisposable _activeSlotsSubscription;

        private readonly List<InventoryEntry> _entries = new List<InventoryEntry>();
        private PrefabInstantiationArguments _prefabInstantiationArguments;

        public void Inject(Resolver resolver)
        {
            _itemPool = resolver.Resolve<Pool<InventoryEntry, Slot, PrefabInstantiationArguments>>();
            _container = resolver.Resolve<Container>();
            _prefabInstantiationArguments = PrefabInstantiationArguments.CreateUIArgs(_hook);
        }

        public void Initialize()
        {
            CreateEntries();
            _activeSlotsSubscription = _container.ActiveSlots.Subscribe(
                added: OnSlotAdded,
                removed: OnSlotRemoved,
                replaced: OnSlotReplaced,
                swapped: OnSlotsSwapped,
                reset: Rebuild);
        }

        public void Clean()
        {
            _activeSlotsSubscription?.Dispose();
            _activeSlotsSubscription = null;
            ClearEntries();
        }

        private void OnSlotAdded(Slot slot, int index)
        {
            InventoryEntry entry = _itemPool.Request(slot, _prefabInstantiationArguments);
            _entries.Insert(index, entry);
            entry.transform.SetSiblingIndex(index);
        }

        private void OnSlotRemoved(Slot slot, int index)
        {
            _itemPool.Return(_entries[index]);
            _entries.RemoveAt(index);
        }

        private void OnSlotReplaced(Slot newSlot, Slot formerSlot, int index)
        {
            _itemPool.Return(_entries[index]);
            InventoryEntry entry = _itemPool.Request(newSlot, _prefabInstantiationArguments);
            _entries[index] = entry;
            entry.transform.SetSiblingIndex(index);
        }

        private void OnSlotsSwapped(Slot slot1, int index1, Slot slot2, int index2)
        {
            (_entries[index1], _entries[index2]) = (_entries[index2], _entries[index1]);
            _entries[index1].transform.SetSiblingIndex(index1);
            _entries[index2].transform.SetSiblingIndex(index2);
        }

        private void Rebuild()
        {
            ClearEntries();
            CreateEntries();
        }

        private void CreateEntries()
        {
            foreach (Slot slot in _container.ActiveSlots)
            {
                InventoryEntry entry = _itemPool.Request(slot, _prefabInstantiationArguments);
                _entries.Add(entry);
            }
        }

        private void ClearEntries()
        {
            foreach (InventoryEntry entry in _entries)
            {
                _itemPool.Return(entry);
            }

            _entries.Clear();
        }
    }
}
