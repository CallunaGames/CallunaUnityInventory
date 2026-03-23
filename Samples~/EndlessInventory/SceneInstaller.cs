using Calluna.DI;
using Calluna.Inventory;

namespace Calluna.Process.Samples.EndlessInventory
{
    public class SceneInstaller : MonoInstaller, Injectable
    {
        private Resolver _resolver;

        public void Inject(Resolver resolver)
        {
            _resolver = resolver;
        }

        public override void InstallBindings(Binder binder)
        {
            binder.Bind<Filter>()
                .And<NameFilter>()
                .ToNew<NameFilter>()
                .AsSingle();

            binder.Bind<Sorter>()
                .ToNew<NameSorter>()
                .AsSingle();

            binder.BindToNewSelf<ItemName>()
                .PerRequest();
            
            binder.Bind<ObservableList<Slot>>()
                .And<ReadonlyObservableList<Slot>>()
                .ToNew<ObservableList<Slot>>()
                .AsSingle();

            binder.BindToSelf<Item>()
                .FromMethod(CreateItem)
                .PerRequest();

            binder.BindToNewSelf<Container>()
                .AsSingle();
            
            binder.Bind<ContainerAccessor>()
                .ToNew<EndlessContainerAccessor>()
                .AsSingle();
            
            binder.Bind<SlotProvider>()
                .ToNew<SlotCreator>()
                .AsSingle();
        }

        private Item CreateItem()
        {
            Item item = new Item();
            item.Add(_resolver.Resolve<ItemName>());
            return item;
        }
    }
}