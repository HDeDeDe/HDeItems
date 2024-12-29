using RoR2;
using R2API;
using BepInEx.Configuration;

namespace HDeMods.HDeItems.Tier1 {
    /*[HDeItem] public class IonCube {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ItemDef item;
        
        public static void HDeItem_Init() {
            Enabled = Plugin.instance.Config.Bind<bool>(
                "Items - Tier 1",
                "IonCube",
                true,
                "Enables Ion Cube."
            );
            if (Options.RoO.Enabled) Options.RoO.AddCheck(Enabled, true);

            if (!Enabled.Value) return;
            item = ItemManager.HDeItemsBundle.LoadAsset<ItemDef>("HDe_IonCubeDef");
            if (item == null) {
                Log.Error("Failed to load " + nameof(IonCube));
                return;
            }

            CustomItem customItem = new CustomItem(item, IonCubeItemDisplays.Dict());
            ItemAPI.Add(customItem);
            
            Log.Info("Successfully loaded " + nameof(IonCube));
        }
    }
    
    internal static class IonCubeItemDisplays {
        public static ItemDisplayRuleDict Dict() {
            ItemDisplayRuleDict dict = new ItemDisplayRuleDict();
            dict.Add("", new [] {
                    new ItemDisplayRule {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = IonCube.item.pickupModelPrefab,
                        childName = "Pelvis", 
                        localPos = Vector3.zero,
                        localAngles = Vector3.zero,
                        localScale = Vector3.one
                    } });
            return dict;
        }
    }*/
}