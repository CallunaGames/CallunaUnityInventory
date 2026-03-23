namespace Calluna.Inventory
{
    public abstract class SlotProvider
    {
        public abstract Slot Get();
        public abstract void Return(Slot slot);
    }
}