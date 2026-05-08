#nullable enable
namespace UniT.UI
{
    using UnityEngine;

    public sealed class RootUICanvas : MonoBehaviour
    {
        public Transform Screens       { get; private set; } = null!;
        public Transform Popups        { get; private set; } = null!;
        public Transform Overlays      { get; private set; } = null!;
        public Transform OverlayPopups { get; private set; } = null!;

        private void Awake()
        {
            this.Screens       = this.CreateChild(nameof(this.Screens));
            this.Popups        = this.CreateChild(nameof(this.Popups));
            this.Overlays      = this.CreateChild(nameof(this.Overlays));
            this.OverlayPopups = this.CreateChild(nameof(this.OverlayPopups));
        }

        private Transform CreateChild(string name)
        {
            var child = new GameObject(name).AddComponent<RectTransform>();
            child.SetParent(this.transform, false);
            child.anchorMin = Vector2.zero;
            child.anchorMax = Vector2.one;
            child.sizeDelta = Vector2.zero;
            return child;
        }
    }
}