namespace Calluna.Inventory
{
    public class SlotCreator : SlotProvider
    {
        public override Slot Get()
        {
            return new Slot();
        }

        public override void Return(Slot slot)
        {
            
        }
    }
}