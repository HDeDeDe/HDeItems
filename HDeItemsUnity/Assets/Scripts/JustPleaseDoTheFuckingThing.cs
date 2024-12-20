#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class JustPleaseDoTheFuckingThing : MonoBehaviour {
    [MenuItem ("Assets/Just fuckin do the thing")]
    static void BuildBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundle",BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
#endif