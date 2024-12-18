using System.Diagnostics.CodeAnalysis;
using BepInEx;
using R2API;

namespace HDeMods.HDeItems {
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class Plugin : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "HDeDeDe";
        public const string PluginName = "HDeItems";
        public const string PluginVersion = "0.0.1";
        public static Plugin instance { get; private set; }

        private void Awake() {
            if (instance != null) {
                HDeItems.Log.Error("There can only be one instance of HDeItemsPlugin.");
                Destroy(this);
                return;
            }
            instance = this;
            HDeItems.Log.Init(Logger);
            HDeItems.ItemManager.Startup();
        }
    }
}