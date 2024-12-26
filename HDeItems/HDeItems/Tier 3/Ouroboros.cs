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

            CustomItem customItem = new CustomItem(item, new[] { new ItemDisplayRule() });
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
}