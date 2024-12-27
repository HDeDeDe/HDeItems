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

            CustomItem customItem = new CustomItem(item, new[] { new ItemDisplayRule() });
            ItemAPI.Add(customItem);
            
            Log.Info("Successfully loaded " + nameof(IonCube));
        }
    }*/
}