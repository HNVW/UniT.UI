#if UNIT_VCONTAINER
#nullable enable
namespace UniT.UI.DI
{
    using UniT.Logging.DI;
    using UniT.Pooling.DI;
    using UnityEngine.EventSystems;
    using VContainer;

    public static class UIManagerVContainer
    {
        public static void RegisterUIManager(this IContainerBuilder builder)
        {
            if (builder.Exists(typeof(IUIManager), true)) return;
            builder.RegisterDependencyContainer();
            builder.RegisterLoggerManager();
            builder.RegisterObjectPoolManager();
            builder.RegisterComponentInNewPrefabResource<RootUICanvas>(nameof(RootUICanvas), Lifetime.Singleton);
            builder.RegisterComponentInNewPrefabResource<EventSystem>(nameof(EventSystem), Lifetime.Singleton);
            builder.Register<UIManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
#endif