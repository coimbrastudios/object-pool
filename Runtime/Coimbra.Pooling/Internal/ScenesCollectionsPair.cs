using UnityEngine;

namespace Coimbra
{
    [System.Serializable]
    internal sealed class ScenesCollectionsPair
    {
        [SerializeField] private SceneAsset[] m_Scenes;
        [SerializeField] private PoolDataCollection[] m_Collections;

        public SceneAsset[] Scenes => m_Scenes;
        public PoolDataCollection[] Collections => m_Collections;

        public ScenesCollectionsPair(SceneAsset[] scenes, PoolDataCollection[] collections)
        {
            m_Scenes = scenes;
            m_Collections = collections;
        }
    }
}
