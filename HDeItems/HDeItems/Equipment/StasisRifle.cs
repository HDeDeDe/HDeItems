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

            CustomEquipment customEquipment = new CustomEquipment(Equipment, StasisRifleItemDisplays.Dict());
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
    
    internal static class StasisRifleItemDisplays { 
        public static ItemDisplayRuleDict Dict() {
            ItemDisplayRuleDict dict = new ItemDisplayRuleDict();
            dict.Add("", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = StasisRifle.Equipment.pickupModelPrefab,
                    childName = "Pelvis", 
                    localPos = Vector3.zero,
                    localAngles = Vector3.zero,
                    localScale = Vector3.one
                } });
            dict.Add("mdlCaptain", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = StasisRifle.Equipment.pickupModelPrefab,
                    childName = "ClavicleL",
                    localPos = new Vector3(0.26582F, -0.04032F, -0.08319F),
                    localAngles = new Vector3(18.86092F, 61.65621F, 234.6299F),
                    localScale = new Vector3(0.13468F, 0.13468F, 0.13468F)
                } });
            dict.Add("mdlCroco", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = StasisRifle.Equipment.pickupModelPrefab,
                    childName = "UpperArmL",
                    localPos = new Vector3(-2.03923F, 0.4199F, -1.05398F),
                    localAngles = new Vector3(11.97096F, 196.9454F, 195.2424F),
                    localScale = new Vector3(1.38521F, 1.38521F, 1.38521F)
                } });
            dict.Add("mdlSeeker", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = StasisRifle.Equipment.pickupModelPrefab,
                    childName = "Chest", 
                    localPos = new Vector3(-0.16253F, 0.40363F, 0.14797F),
                    localAngles = new Vector3(342.3047F, 3.03117F, 2.69013F),
                    localScale = new Vector3(0.14505F, 0.14505F, 0.14505F)
                } });
            dict.Add("mdlFalseSon", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = StasisRifle.Equipment.pickupModelPrefab,
                    childName = "ClavL",
                    localPos = new Vector3(0.14317F, 0.47536F, -0.32542F),
                    localAngles = new Vector3(10.87764F, 106.5215F, 266.3787F),
                    localScale = new Vector3(0.21835F, 0.21835F, 0.21835F)
                } });
            return dict;
        }
    }
}