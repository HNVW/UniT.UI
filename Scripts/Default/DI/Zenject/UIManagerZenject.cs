#nullable enable
namespace UniT.UI.DI
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Zenject;

    public static class UIManagerZenject
    {
        public static void BindUIManager(this DiContainer container)
        {
            container.Bind<Canvas>().FromComponentInNewPrefabResource(nameof(Canvas)).AsSingle();
            container.Bind<EventSystem>().FromComponentInNewPrefabResource(nameof(EventSystem)).AsSingle();
            container.BindInterfacesTo<UIManager>().AsSingle();
        }

        public static void BindUIManager(this DiContainer container, Canvas canvasPrefab, EventSystem eventSystemPrefab)
        {
            container.Bind<Canvas>().FromComponentInNewPrefab(canvasPrefab).AsSingle();
            container.Bind<EventSystem>().FromComponentInNewPrefab(eventSystemPrefab).AsSingle();
            container.BindInterfacesTo<UIManager>().AsSingle();
        }
    }
}