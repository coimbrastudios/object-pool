using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Coimbra
{
    [InitializeOnLoad]
    public static class PoolGUIUtility
    {
        public const float ButtonsSpacing = 4;
        public const float ButtonsWidth = 56;
        public const string IsDuplicated = "m_IsDuplicated";
        public const string Data = "_data";
        public const string Asset = "Asset";
        public const string Overrides = "Overrides";
        public const string Prefab = "Prefab";
        public const string PreloadCount = "PreloadCount";
        public const string MaxCapacity = "MaxCapacity";
        public const string AllowInfinityInstances = "AllowInfinityInstances";
        public const string SpawnFallback = "SpawnFallback";
        public const string AdvancedOptions = "AdvancedOptions";
        public const string MessageType = "MessageType";
        public const string MessageOption = "MessageOption";
        public const string SpawnMessage = "SpawnMessage";
        public const string DespawnMessage = "DespawnMessage";
        public const string CollectionElementName = "$Prefab";
        public const string SystemElementName = "$" + Data + "." + Prefab;

        private static readonly int PoolDataOverridesOffset;
        private static readonly GUIContent AllowInfinityInstancesLabel = new GUIContent("Infinity?");
        private static readonly GUIContent AssetLabel = new GUIContent("Base Asset");
        private static readonly GUIContent PrefabLabel = new GUIContent("Prefab");
        private static readonly GUIContent SlashLabel = new GUIContent("/");
        private static readonly GUIContent AdvancedOptionsLabel = new GUIContent("Advanced Options", "Only change those if sure of what you are doing!");
        private static readonly GUIContent InstancesLabel = new GUIContent("Instances", "The amount of instances to preload and the pool's max capacity.");
        private static readonly GUIContent SpawnFallbackLabel = new GUIContent("Spawn Fallback", "Behaviour when the limit is reached:\n\n" + "Use Instantiate: create a new instance.\n" + "Return Null: don't spawn anything.\n" + "Debug Exception: debug the pool state.");
        private static readonly GUIContent BaseButtonLabel = new GUIContent("Base", "This value is being driven by the base asset.");
        private static readonly GUIContent ResetAllButtonLabel = new GUIContent("Reset All", "Reset all local changes to base asset's values.");
        private static readonly GUIContent ResetButtonLabel = new GUIContent("Reset", "Reset to base asset's value.");
        private static readonly GUIContent SaveAsButtonLabel = new GUIContent("Save As", "Save this pool data to a new pool data asset.");

        private static int? _basePickerId;
        private static int? _prefabPickerId;
        private static SerializedProperty _propertyPicker;

        static PoolGUIUtility()
        {
            PoolDataOverridesOffset = System.Enum.GetValues(typeof(PoolDataOverrides)).Cast<int>().Max() * 2;
        }

        public static void DrawFields(Rect position, SerializedProperty data, SerializedProperty overrides, SerializedProperty prefab, SerializedProperty maxCapacity, SerializedProperty allowInfinityInstances, SerializedProperty messageType, bool disablePrefabAndInstances)
        {
            DrawFields(position, overrides, prefab, data.FindPropertyRelative(PreloadCount), maxCapacity, allowInfinityInstances, data.FindPropertyRelative(SpawnFallback), data.FindPropertyRelative(AdvancedOptions), messageType, data.FindPropertyRelative(MessageOption), data.FindPropertyRelative(SpawnMessage), data.FindPropertyRelative(DespawnMessage), disablePrefabAndInstances);
        }

        public static void DrawFields(Rect position, SerializedProperty overrides, SerializedProperty prefab, SerializedProperty preloadCount, SerializedProperty maxCapacity, SerializedProperty allowInfinityInstances, SerializedProperty spawnFallback, SerializedProperty advancedOptions, SerializedProperty messageType, SerializedProperty messageOption, SerializedProperty spawnMessage, SerializedProperty despawnMessage, bool disablePrefabAndInstances)
        {
            using (new EditorGUI.DisabledScope(disablePrefabAndInstances))
            {
                if (DrawPrefab(position, prefab))
                {
                    SetOverrides(overrides, PoolDataOverrides.Prefab, true);
                }

                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                if (DrawInstances(position, preloadCount, maxCapacity, allowInfinityInstances))
                {
                    SetOverrides(overrides, PoolDataOverrides.Instances, true);
                }
            }

            if (allowInfinityInstances.boolValue == false)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                    if (DrawSpawnFallback(position, spawnFallback))
                    {
                        SetOverrides(overrides, PoolDataOverrides.SpawnFallback, true);
                    }
                }
            }

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            if (DrawAdvancedOptions(position, advancedOptions))
            {
                SetOverrides(overrides, PoolDataOverrides.AdvancedOptions, true);
            }

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            if (DrawMessageType(position, messageType))
            {
                SetOverrides(overrides, PoolDataOverrides.MessageType, true);
            }

            if (messageType.intValue != (int)PoolMessageType.None)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                    if (DrawMessageOption(position, messageOption))
                    {
                        SetOverrides(overrides, PoolDataOverrides.MessageOption, true);
                    }

                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                    if (DrawSpawnMessage(position, spawnMessage))
                    {
                        SetOverrides(overrides, PoolDataOverrides.SpawnMessage, true);
                    }

                    position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

                    if (DrawDespawnMessage(position, despawnMessage))
                    {
                        SetOverrides(overrides, PoolDataOverrides.DespawnMessage, true);
                    }
                }
            }
        }

        public static void DrawResetButtons(Rect position, SerializedProperty overrides, SerializedProperty allowInfinityInstances, SerializedProperty messageType)
        {
            using (new EditorGUI.DisabledScope(overrides.intValue == 0))
            {
                if (GUI.Button(position, ResetAllButtonLabel, EditorStyles.miniButton))
                {
                    overrides.intValue = 0;
                }
            }

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            DrawButton(position, overrides, PoolDataOverrides.Prefab);

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            DrawButton(position, overrides, PoolDataOverrides.Instances);

            if (allowInfinityInstances.boolValue == false)
            {
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                DrawButton(position, overrides, PoolDataOverrides.SpawnFallback);
            }

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            DrawButton(position, overrides, PoolDataOverrides.AdvancedOptions);

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            DrawButton(position, overrides, PoolDataOverrides.MessageType);

            if (messageType.intValue != (int)PoolMessageType.None)
            {
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                DrawButton(position, overrides, PoolDataOverrides.MessageOption);

                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                DrawButton(position, overrides, PoolDataOverrides.SpawnMessage);

                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                DrawButton(position, overrides, PoolDataOverrides.DespawnMessage);
            }
        }

        public static void DrawSaveButton(Rect position, SerializedProperty data, SerializedProperty asset, SerializedProperty overrides, SerializedProperty prefab)
        {
            if (GUI.Button(position, SaveAsButtonLabel, EditorStyles.miniButton))
            {
                string folder = AssetDatabase.GetAssetPath(asset.objectReferenceValue != null ? asset.objectReferenceValue : Selection.activeObject);

                if (string.IsNullOrEmpty(folder))
                {
                    folder = "Assets";
                }
                else if (string.IsNullOrEmpty(Path.GetExtension(folder)) == false)
                {
                    folder = folder.Replace(Path.GetFileName(folder), "");
                }

                string path = EditorUtility.SaveFilePanel("Save Pool Data", folder, (prefab.objectReferenceValue != null ? prefab.objectReferenceValue.name : "New") + "PoolData", "asset");

                if (string.IsNullOrEmpty(path) == false)
                {
                    path = path.Replace(Application.dataPath, "Assets");

                    var reference = AssetDatabase.LoadAssetAtPath<PoolDataAsset>(path);

                    if (reference == null)
                    {
                        reference = PoolDataAsset.CreateInstance(PoolData.Default);
                        AssetDatabase.CreateAsset(reference, path);
                    }

                    var target = new SerializedObject(reference);
                    SerializedProperty property = target.FindProperty(Data);

                    property.FindPropertyRelative(Asset).objectReferenceValue = null;
                    property.FindPropertyRelative(Overrides).intValue = ~0;
                    property.FindPropertyRelative(Prefab).objectReferenceValue = data.FindPropertyRelative(Prefab).objectReferenceValue;
                    property.FindPropertyRelative(PreloadCount).intValue = data.FindPropertyRelative(PreloadCount).intValue;
                    property.FindPropertyRelative(MaxCapacity).intValue = data.FindPropertyRelative(MaxCapacity).intValue;
                    property.FindPropertyRelative(AllowInfinityInstances).boolValue = data.FindPropertyRelative(AllowInfinityInstances).boolValue;
                    property.FindPropertyRelative(SpawnFallback).intValue = data.FindPropertyRelative(SpawnFallback).intValue;
                    property.FindPropertyRelative(AdvancedOptions).intValue = data.FindPropertyRelative(AdvancedOptions).intValue;
                    property.FindPropertyRelative(MessageType).intValue = data.FindPropertyRelative(MessageType).intValue;
                    property.FindPropertyRelative(MessageOption).intValue = data.FindPropertyRelative(MessageOption).intValue;
                    property.FindPropertyRelative(SpawnMessage).stringValue = data.FindPropertyRelative(SpawnMessage).stringValue;
                    property.FindPropertyRelative(DespawnMessage).stringValue = data.FindPropertyRelative(DespawnMessage).stringValue;

                    target.ApplyModifiedPropertiesWithoutUndo();

                    AssetDatabase.SaveAssets();

                    asset.objectReferenceValue = reference;
                    overrides.intValue = 0;
                }
            }
        }

        public static void Validate(SerializedProperty data, SerializedProperty asset, SerializedProperty overrides, SerializedProperty allowInfinityInstances, SerializedProperty messageType)
        {
            if (asset.objectReferenceValue == null)
            {
                SetOverrides(overrides, PoolDataOverrides.Everything, true);
            }
            else
            {
                Apply(data, ((PoolDataAsset)new SerializedObject(asset.objectReferenceValue).targetObject).Data, (PoolDataOverrides)overrides.intValue);
            }

            if (allowInfinityInstances.boolValue && GetOverrides(overrides, PoolDataOverrides.Instances) == false)
            {
                SetOverrides(overrides, PoolDataOverrides.SpawnFallback, false);
            }

            if (messageType.intValue == (int)PoolMessageType.None && GetOverrides(overrides, PoolDataOverrides.MessageType) == false)
            {
                SetOverrides(overrides, PoolDataOverrides.MessageOption | PoolDataOverrides.SpawnMessage | PoolDataOverrides.DespawnMessage, false);
            }
        }

        public static bool DrawAssetField(Rect position, SerializedProperty asset, out Object result)
        {
            using (var propertyScope = new EditorGUI.PropertyScope(position, AssetLabel, asset))
            {
                using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    Object value = EditorGUI.ObjectField(position, propertyScope.content, asset.objectReferenceValue, typeof(PoolDataAsset), false);

                    if (changeCheckScope.changed)
                    {
                        result = value;
                        _basePickerId = EditorGUIUtility.GetObjectPickerControlID();
                        _propertyPicker = asset;

                        return true;
                    }

                    if (Event.current.commandName == "ObjectSelectorUpdated" && _basePickerId.HasValue && _basePickerId.Value == EditorGUIUtility.GetObjectPickerControlID() && _propertyPicker == asset)
                    {
                        result = EditorGUIUtility.GetObjectPickerObject();

                        return true;
                    }

                    result = null;

                    return false;
                }
            }
        }

        public static float GetButtonsHeight(SerializedProperty allowInfinityInstances, SerializedProperty messageType)
        {
            float height = EditorGUIUtility.singleLineHeight * 5 + EditorGUIUtility.standardVerticalSpacing * 4;

            if (allowInfinityInstances.boolValue == false)
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            if (messageType.intValue != (int)PoolMessageType.None)
            {
                height += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 3;
            }

            return height;
        }

        public static float GetFieldsHeight(SerializedProperty allowInfinityInstances, SerializedProperty messageType)
        {
            float height = EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 3;

            if (allowInfinityInstances.boolValue == false)
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            if (messageType.intValue != (int)PoolMessageType.None)
            {
                height += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 3;
            }

            return height;
        }

        private static void Apply(SerializedProperty property, PoolData data, PoolDataOverrides sharedDataOverrides)
        {
            if (sharedDataOverrides == PoolDataOverrides.Everything)
            {
                return;
            }

            if ((sharedDataOverrides & PoolDataOverrides.Prefab) == 0)
            {
                property.FindPropertyRelative(Prefab).objectReferenceValue = data.Prefab;
            }

            if ((sharedDataOverrides & PoolDataOverrides.Instances) == 0)
            {
                property.FindPropertyRelative(PreloadCount).intValue = data.PreloadCount;
                property.FindPropertyRelative(MaxCapacity).intValue = data.MaxCapacity;
                property.FindPropertyRelative(AllowInfinityInstances).boolValue = data.AllowInfinityInstances;
            }

            if ((sharedDataOverrides & PoolDataOverrides.SpawnFallback) == 0)
            {
                property.FindPropertyRelative(SpawnFallback).intValue = (int)data.SpawnFallback;
            }

            if ((sharedDataOverrides & PoolDataOverrides.AdvancedOptions) == 0)
            {
                property.FindPropertyRelative(AdvancedOptions).intValue = (int)data.AdvancedOptions;
            }

            if ((sharedDataOverrides & PoolDataOverrides.MessageType) == 0)
            {
                property.FindPropertyRelative(MessageType).intValue = (int)data.MessageType;
            }

            if ((sharedDataOverrides & PoolDataOverrides.MessageOption) == 0)
            {
                property.FindPropertyRelative(MessageOption).intValue = (int)data.MessageOption;
            }

            if ((sharedDataOverrides & PoolDataOverrides.SpawnMessage) == 0)
            {
                property.FindPropertyRelative(SpawnMessage).stringValue = data.SpawnMessage;
            }

            if ((sharedDataOverrides & PoolDataOverrides.DespawnMessage) == 0)
            {
                property.FindPropertyRelative(DespawnMessage).stringValue = data.DespawnMessage;
            }
        }

        private static void DrawButton(Rect position, SerializedProperty overrides, PoolDataOverrides values)
        {
            if (GetOverrides(overrides, values))
            {
                if (GUI.Button(position, ResetButtonLabel, EditorStyles.miniButton))
                {
                    SetOverrides(overrides, values, false);
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUI.Button(position, BaseButtonLabel, EditorStyles.miniButton);
                }
            }
        }

        private static void SetOverrides(SerializedProperty overrides, PoolDataOverrides values, bool value)
        {
            var current = (PoolDataOverrides)overrides.intValue;

            if (value)
            {
                current |= values;
            }
            else
            {
                current &= ~values;
            }

            var i = (int)current;

            if (i < -1)
            {
                i += PoolDataOverridesOffset;
            }

            overrides.intValue = i;
        }

        private static bool DrawAdvancedOptions(Rect position, SerializedProperty advancedOptions)
        {
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(position, advancedOptions, AdvancedOptionsLabel);

                return changeCheckScope.changed;
            }
        }

        private static bool DrawDespawnMessage(Rect position, SerializedProperty despawnMessage)
        {
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(position, despawnMessage);

                if (changeCheckScope.changed)
                {
                    despawnMessage.stringValue = GetValidMethodName(despawnMessage.stringValue, PoolData.DefaultDespawnMessage);

                    return true;
                }

                despawnMessage.stringValue = GetValidMethodName(despawnMessage.stringValue, PoolData.DefaultDespawnMessage);

                return false;
            }
        }

        private static bool DrawInstances(Rect position, SerializedProperty preloadCount, SerializedProperty maxCapacity, SerializedProperty allowInfinityInstances)
        {
            using (var generalScope = new EditorGUI.ChangeCheckScope())
            {
                Rect fieldPosition = EditorGUI.PrefixLabel(position, InstancesLabel);

                using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
                {
                    float slashLabelWidth = EditorStyles.label.CalcSize(SlashLabel).x;
                    float boolLabelWidth = EditorStyles.label.CalcSize(AllowInfinityInstancesLabel).x;
                    float boolFieldWidth = EditorStyles.toggle.CalcSize(GUIContent.none).x;
                    float intFieldWidth = (fieldPosition.width - boolLabelWidth - boolFieldWidth - slashLabelWidth) / 2;

                    fieldPosition.width = intFieldWidth;

                    using (new EditorGUI.PropertyScope(fieldPosition, GUIContent.none, preloadCount))
                    {
                        using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                        {
                            int value = EditorGUI.IntField(fieldPosition, GUIContent.none, preloadCount.intValue);

                            if (changeCheckScope.changed)
                            {
                                preloadCount.intValue = Mathf.Max(value, 0);
                            }
                        }
                    }

                    fieldPosition.x += fieldPosition.width;
                    fieldPosition.width = slashLabelWidth;
                    EditorGUI.LabelField(fieldPosition, SlashLabel);

                    fieldPosition.x += fieldPosition.width;
                    fieldPosition.width = intFieldWidth;

                    if (allowInfinityInstances.boolValue)
                    {
                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUI.TextField(fieldPosition, GUIContent.none, "Infinity");
                        }
                    }
                    else
                    {
                        using (new EditorGUI.PropertyScope(fieldPosition, GUIContent.none, maxCapacity))
                        {
                            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                            {
                                int value = EditorGUI.IntField(fieldPosition, GUIContent.none, maxCapacity.intValue);

                                if (changeCheckScope.changed)
                                {
                                    maxCapacity.intValue = Mathf.Max(value, 0);
                                }
                            }
                        }
                    }

                    fieldPosition.x += fieldPosition.width;
                    fieldPosition.width = boolLabelWidth;
                    EditorGUI.LabelField(fieldPosition, AllowInfinityInstancesLabel);

                    fieldPosition.x += fieldPosition.width;
                    fieldPosition.width = boolFieldWidth;
                    EditorGUI.PropertyField(fieldPosition, allowInfinityInstances, GUIContent.none);
                }

                return generalScope.changed;
            }
        }

        private static bool DrawMessageOption(Rect position, SerializedProperty messageOption)
        {
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(position, messageOption);

                return changeCheckScope.changed;
            }
        }

        private static bool DrawMessageType(Rect position, SerializedProperty messageType)
        {
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(position, messageType);

                return changeCheckScope.changed;
            }
        }

        private static bool DrawPrefab(Rect position, SerializedProperty prefab)
        {
            using (var propertyScope = new EditorGUI.PropertyScope(position, PrefabLabel, prefab))
            {
                using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    Object value = EditorGUI.ObjectField(position, propertyScope.content, prefab.objectReferenceValue, typeof(GameObject), false);

                    if (changeCheckScope.changed)
                    {
                        prefab.objectReferenceValue = value;
                        _prefabPickerId = EditorGUIUtility.GetObjectPickerControlID();
                        _propertyPicker = prefab;

                        return true;
                    }

                    if (Event.current.commandName == "ObjectSelectorUpdated" && _prefabPickerId.HasValue && _prefabPickerId.Value == EditorGUIUtility.GetObjectPickerControlID() && _propertyPicker == prefab)
                    {
                        prefab.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();

                        return false;
                    }

                    return false;
                }
            }
        }

        private static bool DrawSpawnFallback(Rect position, SerializedProperty spawnFallback)
        {
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(position, spawnFallback, SpawnFallbackLabel);

                return changeCheckScope.changed;
            }
        }

        private static bool DrawSpawnMessage(Rect position, SerializedProperty spawnMessage)
        {
            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(position, spawnMessage);

                if (changeCheckScope.changed)
                {
                    spawnMessage.stringValue = GetValidMethodName(spawnMessage.stringValue, PoolData.DefaultSpawnMessage);

                    return true;
                }

                spawnMessage.stringValue = GetValidMethodName(spawnMessage.stringValue, PoolData.DefaultSpawnMessage);

                return false;
            }
        }

        private static bool GetOverrides(SerializedProperty overrides, PoolDataOverrides values)
        {
            int i = overrides.intValue;

            if (i < -1)
            {
                i += PoolDataOverridesOffset;
                overrides.intValue = i;
            }

            return ((PoolDataOverrides)overrides.intValue & values) == values;
        }

        private static string GetValidMethodName(string name, string defaultValue = "_")
        {
            name = Regex.Replace(name, "[^a-zA-Z0-9_]", "", RegexOptions.Compiled);

            while (string.IsNullOrEmpty(name) == false)
            {
                if (char.IsDigit(name[0]))
                {
                    name = name.Remove(0, 1);
                }
                else
                {
                    return name;
                }
            }

            return defaultValue;
        }
    }
}
