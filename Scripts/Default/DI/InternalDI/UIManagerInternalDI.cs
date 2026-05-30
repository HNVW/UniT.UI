#nullable enable
namespace UniT.UI.Default.DI
{
    using UniT.DI;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public static class UIManagerInternalDI
    {
        public static void AddUIManager(this DependencyContainer container)
        {
            container.AddFromComponentInNewPrefabResource<Canvas>(nameof(Canvas));
            container.AddFromComponentInNewPrefabResource<EventSystem>(nameof(EventSystem));
            container.AddInterfaces<UIManager>();
        }

        public static void AddUIManager(this DependencyContainer container, Canvas canvasPrefab, EventSystem eventSystemPrefab)
        {
            container.AddFromComponentInNewPrefab(canvasPrefab);
            container.AddFromComponentInNewPrefab(eventSystemPrefab);
            container.AddInterfaces<UIManager>();
        }
    }
}