using Calluna.DI;

namespace Calluna.Inventory.Samples.EndlessInventory
{
    public class SceneInstaller : MonoInstaller, Injectable
    {
        private Resolver _resolver;
        private int _id;

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

            binder.BindToNewSelf<NameSorter>()
                .AsSingle();

            binder.BindToNewSelf<IdSorter>()
                .AsSingle();

            binder.BindToSelf<Sorter>()
                .FromMethod(CreateSorter)
                .AsSingle();

            binder.BindToNewSelf<LayeredSorter>()
                .PerRequest();

            binder.BindToNewSelf<ItemName>()
                .PerRequest();

            binder.BindToSelf<ItemId>()
                .FromMethod(CreateItemId)
                .WithoutInjection()
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

        private Sorter CreateSorter()
        {
            LayeredSorter layeredSorter = _resolver.Resolve<LayeredSorter>();
            layeredSorter.Add(_resolver.Resolve<NameSorter>());
            layeredSorter.Add(_resolver.Resolve<IdSorter>());
            return layeredSorter;
        }

        private Item CreateItem()
        {
            Item item = new Item();
            item.Add(_resolver.Resolve<ItemName>());
            item.Add(_resolver.Resolve<ItemId>());
            return item;
        }

        private ItemId CreateItemId()
        {
            ItemId itemId = new ItemId();
            ArgumentsResolver resolver = new ArgumentsResolver(_resolver);
            resolver.AddArgument(_id);
            itemId.Inject(resolver);
            _id++;
            return itemId;
        }
    }
}