#nullable enable
namespace UniT.UI.Utilities
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class NonDrawingGraphic : Graphic
    {
        public override void SetMaterialDirty() { }

        public override void SetVerticesDirty() { }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}