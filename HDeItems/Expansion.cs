using R2API;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HDeMods.HDeItems {
    public static class Expansion {
        public static ExpansionDef def;
        internal static void Init() {
            def = ItemManager.HDeItemsBundle.LoadAsset<ExpansionDef>("HDeItemsExpansion");
            def.disabledIconSprite = Addressables.LoadAssetAsync<Sprite>(
                    "RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();
            ContentAddition.AddExpansionDef(def);
            Log.Debug("Created HDeItems expansion.");
        }
    }
}