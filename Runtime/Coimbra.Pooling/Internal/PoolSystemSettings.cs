using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Coimbra
{
    internal sealed class PoolSystemSettings : ScriptableSingleton<PoolSystemSettings>
    {
        [SerializeField] private PoolDataCollection[] _persistentCollections = new PoolDataCollection[0];
        [SerializeField] private PoolSearchPriority _searchPriority = PoolSearchPriority.Folders;
        [SerializeField] private PoolSearchMode _searchMode = PoolSearchMode.FirstMatch;
        [SerializeField] private FoldersCollectionsPair[] _foldersCollectionsPairs = new FoldersCollectionsPair[0];
        [SerializeField] private ScenesCollectionsPair[] _scenesCollectionsPairs = new ScenesCollectionsPair[0];
        [SerializeField] private PoolDataCollection[] _defaultCollections = new PoolDataCollection[0];

        protected override string DefaultAssetPath => $"Assets/Coimbra/Resources/{GetType().Name}.asset";

        internal static void GetPersistentPools(List<PoolData> pools)
        {
            pools.Clear();

            if (Exists)
            {
                GetInstance(false).Internal_GetPersistentPools(pools);
            }
        }

        internal static void GetTemporaryPools(string scenePath, List<PoolData> pools)
        {
            pools.Clear();

            if (Exists)
            {
                GetInstance(false).Internal_GetTemporaryPools(scenePath, pools);
            }
        }

        [Conditional("UNITY_EDITOR")]
        internal static void OnPreprocessBuild()
        {
            GetInstance(false);
        }

        internal static PoolSystemSettings GetInstance()
        {
            return GetInstance(true);
        }

        protected override void OnInitialize()
        {
            DontDestroyOnLoad(this);

#if UNITY_EDITOR
            foreach (ScenesCollectionsPair pair in _scenesCollectionsPairs)
            {
                SceneAsset[] scenes = pair.Scenes;

                if (scenes == null)
                {
                    continue;
                }

                for (int i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i].Asset != null)
                    {
                        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(scenes[i].Asset);
                        scenes[i].Path = string.IsNullOrEmpty(assetPath) ? "" : assetPath;
                    }
                    else
                    {
                        scenes[i].Path = "";
                    }
                }
            }
#endif
        }

        private void Internal_GetPersistentPools(List<PoolData> pools)
        {
            ReadCollections(_persistentCollections, pools);
        }

        private void Internal_GetTemporaryPools(string scenePath, List<PoolData> pools)
        {
            switch (_searchPriority)
            {
                case PoolSearchPriority.Folders:
                {
                    SearchByFolder(scenePath, pools);

                    if (pools.Count == 0 || _searchMode == PoolSearchMode.AllMatches)
                    {
                        SearchByScene(scenePath, pools);
                    }

                    break;
                }

                case PoolSearchPriority.Scenes:
                {
                    SearchByScene(scenePath, pools);

                    if (pools.Count == 0 || _searchMode == PoolSearchMode.AllMatches)
                    {
                        SearchByFolder(scenePath, pools);
                    }

                    break;
                }
            }

            if (pools.Count == 0)
            {
                ReadCollections(_defaultCollections, pools);
            }
        }

        private void ReadCollections(PoolDataCollection[] collections, List<PoolData> pools)
        {
            for (int i = 0; i < collections.Length; i++)
            {
                if (collections[i] == null)
                {
                    continue;
                }

                for (int j = 0; j < collections[i].Length; j++)
                {
                    pools.Add(collections[i][j]);
                }
            }
        }

        private void SearchByFolder(string scenePath, List<PoolData> pools)
        {
            for (int i = 0; i < _foldersCollectionsPairs.Length; i++)
            {
                FoldersCollectionsPair pair = _foldersCollectionsPairs[i];

                if (pair?.Folders == null)
                {
                    continue;
                }

                for (int j = 0; j < pair.Folders.Length; j++)
                {
                    if (string.IsNullOrEmpty(pair.Folders[j]) == false && scenePath.StartsWith(pair.Folders[j]))
                    {
                        goto FOUND;
                    }
                }

                continue;

                FOUND:
                {
                    ReadCollections(pair.Collections, pools);

                    if (_searchMode == PoolSearchMode.FirstMatch)
                    {
                        return;
                    }
                }
            }
        }

        private void SearchByScene(string scenePath, List<PoolData> pools)
        {
            for (int i = 0; i < _scenesCollectionsPairs.Length; i++)
            {
                ScenesCollectionsPair pair = _scenesCollectionsPairs[i];

                if (pair?.Scenes == null)
                {
                    continue;
                }

                for (int j = 0; j < pair.Scenes.Length; j++)
                {
                    if (scenePath == pair.Scenes[j].Path)
                    {
                        goto FOUND;
                    }
                }

                continue;

                FOUND:
                {
                    ReadCollections(pair.Collections, pools);

                    if (_searchMode == PoolSearchMode.FirstMatch)
                    {
                        return;
                    }
                }
            }
        }
    }
}
