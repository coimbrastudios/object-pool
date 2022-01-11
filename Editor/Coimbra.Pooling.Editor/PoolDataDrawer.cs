using UnityEditor;
using UnityEngine;

namespace Coimbra
{
    [CustomPropertyDrawer(typeof(PoolData))]
    public sealed class PoolDataDrawer : PropertyDrawer
    {
        private const float BackgroundSpacing = 6;
        private const float ContentSpacing = 10;
        private const float HeaderHeight = 16;

        private static readonly GUIContent EmptyLabel = new GUIContent(" ");
        private static readonly GUIContent PrefabErrorLabel = new GUIContent("Prefab should not be null!");
        private static readonly GUIContent DuplicatedWarningLabel = new GUIContent("Prefab is used by another Pool!");
        private static readonly GUIStyle BackgroundStyle = new GUIStyle("RL Background");
        private static readonly GUIStyle HeaderStyle = new GUIStyle("RL Header");

        private static Object _result;
        private static SerializedProperty _asset;
        private static SerializedProperty _overrides;
        private static SerializedProperty _prefab;
        private static SerializedProperty _maxCapacity;
        private static SerializedProperty _allowInfinityInstances;
        private static SerializedProperty _messageType;

        public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label, bool drawBaseAsset)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.HelpBox(position, "Multi-object editing not supported by pools", MessageType.None);

                return;
            }

            _asset = property.FindPropertyRelative(PoolGUIUtility.Asset);
            _overrides = property.FindPropertyRelative(PoolGUIUtility.Overrides);
            _prefab = property.FindPropertyRelative(PoolGUIUtility.Prefab);
            _maxCapacity = property.FindPropertyRelative(PoolGUIUtility.MaxCapacity);
            _allowInfinityInstances = property.FindPropertyRelative(PoolGUIUtility.AllowInfinityInstances);
            _messageType = property.FindPropertyRelative(PoolGUIUtility.MessageType);

            PoolGUIUtility.Validate(property, _asset, _overrides, _allowInfinityInstances, _messageType);

            position.height = EditorGUIUtility.singleLineHeight;
            DrawHeader(position, property, label);

            if (property.isExpanded)
            {
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                position.height = PoolGUIUtility.GetFieldsHeight(_allowInfinityInstances, _messageType) + EditorGUIUtility.standardVerticalSpacing * 4;

                if (drawBaseAsset)
                {
                    position.height += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    BackgroundStyle.Draw(position, false, false, false, false);
                }

                position.xMin += BackgroundSpacing;
                position.xMax -= BackgroundSpacing;
                position.y += EditorGUIUtility.standardVerticalSpacing;
                position.height = EditorGUIUtility.singleLineHeight;

                using (new LabelWidthScope(-BackgroundSpacing))
                {
                    if (drawBaseAsset)
                    {
                        var overrideBoxesPosition = new Rect();

                        if (EditorApplication.isPlaying == false)
                        {
                            PoolGUIUtility.DrawSaveButton(position, property, _asset, _overrides, _prefab);

                            position.y += position.height + EditorGUIUtility.standardVerticalSpacing * 2;

                            if (_asset.objectReferenceValue != null)
                            {
                                overrideBoxesPosition = position;
                                overrideBoxesPosition.width = PoolGUIUtility.ButtonsWidth;
                                PoolGUIUtility.DrawResetButtons(overrideBoxesPosition, _overrides, _allowInfinityInstances, _messageType);
                                overrideBoxesPosition.width += PoolGUIUtility.ButtonsSpacing;
                            }
                        }

                        using (new LabelWidthScope(-overrideBoxesPosition.width))
                        {
                            position.xMin += overrideBoxesPosition.width;

                            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                            {
                                if (PoolGUIUtility.DrawAssetField(position, _asset, out _result))
                                {
                                    _asset.objectReferenceValue = _result;
                                }
                            }

                            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                            PoolGUIUtility.DrawFields(position, property, _overrides, _prefab, _maxCapacity, _allowInfinityInstances, _messageType, false);
                        }
                    }
                    else
                    {
                        PoolGUIUtility.DrawFields(position, property, _overrides, _prefab, _maxCapacity, _allowInfinityInstances, _messageType, false);
                    }
                }
            }
        }

        public static float GetHeight(SerializedProperty property, bool includeBaseAsset)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                height += PoolGUIUtility.GetFieldsHeight(property.FindPropertyRelative(PoolGUIUtility.AllowInfinityInstances), property.FindPropertyRelative(PoolGUIUtility.MessageType)) + EditorGUIUtility.standardVerticalSpacing * 3;

                if (includeBaseAsset)
                {
                    height += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 3;

                    if (EditorApplication.isPlaying)
                    {
                        height -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
                    }
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawGUI(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight(property, true);
        }

        private static void DrawHeader(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_prefab.objectReferenceValue == null)
            {
                using (new BackgroundColorScope(Color.red))
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        HeaderStyle.Draw(position, false, false, false, false);
                    }
                }

                Rect helpPosition = position;
                helpPosition.y += 2;
                helpPosition.width -= 3;
                helpPosition.height = HeaderHeight - 1;
                EditorGUI.LabelField(helpPosition, EmptyLabel, PrefabErrorLabel, EditorStyles.helpBox);
            }
            else if (property.FindPropertyRelative(PoolGUIUtility.IsDuplicated).boolValue)
            {
                using (new BackgroundColorScope(Color.yellow))
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        HeaderStyle.Draw(position, false, false, false, false);
                    }
                }

                Rect helpPosition = position;
                helpPosition.y += 2;
                helpPosition.width -= 3;
                helpPosition.height = HeaderHeight - 1;
                EditorGUI.LabelField(helpPosition, EmptyLabel, DuplicatedWarningLabel, EditorStyles.helpBox);
            }
            else
            {
                if (Event.current.type == EventType.Repaint)
                {
                    HeaderStyle.Draw(position, false, false, false, false);
                }
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
