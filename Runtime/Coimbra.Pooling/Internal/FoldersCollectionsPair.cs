using UnityEngine;

namespace Coimbra
{
    [System.Serializable]
    internal sealed class FoldersCollectionsPair
    {
        [SerializeField] private string[] m_Folders;
        [SerializeField] private PoolDataCollection[] m_Collections;

        public string[] Folders => m_Folders;
        public PoolDataCollection[] Collections => m_Collections;

        public FoldersCollectionsPair(string[] folders, PoolDataCollection[] collections)
        {
            m_Folders = folders;
            m_Collections = collections;
        }
    }
}
