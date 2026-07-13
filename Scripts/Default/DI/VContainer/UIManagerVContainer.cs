#nullable enable
namespace UniT.UI.DI
{
    using Extensions;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using VContainer;
    using VContainer.Unity;

    public static class UIManagerVContainer
    {
        public static void RegisterUIManager(this IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefabResource<Canvas>(nameof(Canvas), Lifetime.Singleton);
            builder.RegisterComponentInNewPrefabResource<EventSystem>(nameof(EventSystem), Lifetime.Singleton);
            builder.Register<UIManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        public static void RegisterUIManager(this IContainerBuilder builder, Canvas canvasPrefab, EventSystem eventSystemPrefab)
        {
            builder.RegisterComponentInNewPrefab(canvasPrefab, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(eventSystemPrefab, Lifetime.Singleton);
            builder.Register<UIManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}