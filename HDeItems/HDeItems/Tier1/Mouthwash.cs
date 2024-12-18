using RoR2;
using R2API;

namespace HDeMods.HDeItems.Tier1 {
    [HDeItem]
    public static class Mouthwash {
        public static CustomItem item;

        public static void HDeItem_Init() {
            item = new CustomItem(
                ItemManager.HDeItemsBundle.LoadAsset<ItemDef>("MouthwashDef"),
                new [] {new ItemDisplayRule()});
            
            if (item.ItemDef == null) {
                Log.Error("Failed to load " + nameof(Mouthwash));
                return;
            }
            item.ItemDef.tier = ItemTier.Tier1;
            
            ItemAPI.Add(item);
            Log.Debug("Successfully loaded " + nameof(Mouthwash));
        }
    }
}