#if UNIT_DI
#nullable enable
namespace UniT.UI.DI
{
    using UniT.DI;
    using UniT.Logging.DI;
    using UniT.Pooling.DI;
    using UnityEngine.EventSystems;

    public static class UIManagerDI
    {
        public static void AddUIManager(this DependencyContainer container)
        {
            if (container.Contains<IUIManager>()) return;
            container.AddLoggerManager();
            container.AddObjectPoolManager();
            container.AddFromComponentInNewPrefabResource<RootUICanvas>(nameof(RootUICanvas));
            container.AddFromComponentInNewPrefabResource<EventSystem>(nameof(EventSystem));
            container.AddInterfaces<UIManager>();
        }
    }
}
#endif