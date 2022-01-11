using UnityEditor;
using UnityEngine;

namespace Coimbra
{
    [CustomPropertyDrawer(typeof(ScenesCollectionsPair))]
    internal sealed class ScenesCollectionsPairDrawer : PropertyDrawer
    {
        private static class SerializedFields
        {
            public const string Scenes = "m_Scenes";
            public const string Collections = "m_Collections";
        }

        private const float BackgroundSpacing = 6;
        private const float ContentSpacing = 10;
        private const float HeaderHeight = 16;

        private static readonly GUIStyle BackgroundStyle = new GUIStyle("RL Background");
        private static readonly GUIStyle HeaderStyle = new GUIStyle("RL Header");
        private static readonly ReorderableAttribute Attribute = new ReorderableAttribute("", ReorderablePermissions.Everything & ~ReorderablePermissions.Collapse, ReorderableElementDisplay.NonExpandable);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            DrawHeader(position, property, label);

            if (property.isExpanded)
            {
                ReorderableList collections = ReorderableList.GetReorderableList(property.FindPropertyRelative(SerializedFields.Collections), Attribute);
                ReorderableList scenes = ReorderableList.GetReorderableList(property.FindPropertyRelative(SerializedFields.Scenes), Attribute);

                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                position.height = Mathf.Max(scenes.TotalHeight, collections.TotalHeight) + EditorGUIUtility.standardVerticalSpacing * 4;

                if (Event.current.type == EventType.Repaint)
                {
                    BackgroundStyle.Draw(position, false, false, false, false);
                }

                position.xMin += BackgroundSpacing;
                position.xMax -= BackgroundSpacing;
                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.height -= EditorGUIUtility.standardVerticalSpacing * 4;

                using (new LabelWidthScope(-BackgroundSpacing))
                {
                    position.width = position.width / 2 - 1;
                    scenes.DrawGUI(position);

                    position.x += position.width + 2;
                    collections.DrawGUI(position);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                ReorderableList collections = ReorderableList.GetReorderableList(property.FindPropertyRelative(SerializedFields.Collections), Attribute);
                ReorderableList scenes = ReorderableList.GetReorderableList(property.FindPropertyRelative(SerializedFields.Scenes), Attribute);

                height += Mathf.Max(scenes.TotalHeight, collections.TotalHeight);
                height += EditorGUIUtility.standardVerticalSpacing * 3;
            }

            return height;
        }

        private static void DrawHeader(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.Repaint)
            {
                HeaderStyle.Draw(position, false, false, false, false);
            }

            position.xMin += BackgroundSpacing;
            position.xMax -= BackgroundSpacing;
            position.x += ContentSpacing;
            position.width -= ContentSpacing;
            position.y += 1;
            position.height = HeaderHeight;

            using (var propertyScope = new EditorGUI.PropertyScope(position, label, property))
            {
                using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    bool isExpanded = EditorGUI.Foldout(position, property.isExpanded, propertyScope.content, true);

                    if (changeCheckScope.changed)
                    {
                        property.isExpanded = isExpanded;
                    }
                }
            }
        }
    }
}
