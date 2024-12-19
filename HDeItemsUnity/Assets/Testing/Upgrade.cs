using UnityEngine;

public class Upgrade : MonoBehaviour {
    public Material upgradeMe;
    public MeshRenderer renderMe;
    public Material dupeMe;
    void Awake() {
        dupeMe = new Material(upgradeMe);
        ShaderSwapper.ShaderSwapper.UpgradeStubbedShader(dupeMe);
        renderMe.material = dupeMe;
    }
}
