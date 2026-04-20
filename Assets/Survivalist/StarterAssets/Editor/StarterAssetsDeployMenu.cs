using UnityEditor;
using UnityEngine;

namespace StarterAssets
{
    // Script de menú de StarterAssets sin Cinemachine
    public partial class StarterAssetsDeployMenu : ScriptableObject
    {
        public const string MenuRoot = "Tools/Starter Assets";

        private static string PathToThisFile
        {
            get
            {
                var dummy = CreateInstance<StarterAssetsDeployMenu>();
                string path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(dummy));
                DestroyImmediate(dummy);
                return path.Substring(0, path.LastIndexOf("/Editor/StarterAssetsDeployMenu.cs"));
            }
        }

        [MenuItem(MenuRoot + "/Reinstall Dependencies", false)]
        static void ResetPackageChecker()
        {
            // Si no usas PackageChecker, puedes borrar esta línea
            ScriptingDefineUtils.RemoveScriptingDefine("STARTER_ASSETS_PACKAGES_CHECKED");
        }
    }
}
