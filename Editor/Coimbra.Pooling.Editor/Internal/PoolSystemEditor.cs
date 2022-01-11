using UnityEditor;
using UnityEngine;

namespace Coimbra
{
    [CustomEditor(typeof(PoolSystem))]
    internal sealed class PoolSystemEditor : Editor
    {
        private ReorderableList _pools;

        private void OnEnable()
        {
            _pools = new ReorderableList(serializedObject.FindProperty("m_Pools"), PoolGUIUtility.SystemElementName, ReorderablePermissions.ReadOnly, ReorderableElementDisplay.Custom);
            _pools.OnDrawElement += HandlePoolsDrawElement;
            _pools.OnGetElementHeight += HandlePoolsGetElementHeight;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                if (EditorApplication.isPlaying)
                {
                    _pools.DrawGUILayout();
                }

                if (changeCheckScope.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static void HandlePoolsDrawElement(ReorderableList list, Rect position, SerializedProperty element, GUIContent label, bool selected, bool focused)
        {
            PoolDrawer.DrawGUI(position, element, label, false);
        }

        private static float HandlePoolsGetElementHeight(ReorderableList list, SerializedProperty element, int index)
        {
            return PoolDrawer.GetHeight(element, false);
        }
    }
}
