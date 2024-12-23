using BepInEx.Configuration;
using RoR2;
using R2API;

namespace HDeMods.HDeItems.Tier2 {
    [HDeItem] public static class AggroDown {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ItemDef item;
        
        public static void HDeItem_Init() {
            Enabled = Plugin.instance.Config.Bind<bool>(
                "Items - Tier 2",
                "AggroDown",
                true,
                "Enables Music Cassette."
            );
            if (Options.RoO.Enabled) Options.RoO.AddCheck(Enabled, true);
            if (!Enabled.Value) return;
            
            item = ItemManager.HDeItemsBundle.LoadAsset<ItemDef>("HDe_AggroDownDef");
            if (item == null) {
                Log.Error("Failed to load " + nameof(AggroDown));
                return;
            }
            item.tier = ItemTier.Tier2;
            
            CustomItem customItem = new CustomItem( item, new [] {new ItemDisplayRule()});
            ItemAPI.Add(customItem);
            
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStats;
            Log.Debug("Successfully loaded " + nameof(AggroDown));
        }

        private static void RecalculateStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            Inventory inventory = sender.inventory;
            if (!inventory) return;
            BodyData bodyData = sender.masterObject.GetComponent<BodyData>();
            if (!bodyData) {
#if DEBUG
                Log.Error("AggroDown.RecalculateStats: " + sender.masterObject.name + " does not have body data.");
#endif
                return;
            }
            bodyData.aggroDown = inventory.GetItemCount(item);
        }
    }
}