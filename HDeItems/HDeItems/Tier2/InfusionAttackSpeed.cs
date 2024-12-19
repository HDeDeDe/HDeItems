using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Orbs;
using UnityEngine.Networking;

namespace HDeMods.HDeItems.Tier2 {
    [HDeItem] public static class InfusionAttackSpeed {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ItemDef item;
        public static bool orbAdded;
        
        public static void HDeItem_Init() {
            Enabled = Plugin.instance.Config.Bind<bool>(
                "Items - Tier 2",
                "InfusionAttackSpeed",
                true,
                "Enables 5 gram Crack."
            );
            if (Options.RoO.Enabled) Options.RoO.AddCheck(Enabled, true);
            if (!Enabled.Value) return;
            
            if (!orbAdded) {
                Log.Error("Failed to load " + nameof(InfusionAttackSpeed));
                return;
            }
            
            item = ItemManager.HDeItemsBundle.LoadAsset<ItemDef>("HDe_InfusionAttackSpeedDef");
            if (item == null) {
                Log.Error("Failed to load " + nameof(InfusionAttackSpeed));
                return;
            }
            item.tier = ItemTier.Tier2;
            
            CustomItem customItem = new CustomItem( item, new [] {new ItemDisplayRule()});
            ItemAPI.Add(customItem);
            
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStats;
            
            Log.Debug("Successfully loaded " + nameof(InfusionAttackSpeed));
        }

        public static void OnCharacterDeath(DamageReport report) {
            CharacterBody body = report.attackerBody;
            if (!body) return;
            Inventory inventory = body.inventory;
            if (!inventory) return;
            int infCount = inventory.GetItemCount(item);
            if (infCount <= 0) return;
            HDeItemHelper hdeHelper = body.GetComponent<HDeItemHelper>();
            if (hdeHelper.infusionBonus >= (ulong)(infCount * 100)) return;
            
            InfusionAttackSpeedOrb orbMe = new InfusionAttackSpeedOrb {
                origin = report.victimBody.transform.position,
                target = Util.FindBodyMainHurtBox(body),
                attackSpeedValue = infCount
            };
            OrbManager.instance.AddOrb(orbMe);
        }

        public static void RecalculateStats(CharacterBody body, RecalculateStatsAPI.StatHookEventArgs args) {
            Inventory inventory = body.inventory;
            if (!inventory) return;
            int infCount = inventory.GetItemCount(item);
            if (infCount <= 0) return;
            HDeItemHelper hdeHelper = body.gameObject.GetComponent<HDeItemHelper>();
            args.attackSpeedMultAdd += 0.01f * hdeHelper.infusionBonus;
        }
        
        public static void AddInfusionBonus(this HDeItemHelper helper, uint value) {
            if (!NetworkServer.active) {
                Log.Warning("InfusionAttackSpeed.AddInfusionBonus(uint) called on client.");
                return;
            }
            if (value == 0) return;
            helper.infusionBonus += value;
            helper.body.statsDirty = true;
        }
    }

    public class InfusionAttackSpeedOrb : Orb {
        private const float speed = 30f;
        public int attackSpeedValue;
        private HDeItemHelper m_targetHelper;
        
        public override void Begin() {
            duration = distanceToTarget / speed;
            EffectData effectData = new EffectData
            {
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);
            EffectManager.SpawnEffect(OrbStorageUtility.Get("Prefabs/Effects/OrbEffects/InfusionOrbEffect"), effectData, true);
            HurtBox component = target.GetComponent<HurtBox>();
            CharacterBody characterBody = (component != null) ? component.healthComponent.GetComponent<CharacterBody>() : null;
            if (!characterBody) {
                Log.Warning("No character body found");
                return;
            }
            HDeItemHelper helper = characterBody.GetComponent<HDeItemHelper>();
            if (!helper) {
                Log.Warning("Could not aquire item helper.");
                return;
            }
            m_targetHelper = helper;
        }

        public override void OnArrival() {
            if (!m_targetHelper) {
                Log.Warning("No item helper to apply to");
                return;
            }
            m_targetHelper.AddInfusionBonus((uint)attackSpeedValue);
        }
    }
}