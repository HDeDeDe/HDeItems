using UnityEngine;

public class Upgrade : MonoBehaviour {
    public MeshRenderer renderMe;
    private Material[] dupeMe;
    void Awake() {
        dupeMe = new Material[renderMe.materials.Length];
        for (int i = 0; i < renderMe.materials.Length; i++) {
            dupeMe[i] = new Material(renderMe.materials[i]);
            ShaderSwapper.ShaderSwapper.UpgradeStubbedShader(dupeMe[i]);
        }
        renderMe.materials = dupeMe;
    }
}
