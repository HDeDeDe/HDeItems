using BepInEx.Configuration;
using RoR2;
using R2API;
using UnityEngine;

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
            
            CustomItem customItem = new CustomItem( item, AggroDownItemDisplays.Dict());
            ItemAPI.Add(customItem);
            
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStats;
            Log.Info("Successfully loaded " + nameof(AggroDown));
        }

        private static void RecalculateStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            Inventory inventory = sender.inventory;
            if (!inventory) return;
            BodyDataHelper bodyDataHelper = sender.GetComponent<BodyDataHelper>();
            if (!bodyDataHelper) {
#if DEBUG
                Log.Error("AggroDown.RecalculateStats: " + sender.name + " does not have body data.");
#endif
                return;
            }
            if (!bodyDataHelper.invoker) return;
            bodyDataHelper.invoker.aggroDown = inventory.GetItemCount(item);
        }
    }
    
    internal static class AggroDownItemDisplays { 
        public static ItemDisplayRuleDict Dict() {
            ItemDisplayRuleDict dict = new ItemDisplayRuleDict();
            dict.Add("", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroDown.item.pickupModelPrefab,
                    childName = "Pelvis", 
                    localPos = Vector3.zero,
                    localAngles = Vector3.zero,
                    localScale = Vector3.one
                } });
            dict.Add("mdlCaptain", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroDown.item.pickupModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(0.05939F, -0.14319F, -0.19125F),
                    localAngles = new Vector3(320.9315F, 178.0158F, 159.9391F),
                    localScale = new Vector3(0.03587F, 0.03587F, 0.03587F)
                } });
            dict.Add("mdlCroco", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroDown.item.pickupModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(1.1954F, 3.50799F, 0.22777F),
                    localAngles = new Vector3(19.52925F, 75.19997F, 345.0016F),
                    localScale = new Vector3(0.4365F, 0.4365F, 0.4365F)
                } });
            dict.Add("mdlSeeker", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroDown.item.pickupModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(-0.08133F, 0.09759F, -0.10975F),
                    localAngles = new Vector3(351.662F, 207.5618F, 152.1761F),
                    localScale = new Vector3(0.05069F, 0.05069F, 0.05069F)
                } });
            dict.Add("mdlFalseSon", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = AggroDown.item.pickupModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.17879F, 0.108F, 0.03433F),
                    localAngles = new Vector3(27.36147F, 270.0096F, 340.8546F),
                    localScale = new Vector3(0.06671F, 0.06671F, 0.06671F)
                } });
            return dict;
        }
    }
}