using UnityEngine;

public class Upgrade : MonoBehaviour {
    private MeshRenderer renderMe;
    private Material[] dupeMe;
    void Awake() {
        renderMe = GetComponent<MeshRenderer>();
        if (!renderMe) {
            Debug.LogError("There's no renderer on this thing idiot.");
            return;
        }
        dupeMe = new Material[renderMe.materials.Length];
        for (int i = 0; i < renderMe.materials.Length; i++) {
            dupeMe[i] = new Material(renderMe.materials[i]);
            ShaderSwapper.ShaderSwapper.UpgradeStubbedShader(dupeMe[i]);
        }
        renderMe.materials = dupeMe;
    }
}
