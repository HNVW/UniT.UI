#if UNIT_ZENJECT
#nullable enable
namespace UniT.UI.DI
{
    using UniT.Logging.DI;
    using UniT.Pooling.DI;
    using UnityEngine.EventSystems;
    using Zenject;

    public static class UIManagerZenject
    {
        public static void BindUIManager(this DiContainer container)
        {
            if (container.HasBinding<IUIManager>()) return;
            container.BindDependencyContainer();
            container.BindLoggerManager();
            container.BindObjectPoolManager();
            container.Bind<RootUICanvas>().FromComponentInNewPrefabResource(nameof(RootUICanvas)).AsSingle();
            container.Bind<EventSystem>().FromComponentInNewPrefabResource(nameof(EventSystem)).AsSingle();
            container.BindInterfacesTo<UIManager>().AsSingle();
        }
    }
}
#endif