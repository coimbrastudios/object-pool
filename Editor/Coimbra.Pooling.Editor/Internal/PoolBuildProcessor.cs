using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Coimbra
{
    internal sealed class PoolBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            PoolSystemSettings.OnPreprocessBuild();
            AssetDatabase.SaveAssets();
        }
    }
}
