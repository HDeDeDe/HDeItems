using BepInEx.Configuration;
using RoR2;
using R2API;

namespace HDeMods.HDeItems.Tier2 {
    [HDeItem] public static class AggroUp {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ItemDef item;
        
        public static void HDeItem_Init() {
            Enabled = Plugin.instance.Config.Bind<bool>(
                "Items - Tier 2",
                "AggroUp",
                true,
                "Enables Boomer Bile."
            );
            if (Options.RoO.Enabled) Options.RoO.AddCheck(Enabled, true);
            if (!Enabled.Value) return;
            
            item = ItemManager.HDeItemsBundle.LoadAsset<ItemDef>("HDe_AggroUpDef");
            if (item == null) {
                Log.Error("Failed to load " + nameof(AggroUp));
                return;
            }
            item.tier = ItemTier.Tier2;
            
            CustomItem customItem = new CustomItem( item, new [] {new ItemDisplayRule()});
            ItemAPI.Add(customItem);
            
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStats;
        }

        private static void RecalculateStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            Inventory inventory = sender.inventory;
            if (!inventory) return;
            BodyData bodyData = sender.GetComponent<BodyData>();
            if (!bodyData) return;
            bodyData.aggroUp = inventory.GetItemCount(item);
        }
    }
}