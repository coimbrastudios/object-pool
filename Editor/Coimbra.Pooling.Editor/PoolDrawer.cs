using UnityEditor;
using UnityEngine;

namespace Coimbra
{
    [CustomPropertyDrawer(typeof(Pool))]
    public sealed class PoolDrawer : PropertyDrawer
    {
        private const float BackgroundSpacing = 6;
        private const float ContentSpacing = 10;
        private const float HeaderHeight = 16;
        private const string IsActive = "_isActive";
        private const string ContainerMode = "_containerMode";
        private const string Container = "_container";
        private const string AvailableCount = "m_AvailableCount";
        private const string TotalCount = "m_TotalCount";

        private static readonly GUIContent EmptyLabel = new GUIContent(" ");
        private static readonly GUIContent PrefabErrorLabel = new GUIContent("Prefab should not be null!");
        private static readonly GUIContent DuplicatedWarningLabel = new GUIContent("Prefab is used by another Pool!");
        private static readonly GUIContent ContainerModeLabel = new GUIContent("Container Mode", "Where to keep the available instances:\n\n" + "Automatic: container is automatically created.\n" + "Manual: container is manually chosen.");
        private static readonly GUIStyle BackgroundStyle = new GUIStyle("RL Background");
        private static readonly GUIStyle HeaderStyle = new GUIStyle("RL Header");

        private static Object _result;
        private static SerializedProperty _isActive;
        private static SerializedProperty _data;
        private static SerializedProperty _asset;
        private static SerializedProperty _overrides;
        private static SerializedProperty _prefab;
        private static SerializedProperty _maxCapacity;
        private static SerializedProperty _allowInfinityInstances;
        private static SerializedProperty _messageType;
        private static SerializedProperty _containerMode;

        public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label, bool drawBaseAsset)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.HelpBox(position, "Multi-object editing not supported by pools", MessageType.None);

                return;
            }

            _isActive = property.FindPropertyRelative(IsActive);
            _data = property.FindPropertyRelative(PoolGUIUtility.Data);
            _containerMode = property.FindPropertyRelative(ContainerMode);
            _asset = _data.FindPropertyRelative(PoolGUIUtility.Asset);
            _overrides = _data.FindPropertyRelative(PoolGUIUtility.Overrides);
            _prefab = _data.FindPropertyRelative(PoolGUIUtility.Prefab);
            _maxCapacity = _data.FindPropertyRelative(PoolGUIUtility.MaxCapacity);
            _allowInfinityInstances = _data.FindPropertyRelative(PoolGUIUtility.AllowInfinityInstances);
            _messageType = _data.FindPropertyRelative(PoolGUIUtility.MessageType);

            PoolGUIUtility.Validate(_data, _asset, _overrides, _allowInfinityInstances, _messageType);

            position.height = EditorGUIUtility.singleLineHeight;
            DrawHeader(position, property, label);

            if (property.isExpanded)
            {
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                position.height = PoolGUIUtility.GetButtonsHeight(_allowInfinityInstances, _messageType) + EditorGUIUtility.standardVerticalSpacing * 4;

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
                        Rect overrideBoxesPosition = new Rect();

                        if (EditorApplication.isPlaying == false)
                        {
                            PoolGUIUtility.DrawSaveButton(position, _data, _asset, _overrides, _prefab);

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
                            DrawFields(position, property);
                        }
                    }
                    else
                    {
                        DrawFields(position, property);
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
                _data = property.FindPropertyRelative(PoolGUIUtility.Data);
                height += PoolGUIUtility.GetFieldsHeight(_data.FindPropertyRelative(PoolGUIUtility.AllowInfinityInstances), _data.FindPropertyRelative(PoolGUIUtility.MessageType)) + EditorGUIUtility.standardVerticalSpacing * 3;
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

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

        private static void DrawFields(Rect position, SerializedProperty property)
        {
            PoolGUIUtility.DrawFields(position, _data, _overrides, _prefab, _maxCapacity, _allowInfinityInstances, _messageType, _isActive.boolValue);

            using (new EditorGUI.DisabledScope(_isActive.boolValue))
            {
                position.y += PoolGUIUtility.GetFieldsHeight(_allowInfinityInstances, _messageType) + EditorGUIUtility.standardVerticalSpacing;

                if (_containerMode.intValue != (int)PoolContainerMode.Automatic)
                {
                    Rect fieldPosition = EditorGUI.PrefixLabel(position, ContainerModeLabel);

                    using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
                    {
                        float totalWidth = fieldPosition.width;

                        fieldPosition.width = totalWidth * 0.3f;
                        EditorGUI.PropertyField(fieldPosition, _containerMode, GUIContent.none);

                        fieldPosition.x += fieldPosition.width + 2;
                        fieldPosition.width = totalWidth - fieldPosition.width - 2;
                        EditorGUI.PropertyField(fieldPosition, property.FindPropertyRelative(Container), GUIContent.none);
                    }
                }
                else
                {
                    EditorGUI.PropertyField(position, _containerMode, ContainerModeLabel);
                }
            }
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
            else if (_data.FindPropertyRelative(PoolGUIUtility.IsDuplicated).boolValue)
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

            var headerLabel = new GUIContent(label.text, label.tooltip);

            if (_isActive.boolValue)
            {
                SerializedProperty totalCount = property.FindPropertyRelative(TotalCount);
                headerLabel.text = string.Format("{0} [{1}/{2}]", label.text, property.FindPropertyRelative(AvailableCount).intValue, _allowInfinityInstances.boolValue ? totalCount.intValue : Mathf.Min(totalCount.intValue, _maxCapacity.intValue));
            }

            using (var propertyScope = new EditorGUI.PropertyScope(position, headerLabel, property))
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
