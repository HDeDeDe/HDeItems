using RoR2;
using R2API;
using BepInEx.Configuration;

namespace HDeMods.HDeItems.Equipment {
    [HDeEquipment] public static class StasisRifle {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static EquipmentDef Equipment { get; set; }
        
        public static void HDeEquipment_Init() {
            Enabled = Plugin.instance.Config.Bind<bool>(
                "Items - Equipment",
                "StasisRifle",
                true,
                "Enables Stasis Rifle."
            );
            if (Options.RoO.Enabled) Options.RoO.AddCheck(Enabled, true);

            if (!Enabled.Value) return;
            Equipment = ItemManager.HDeItemsBundle.LoadAsset<EquipmentDef>("HDe_StasisRifleDef");
            if (Equipment == null) {
                Log.Error("Failed to load " + nameof(StasisRifle));
                return;
            }

            CustomEquipment customEquipment = new CustomEquipment(Equipment, new[] { new ItemDisplayRule() });
            ItemAPI.Add(customEquipment);
            
            Log.Info("Successfully loaded " + nameof(StasisRifle));
        }

        public static bool HDeEquipment_Activate(EquipmentSlot self) {
            return true;
        }
    }
}