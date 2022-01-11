using UnityEngine;

namespace Coimbra
{
    [System.Serializable]
    internal sealed class SceneAsset
    {
        [SerializeField, HideInInspector] private string m_Path;

        public string Path
        {
            get => m_Path;
            set => m_Path = value;
        }

#if UNITY_EDITOR
        [SerializeField] private UnityEditor.SceneAsset m_Asset;

        public UnityEditor.SceneAsset Asset => m_Asset;

        public SceneAsset(UnityEditor.SceneAsset asset)
        {
            m_Asset = asset;
        }
#endif
    }
}
