using System.IO;
using UnityEditor;

namespace Coimbra
{
    [CustomEditor(typeof(PoolDataCollection))]
    internal sealed class PoolDataCollectionEditor : Editor
    {
        private ReorderableList _datas;

        [MenuItem("Assets/Create/Pool Data Collection", false, 181)]
        private static void CreatePoolAsset()
        {
            var asset = CreateInstance<PoolDataCollection>();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            path = string.IsNullOrEmpty(path) ? "Assets" : path.Replace(Path.GetFileName(path), "");
            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/NewPoolDataCollection.asset");

            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        private void OnEnable()
        {
            _datas = new ReorderableList(serializedObject.FindProperty("_datas"), PoolGUIUtility.CollectionElementName, ReorderablePermissions.Everything, ReorderablePermissions.ReadOnly, ReorderableElementDisplay.Custom);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("This asset can't be edited in play mode.", MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                Validate(target as PoolDataCollection);

                _datas.DrawGUILayout();

                serializedObject.ApplyModifiedProperties();
            }
        }

        private void Validate(PoolDataCollection data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                _datas.GetItem(i).FindPropertyRelative(PoolGUIUtility.IsDuplicated).boolValue = false;

                if (data[i].Prefab == null)
                {
                    continue;
                }

                for (int j = 0; j < data.Length; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    if (data[i].Prefab == data[j].Prefab)
                    {
                        _datas.GetItem(i).FindPropertyRelative(PoolGUIUtility.IsDuplicated).boolValue = true;

                        break;
                    }
                }
            }
        }
    }
}
