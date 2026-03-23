using System;
using UnityEngine;

namespace Calluna.Inventory
{
    [CreateAssetMenu(fileName = "ItemId", menuName = "Settings/Inventory/ItemId")]
    public class ItemId : ScriptableObjectId, IEquatable<ItemId>
    {
        public bool Equals(ItemId other)
        {
            return ReferenceEquals(this, other);
        }
    }
}
