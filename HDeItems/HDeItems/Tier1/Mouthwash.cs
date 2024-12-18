using RoR2;
using R2API;

namespace HDeMods.HDeItems.Tier1 {
    [HDeItem] public static class Mouthwash {
        public static ItemDef item;

        public static void HDeItem_Init() {
            item = ItemManager.HDeItemsBundle.LoadAsset<ItemDef>("MouthwashDef");
            if (item == null) {
                Log.Error("Failed to load " + nameof(Mouthwash));
                return;
            }
            item.tier = ItemTier.Tier1;
            
            CustomItem customItem = new CustomItem( item, new [] {new ItemDisplayRule()});
            ItemAPI.Add(customItem);
            Log.Debug("Successfully loaded " + nameof(Mouthwash));
        }
    }
}