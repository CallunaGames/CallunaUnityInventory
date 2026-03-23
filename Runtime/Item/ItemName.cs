using System;

namespace Calluna.Inventory
{
    public class ItemName : ItemProperty
    {
        public Observable<string> Name { get; private set; } = new Observable<string>();
    }
}