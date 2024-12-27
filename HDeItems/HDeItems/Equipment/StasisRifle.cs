using RoR2;
using R2API;
using BepInEx.Configuration;

namespace HDeMods.HDeItems.Equipment {
    [HDeItem] public class StasisRifle {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static EquipmentDef equipment;
        
        public static void HDeItem_Init() {
            Enabled = Plugin.instance.Config.Bind<bool>(
                "Items - Equipment",
                "StasisRifle",
                true,
                "Enables Stasis Rifle."
            );
            if (Options.RoO.Enabled) Options.RoO.AddCheck(Enabled, true);

            if (!Enabled.Value) return;
            equipment = ItemManager.HDeItemsBundle.LoadAsset<EquipmentDef>("HDe_StasisRifleDef");
            if (equipment == null) {
                Log.Error("Failed to load " + nameof(StasisRifle));
                return;
            }

            CustomEquipment customEquipment = new CustomEquipment(equipment, new[] { new ItemDisplayRule() });
            ItemAPI.Add(customEquipment);
            
            Log.Info("Successfully loaded " + nameof(StasisRifle));
        }
    }
}