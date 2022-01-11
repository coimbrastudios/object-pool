using System.IO;
using UnityEditor;
using UnityEngine;

namespace Coimbra
{
    [CustomEditor(typeof(PoolDataAsset))]
    internal sealed class PoolDataAssetEditor : Editor
    {
        private static Object _result;
        private SerializedProperty _data;
        private SerializedProperty _asset;
        private SerializedProperty _overrides;
        private SerializedProperty _prefab;
        private SerializedProperty _preloadCount;
        private SerializedProperty _maxCapacity;
        private SerializedProperty _allowInfinityInstances;
        private SerializedProperty _spawnFallback;
        private SerializedProperty _advancedOptions;
        private SerializedProperty _messageType;
        private SerializedProperty _messageOption;
        private SerializedProperty _spawnMessage;
        private SerializedProperty _despawnMessage;

        [MenuItem("Assets/Create/Pool Data", false, 180)]
        private static void CreatePoolAsset()
        {
            PoolDataAsset asset = PoolDataAsset.CreateInstance(PoolData.Default);
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            path = string.IsNullOrEmpty(path) ? "Assets" : path.Replace(Path.GetFileName(path), "");
            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/NewPoolData.asset");

            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        private void OnEnable()
        {
            _data = serializedObject.FindProperty(PoolGUIUtility.Data);
            _asset = _data.FindPropertyRelative(PoolGUIUtility.Asset);
            _overrides = _data.FindPropertyRelative(PoolGUIUtility.Overrides);
            _prefab = _data.FindPropertyRelative(PoolGUIUtility.Prefab);
            _preloadCount = _data.FindPropertyRelative(PoolGUIUtility.PreloadCount);
            _maxCapacity = _data.FindPropertyRelative(PoolGUIUtility.MaxCapacity);
            _allowInfinityInstances = _data.FindPropertyRelative(PoolGUIUtility.AllowInfinityInstances);
            _spawnFallback = _data.FindPropertyRelative(PoolGUIUtility.SpawnFallback);
            _advancedOptions = _data.FindPropertyRelative(PoolGUIUtility.AdvancedOptions);
            _messageType = _data.FindPropertyRelative(PoolGUIUtility.MessageType);
            _messageOption = _data.FindPropertyRelative(PoolGUIUtility.MessageOption);
            _spawnMessage = _data.FindPropertyRelative(PoolGUIUtility.SpawnMessage);
            _despawnMessage = _data.FindPropertyRelative(PoolGUIUtility.DespawnMessage);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("This asset can't be edited in play mode.", MessageType.Info);
            }
            else
            {
                PoolGUIUtility.Validate(_data, _asset, _overrides, _allowInfinityInstances, _messageType);
            }

            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        float labelOffset = 0;

                        if (_asset.objectReferenceValue != null)
                        {
                            if (EditorApplication.isPlaying == false)
                            {
                                using (new EditorGUILayout.VerticalScope(GUILayout.Width(PoolGUIUtility.ButtonsWidth), GUILayout.ExpandWidth(false)))
                                {
                                    Rect position = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(PoolGUIUtility.ButtonsWidth));
                                    position.y += EditorGUIUtility.standardVerticalSpacing;
                                    PoolGUIUtility.DrawResetButtons(position, _overrides, _allowInfinityInstances, _messageType);
                                    labelOffset = position.width + PoolGUIUtility.ButtonsSpacing;
                                }
                            }
                        }
                        else if (_asset.objectReferenceValue == null)
                        {
                            _overrides.intValue = (int)PoolDataOverrides.Everything;
                        }

                        using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                        {
                            using (new LabelWidthScope(-labelOffset))
                            {
                                Rect position = EditorGUILayout.GetControlRect(true, PoolGUIUtility.GetButtonsHeight(_allowInfinityInstances, _messageType));
                                position.height = EditorGUIUtility.singleLineHeight;

                                if (PoolGUIUtility.DrawAssetField(position, _asset, out _result))
                                {
                                    ValidateSharedDataPicker(_result);
                                }

                                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                                PoolGUIUtility.DrawFields(position, _overrides, _prefab, _preloadCount, _maxCapacity, _allowInfinityInstances, _spawnFallback, _advancedOptions, _messageType, _messageOption, _spawnMessage, _despawnMessage, false);
                            }
                        }
                    }

                    if (changeCheckScope.changed)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }

        private void ValidateSharedDataPicker(Object value)
        {
            if (value == null)
            {
                _asset.objectReferenceValue = null;

                return;
            }

            if (value == target)
            {
                Debug.LogException(new System.InvalidOperationException("You should not assign the same pool data as it's own base asset."), target);
            }
            else
            {
                Object reference = value;

                while (reference != null)
                {
                    if (reference == target)
                    {
                        Debug.LogException(new System.InvalidOperationException("This pool data already is the base asset of the selected pool data or one of it's base asset."), target);

                        break;
                    }

                    reference = new SerializedObject(reference).FindProperty(PoolGUIUtility.Data).FindPropertyRelative(PoolGUIUtility.Asset).objectReferenceValue;
                }

                if (reference == null)
                {
                    _asset.objectReferenceValue = value;
                }
            }
        }
    }
}
