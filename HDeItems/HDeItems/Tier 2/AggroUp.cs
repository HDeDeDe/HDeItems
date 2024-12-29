using BepInEx.Configuration;
using RoR2;
using R2API;
using UnityEngine;

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
            
            CustomItem customItem = new CustomItem( item, AggroUpItemDisplays.Dict());
            ItemAPI.Add(customItem);
            
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStats;
            Log.Info("Successfully loaded " + nameof(AggroUp));
        }

        private static void RecalculateStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            Inventory inventory = sender.inventory;
            if (!inventory) return;
            BodyDataHelper bodyDataHelper = sender.GetComponent<BodyDataHelper>();
            if (!bodyDataHelper) {
#if DEBUG
                Log.Error("AggroUp.RecalculateStats: " + sender.name + " does not have body data.");
#endif
                return;
            }
            if (!bodyDataHelper.invoker) return;
            bodyDataHelper.invoker.aggroUp = inventory.GetItemCount(item);
        }
    }
    
    internal static class AggroUpItemDisplays { 
        public static ItemDisplayRuleDict Dict() {
            ItemDisplayRuleDict dict = new ItemDisplayRuleDict();
            dict.Add("", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroUp.item.pickupModelPrefab,
                    childName = "Pelvis", 
                    localPos = Vector3.zero,
                    localAngles = Vector3.zero,
                    localScale = Vector3.one
                } });
            dict.Add("mdlCaptain", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroUp.item.pickupModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.22936F, 0.15437F, 0.20492F),
                    localAngles = new Vector3(25.8474F, 338.3632F, 3.01506F),
                    localScale = new Vector3(0.20412F, 0.20412F, 0.20412F)
                } });
            dict.Add("mdlCroco", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroUp.item.pickupModelPrefab,
                    childName = "MuzzleHandL",
                    localPos = new Vector3(0.55362F, 0.29384F, 0.19695F),
                    localAngles = new Vector3(18.69648F, 93.08537F, 20.37013F),
                    localScale = new Vector3(1.58809F, 1.58809F, 1.58809F)
                } });
            dict.Add("mdlSeeker", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroUp.item.pickupModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.08417F, 0.00688F, 0.1085F),
                    localAngles = new Vector3(34.85723F, 359.8937F, 0.10785F),
                    localScale = new Vector3(0.17757F, 0.17757F, 0.17757F)
                } });
            dict.Add("mdlFalseSon", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroUp.item.pickupModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(0.02985F, 0.15759F, 0.14391F),
                    localAngles = new Vector3(353.3982F, 324.8724F, 303.6209F),
                    localScale = new Vector3(0.1963F, 0.1963F, 0.1963F)
                } });
            return dict;
        }
    }
}