using RoR2;
using R2API;
using BepInEx.Configuration;
using RoR2.Stats;
using UnityEngine;
using UnityEngine.Networking;

namespace HDeMods.HDeItems.Tier3 {
    [HDeItem] public class Ouroboros : CharacterBody.ItemBehavior {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ItemDef item;
        public static StatDef stat;
        
        public static void HDeItem_Init() {
            Enabled = Plugin.instance.Config.Bind<bool>(
                "Items - Tier 3",
                "Ouroboros",
                true,
                "Enables Ourobot."
            );
            if (Options.RoO.Enabled) Options.RoO.AddCheck(Enabled, true);

            if (!Enabled.Value) return;
            item = ItemManager.HDeItemsBundle.LoadAsset<ItemDef>("HDe_OuroborosDef");
            if (item == null) {
                Log.Error("Failed to load " + nameof(Ouroboros));
                return;
            }

            CustomItem customItem = new CustomItem(item, OuroborosItemDisplays.Dict());
            ItemAPI.Add(customItem);
            
            CharacterBody.onBodyInventoryChangedGlobal += OnInventoryChangedGlobal;
            /*RecalculateStatsAPI.GetStatCoefficients += RecalcStats;*/
            On.RoR2.CharacterMaster.Respawn += CharacterMaster_Respawn;
            RoR2Application.onLoad += OnLoad;
            
            Log.Info("Successfully loaded " + nameof(Ouroboros));
        }

        private static CharacterBody CharacterMaster_Respawn(On.RoR2.CharacterMaster.orig_Respawn orig, 
            CharacterMaster self, Vector3 footposition, Quaternion rotation, bool wasrevivedmidstage) {
            if (!NetworkServer.active) {
                return orig(self, footposition, rotation, wasrevivedmidstage);
            }
            BodyData data = self.GetComponent<BodyData>();
            if (!data) {
#if DEBUG
                Log.Error("Ouroboros.CharacterMaster_Respawn: " + self.name + " does not have body data.");
#endif
                return orig(self, footposition, rotation, wasrevivedmidstage);
            }
            data.ouroborosBonus++;
            return orig(self, footposition, rotation, wasrevivedmidstage);
        }

        public static void OnInventoryChangedGlobal(CharacterBody body) {
            body.AddItemBehavior<Ouroboros>(body.inventory.GetItemCount(item));
        }

        private static void OnLoad() {
            stat = StatDef.Find("totalCollected.HDe_OuroborosDef");
        }

        /*private static void RecalcStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            if (!sender.inventory) return;
            if (sender.inventory.GetItemCount(item) <= 0) return;
            
            PlayerCharacterMasterController pcmc = sender.master.playerCharacterMasterController;
            if (!pcmc) return;
            if (!pcmc.hasAuthority) return;
            LocalUser player = null;
            foreach (LocalUser user in LocalUserManager.localUsersList) {
                if (user.cachedMasterController == pcmc) player = user;
            }
            if (player == null) return;
            BodyData data = sender.gameObject.GetComponent<BodyData>();
            if (!data) return;
            int ouroCount = (int)player.userProfile.statSheet.GetStatValueULong(stat);
            data.ouroborosBonus = ouroCount;
        }*/

        public BodyData bodyData;
        
        private void Awake() {
            enabled = false;
        }

        private void OnEnable() {
            bodyData = body.masterObject.GetComponent<BodyData>();
            HealthComponentAPI.OnTakeDamageProcess += OnDamageDealtServer;
        }
        
        private void OnDisable() {
            HealthComponentAPI.OnTakeDamageProcess -= OnDamageDealtServer;
        }

        private void OnDamageDealtServer(HealthComponent hc, DamageInfo damageInfo) {
            if (damageInfo.attacker != body.gameObject) return;
            if (!body.inventory) return;
            float damageMult = body.damage * body.inventory.GetItemCount(item);
            damageMult *= bodyData.ouroborosBonus;
            damageInfo.damage += damageMult;
        }
    }
    
    internal static class OuroborosItemDisplays { 
        public static ItemDisplayRuleDict Dict() {
            ItemDisplayRuleDict dict = new ItemDisplayRuleDict();
            dict.Add("", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = Ouroboros.item.pickupModelPrefab,
                    childName = "Pelvis", 
                    localPos = Vector3.zero,
                    localAngles = Vector3.zero,
                    localScale = Vector3.one
                } });
            dict.Add("mdlCaptain", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = Ouroboros.item.pickupModelPrefab,
                    childName = "MuzzleGun",
                    localPos = new Vector3(0.00199F, 0.03108F, 0.03413F),
                    localAngles = new Vector3(294.3165F, 273.2378F, 352.7377F),
                    localScale = new Vector3(0.0859F, 0.0859F, 0.0859F)
                } });
            dict.Add("mdlCroco", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = Ouroboros.item.pickupModelPrefab,
                    childName = "Finger11R",
                    localPos = new Vector3(0.29144F, 0.53971F, 0.3068F),
                    localAngles = new Vector3(1.69213F, 341.5157F, 261.3066F),
                    localScale = new Vector3(1F, 1F, 1F)
                } });
            dict.Add("mdlSeeker", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = Ouroboros.item.pickupModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(-0.00014F, 0.03745F, 0.02692F),
                    localAngles = new Vector3(22.19945F, 346.8863F, 1.49889F),
                    localScale = new Vector3(0.07501F, 0.07501F, 0.07501F)
                } });
            dict.Add("mdlFalseSon", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = Ouroboros.item.pickupModelPrefab,
                    childName = "HandR",
                    localPos = new Vector3(0.07315F, 0.31218F, 0.01564F),
                    localAngles = new Vector3(11.84246F, 128.1426F, 82.2345F),
                    localScale = new Vector3(0.12908F, 0.12908F, 0.12908F)
                } });
            return dict;
        }
    }
}