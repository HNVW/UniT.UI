#nullable enable
namespace UniT.UI.Utilities.Editor
{
    using UnityEditor;
    using UnityEditor.UI;

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
}