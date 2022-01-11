using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UIElements;

namespace Coimbra
{
    internal sealed class PoolSystemSettingsProvider : SettingsProvider
    {
        private struct Styles
        {
            public static readonly GUIContent TemporaryCollections = new GUIContent("Temporary Collections", "Pools temporarily loaded as needed.");
            public static readonly GUIContent PersistentCollections = new GUIContent("Persistent Collections", "Persistent pools available from the very beginning.");
            public static readonly GUIContent SearchPriority = new GUIContent("Search Priority", "Which pair should be iterated first.");
            public static readonly GUIContent SearchMode = new GUIContent("Search Mode", "Whether it should iterate through all pairs even after a match or not.");
            public static readonly GUIContent FoldersCollectionsPairs = new GUIContent("Folders Collections Pairs", "Pairs of folders and their respective collections.");
            public static readonly GUIContent ScenesCollectionsPairs = new GUIContent("Scenes Collections Pairs", "Pairs of scenes and their respective collections.");
            public static readonly GUIContent DefaultCollections = new GUIContent("Default Collections", "Default pools for scenes with no matching pairs.");
        }

        private const float ListHeaderStyleMargin = 6;
        private const float FoldoutWidth = 10;

        private static bool _isTemporaryCollectionsExpanded;
        private static GUIStyle m_ListBodyStyle;
        private static GUIStyle m_ListHeaderStyle;

        private AnimFloat _searchPriorityAnimation;
        private ReorderableList _persistentCollections;
        private ReorderableList _foldersCollectionsPairs;
        private ReorderableList _scenesCollectionsPairs;
        private ReorderableList _defaultCollections;
        private SerializedObject _serializedObject;
        private SerializedProperty _searchPriority;
        private SerializedProperty _searchMode;

        public PoolSystemSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        private static GUIStyle ListBodyStyle => m_ListBodyStyle ?? (m_ListBodyStyle = new GUIStyle("RL Background") { border = new RectOffset(6, 3, 3, 6) });
        private static GUIStyle ListHeaderStyle => m_ListHeaderStyle ?? (m_ListHeaderStyle = new GUIStyle("RL Header"));

        [SettingsProvider]
        public static SettingsProvider CreatePoolSystemSettingsProvider()
        {
            return new PoolSystemSettingsProvider("Project/Pool System")
            {
                keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
            };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _serializedObject = new SerializedObject(PoolSystemSettings.GetInstance());
            _searchPriority = _serializedObject.FindProperty("_searchPriority");
            _searchMode = _serializedObject.FindProperty("_searchMode");
            _searchPriorityAnimation = new AnimFloat(_searchPriority.intValue == (int)PoolSearchPriority.Folders ? 0 : 1, Repaint);
            _persistentCollections = new ReorderableList(_serializedObject.FindProperty("_persistentCollections"), "name", ReorderablePermissions.Everything, ReorderablePermissions.ReadOnly);
            _foldersCollectionsPairs = new ReorderableList(_serializedObject.FindProperty("_foldersCollectionsPairs"), "@Pair", ReorderablePermissions.Everything, ReorderablePermissions.ReadOnly, ReorderableElementDisplay.Custom);
            _scenesCollectionsPairs = new ReorderableList(_serializedObject.FindProperty("_scenesCollectionsPairs"), "@Pair", ReorderablePermissions.Everything, ReorderablePermissions.ReadOnly, ReorderableElementDisplay.Custom);
            _defaultCollections = new ReorderableList(_serializedObject.FindProperty("_defaultCollections"), "name", ReorderablePermissions.Everything, ReorderablePermissions.ReadOnly);
        }

        public override void OnGUI(string searchContext)
        {
            _serializedObject.Update();

            using (new HierarchyModeScope(true))
            {
                using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
                    {
                        _persistentCollections.DrawGUILayout(Styles.PersistentCollections);

                        Rect controlRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                        controlRect = EditorGUI.IndentedRect(controlRect);

                        if (Event.current.type == EventType.Repaint)
                        {
                            ListHeaderStyle.Draw(controlRect, false, false, false, false);
                        }

                        controlRect.y += 1;
                        controlRect.xMin += ListHeaderStyleMargin + FoldoutWidth;
                        controlRect.xMax -= ListHeaderStyleMargin;

                        using (var innerChangeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            bool value = EditorGUI.Foldout(controlRect, _isTemporaryCollectionsExpanded, Styles.TemporaryCollections, true);

                            if (innerChangeCheckScope.changed)
                            {
                                _isTemporaryCollectionsExpanded = value;
                                Repaint();
                            }
                        }

                        if (_isTemporaryCollectionsExpanded)
                        {
                            controlRect.xMin -= ListHeaderStyleMargin + FoldoutWidth;
                            controlRect.xMax += ListHeaderStyleMargin;
                            controlRect.y += EditorGUIUtility.singleLineHeight - 1 + EditorGUIUtility.standardVerticalSpacing;
                            controlRect.height = EditorGUIUtility.singleLineHeight * 2
                                               + _foldersCollectionsPairs.TotalHeight
                                               + _scenesCollectionsPairs.TotalHeight
                                               + _defaultCollections.TotalHeight
                                               + EditorGUIUtility.standardVerticalSpacing * 7
                                               + ListBodyStyle.border.vertical;

                            if (Event.current.type == EventType.Repaint)
                            {
                                ListBodyStyle.Draw(controlRect, false, false, false, false);
                            }

                            controlRect.y += ListBodyStyle.border.top;
                            controlRect.xMin += ListBodyStyle.border.left;
                            controlRect.xMax -= ListBodyStyle.border.right;
                            controlRect.height = EditorGUIUtility.singleLineHeight;

                            EditorGUI.PropertyField(controlRect, _searchPriority, Styles.SearchPriority);

                            controlRect.y += controlRect.height + EditorGUIUtility.standardVerticalSpacing;
                            controlRect.height = EditorGUIUtility.singleLineHeight;

                            EditorGUI.PropertyField(controlRect, _searchMode, Styles.SearchMode);

                            _searchPriorityAnimation.target = _searchPriority.intValue == (int)PoolSearchPriority.Folders ? 0 : 1;

                            controlRect.y += controlRect.height + EditorGUIUtility.standardVerticalSpacing * 2;
                            controlRect.height = _foldersCollectionsPairs.TotalHeight + _scenesCollectionsPairs.TotalHeight + EditorGUIUtility.standardVerticalSpacing * 2;

                            Rect foldersPosition = controlRect;
                            foldersPosition.y += _searchPriorityAnimation.value * _scenesCollectionsPairs.TotalHeight + _searchPriorityAnimation.value * EditorGUIUtility.standardVerticalSpacing * 2;
                            foldersPosition.height = _foldersCollectionsPairs.TotalHeight;

                            Rect scenesPosition = controlRect;
                            scenesPosition.y += (1 - _searchPriorityAnimation.value) * _foldersCollectionsPairs.TotalHeight + (1 - _searchPriorityAnimation.value) * EditorGUIUtility.standardVerticalSpacing * 2;
                            scenesPosition.height = _scenesCollectionsPairs.TotalHeight;

                            _foldersCollectionsPairs.DrawGUI(foldersPosition, Styles.FoldersCollectionsPairs);
                            _scenesCollectionsPairs.DrawGUI(scenesPosition, Styles.ScenesCollectionsPairs);

                            controlRect.y += controlRect.height + EditorGUIUtility.standardVerticalSpacing * 2;
                            controlRect.height = _defaultCollections.TotalHeight;

                            _defaultCollections.DrawGUI(controlRect, Styles.DefaultCollections);
                        }
                    }

                    if (changeCheckScope.changed)
                    {
                        _serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }
    }
}
