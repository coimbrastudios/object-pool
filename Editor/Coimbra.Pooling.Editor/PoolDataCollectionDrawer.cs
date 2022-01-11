using UnityEditor;
using UnityEngine;

namespace Coimbra
{
    [CustomPropertyDrawer(typeof(PoolDataCollection))]
    public sealed class PoolDataCollectionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (var propertyScope = new EditorGUI.PropertyScope(position, label, property))
            {
                using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    Object value = EditorGUI.ObjectField(position, propertyScope.content, property.objectReferenceValue, typeof(PoolDataCollection), false);

                    if (changeCheckScope.changed)
                    {
                        property.objectReferenceValue = value;
                    }
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
