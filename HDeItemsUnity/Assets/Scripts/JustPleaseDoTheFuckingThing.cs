#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class JustPleaseDoTheFuckingThing : MonoBehaviour {
    [MenuItem ("Assets/AB Make")]
    static void BuildBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundle",BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
#endif