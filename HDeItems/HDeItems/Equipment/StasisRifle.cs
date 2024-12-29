using RoR2;
using R2API;
using BepInEx.Configuration;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HDeMods.HDeItems.Equipment {
    [HDeEquipment] public static class StasisRifle {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static EquipmentDef Equipment { get; set; }

        public static readonly float rifleDamage = 50f;
        public static readonly float rifleForce = 10f;

        private static GameObject iceSpearPrefab;
        
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

            iceSpearPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageIceBombProjectile.prefab")
                .WaitForCompletion();

            CustomEquipment customEquipment = new CustomEquipment(Equipment, 
                ItemManager.GetDisplayRules(nameof(StasisRifle), Equipment, ItemDisplayRuleType.ParentedPrefab) );
            ItemAPI.Add(customEquipment);
            
            Log.Info("Successfully loaded " + nameof(StasisRifle));
        }

        public static bool HDeEquipment_Activate(EquipmentSlot self) {
            Ray aimRay = self.GetAimRay();
            
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo {
                projectilePrefab = iceSpearPrefab,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                owner = self.gameObject,
                damage = self.characterBody.damage * rifleDamage,
                force = rifleForce,
                crit = self.characterBody.RollCrit(),
                damageTypeOverride = new DamageTypeCombo(
                    DamageType.Freeze2s, DamageTypeExtended.Generic, DamageSource.NoneSpecified)
            };
            TrajectoryAimAssist.ApplyTrajectoryAimAssist(ref aimRay, ref fireProjectileInfo, 1f);
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            return true;
        }
    }
}