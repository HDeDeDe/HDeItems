using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Configuration;

namespace HDeMods.HDeItems.Tier1 {
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [HDeItem] public class Mouthwash : CharacterBody.ItemBehavior {
        public static ConfigEntry<bool> Enabled { get; set; }
        public static ItemDef item;
        public static List<HurtBox> hurtBoxBuffer = new List<HurtBox>();
        public static SphereSearch searchMe = new SphereSearch();
        public static float baseDistance = 15f;
        public static GameObject rangeDisplayPrefab;

        public static void HDeItem_Init() {
            Enabled = Plugin.instance.Config.Bind<bool>(
                "Items - Tier 1",
                "Mouthwash",
                true,
                "Enables Blazing Mouthwash."
                );
            if (Options.RoO.Enabled) Options.RoO.AddCheck(Enabled, true);
            
            if (!Enabled.Value) return;
            item = ItemManager.HDeItemsBundle.LoadAsset<ItemDef>("HDe_MouthwashDef");
            if (item == null) {
                Log.Error("Failed to load " + nameof(Mouthwash));
                return;
            }
            
            CustomItem customItem = new CustomItem( item, MouthwashItemDisplays.Dict());
            ItemAPI.Add(customItem);
            CharacterBody.onBodyInventoryChangedGlobal += OnInventoryChangedGlobal;
            
            GameObject temp = ItemManager.HDeItemsBundle.LoadAsset<GameObject>("MouthwashRangeIndicator");
            temp.AddComponent<MouthwashRangeDisplay>();
            rangeDisplayPrefab = temp.InstantiateClone("HDeItems_MouthwashRangeIndicator");
            Destroy(temp);
            
            Log.Info("Successfully loaded " + nameof(Mouthwash));
        }

        public static void OnInventoryChangedGlobal(CharacterBody body) {
            body.AddItemBehavior<Mouthwash>(body.inventory.GetItemCount(item));
        }
        
        public float totalDistance;
        public MouthwashRangeDisplay igniteSphere; 
        public BodyData bodyData;
        public bool active;
        public bool calcDirty;

        public void SetCalcDirty() => calcDirty = true;
        public void Recalc() => totalDistance = baseDistance + ((stack - 1) * 5);

        private void Awake() {
            enabled = false;
        }

        private void OnEnable() {
            Recalc();
            bodyData = body.masterObject.GetComponent<BodyData>();
            bodyData.DamageReceivedServerEvent += OnTakeDamageReceivedServer;
            body.inventory.onInventoryChanged += SetCalcDirty;
            active = true;
            if (!NetworkServer.active) return;
            GameObject temp = Instantiate(rangeDisplayPrefab);
            NetworkServer.Spawn(temp);
            igniteSphere = temp.GetComponent<MouthwashRangeDisplay>();
        }

        private void OnDisable() {
            if (!active) return;
            active = false;
            bodyData.DamageReceivedServerEvent -= OnTakeDamageReceivedServer;
            body.inventory.onInventoryChanged -= SetCalcDirty;
            if (!NetworkServer.active) return;
            NetworkServer.Destroy(igniteSphere.gameObject);
        }

        public void OnTakeDamageReceivedServer(DamageReport damageReport) {
            searchMe.origin = transform.position;
            searchMe.mask = LayerIndex.entityPrecise.mask;
            searchMe.radius = totalDistance;
            searchMe.RefreshCandidates();
            searchMe.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(body.teamComponent.teamIndex));
            searchMe.FilterCandidatesByDistinctHurtBoxEntities();
            searchMe.OrderCandidatesByDistance();
            searchMe.GetHurtBoxes(hurtBoxBuffer);
            searchMe.ClearCandidates();
            foreach (HurtBox box in hurtBoxBuffer) {
                HealthComponent hc = box.healthComponent;
                if (!hc) continue;
                BodyDataHelper bd = hc.body.GetComponent<BodyDataHelper>();
                if (bd.invoker.damagedThisTick) continue;
                if (hc.body.GetBuffCount(RoR2Content.Buffs.OnFire) == 3 + (uint)(stack * 2)) continue;
                InflictDotInfo igniteEm = new InflictDotInfo {
                    victimObject = hc.body.gameObject,
                    attackerObject = body.gameObject,
                    totalDamage = body.damage,
                    dotIndex = DotController.DotIndex.Burn,
                    damageMultiplier = 0.75f + (stack * 0.25f),
                    maxStacksFromAttacker = 3 + (uint)(stack * 2)
                };
                StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref igniteEm);
                DotController.InflictDot(ref igniteEm);
                bd.invoker.damagedThisTick = true;
            }
        }

        private void FixedUpdate() {
            if (body.statsDirty) calcDirty = true;
            if (calcDirty) Recalc();
            calcDirty = false;
            if (!NetworkServer.active) return;
            igniteSphere.RpcSetPosition(transform.position);
            igniteSphere.RpcSetSize(Vector3.one * (totalDistance * 2));
        }
    }

    public class MouthwashRangeDisplay : NetworkBehaviour {
        public ObjectScaleCurve scaleCurve;

        private void Awake() => scaleCurve = GetComponent<ObjectScaleCurve>();
        
        [ClientRpc] public void RpcSetPosition(Vector3 position) => SetPosition(position);
        [ClientRpc] public void RpcSetSize(Vector3 size) => SetSize(size);
        private void SetPosition(Vector3 position) => transform.localPosition = position;
        private void SetSize(Vector3 size) => scaleCurve.baseScale = size;
    }

    internal static class MouthwashItemDisplays {
        public static ItemDisplayRuleDict Dict() {
            ItemDisplayRuleDict dict = new ItemDisplayRuleDict();
            dict.Add("", new [] {
                    new ItemDisplayRule {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = Mouthwash.item.pickupModelPrefab,
                        childName = "Pelvis", 
                        localPos = Vector3.zero,
                        localAngles = Vector3.zero,
                        localScale = Vector3.one
                    } });
            dict.Add("mdlCaptain", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = Mouthwash.item.pickupModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.09907F, 0.31531F, 0.12536F),
                    localAngles = new Vector3(5.91019F, 233.3841F, 182.2552F),
                    localScale = new Vector3(0.68848F, 0.68848F, 0.68848F)
                } });
            dict.Add("mdlCroco", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = Mouthwash.item.pickupModelPrefab,
                    childName = "MouthMuzzle",
                    localPos = new Vector3(-2.25849F, 1.75344F, 1.86785F),
                    localAngles = new Vector3(67.92722F, 6.17886F, 254.4054F),
                    localScale = new Vector3(4.8312F, 4.8312F, 4.8312F)
                } });
            dict.Add("mdlSeeker", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = Mouthwash.item.pickupModelPrefab,
                    childName = "Pack",
                    localPos = new Vector3(-0.14047F, 0.05488F, -0.21642F),
                    localAngles = new Vector3(359.3977F, 356.4049F, 45.52491F),
                    localScale = new Vector3(1F, 1F, 1F)
                } });
            dict.Add("mdlFalseSon", new [] {
                new ItemDisplayRule {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = Mouthwash.item.pickupModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.0247F, 0.20892F, 0.46259F),
                    localAngles = new Vector3(359.8492F, 93.74285F, 267.8969F),
                    localScale = new Vector3(1F, 1F, 1F)
                } });
            return dict;
        }
    }
}