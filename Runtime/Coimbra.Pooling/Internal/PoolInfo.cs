using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Coimbra
{
    [System.Serializable]
    internal struct PoolInfo
    {
        public bool IsPersistent { get; private set; }
        public HashSet<Scene> Scenes { get; private set; }

        public PoolInfo(bool isPersistent) : this()
        {
            IsPersistent = isPersistent;
            Scenes = new HashSet<Scene>();
        }

        public PoolInfo(Scene scene) : this()
        {
            IsPersistent = false;
            Scenes = new HashSet<Scene> { scene };
        }

        public void SetPersistent(bool value)
        {
            IsPersistent = value;
        }
    }
}
