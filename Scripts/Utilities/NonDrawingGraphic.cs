#nullable enable
namespace UniT.UI.Utilities
{
    using UnityEngine;
    using UnityEngine.UI;
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.UI;
    #endif

    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class NonDrawingGraphic : Graphic
    {
        public override void SetMaterialDirty() { }

        public override void SetVerticesDirty() { }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }

    #if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NonDrawingGraphic))]
    internal sealed class NonDrawingGraphicEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            EditorGUILayout.PropertyField(this.m_Script);
            this.RaycastControlsGUI();
            this.serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}