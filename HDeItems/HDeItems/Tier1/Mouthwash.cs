using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace HDeMods.HDeItems.Tier1 {
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [HDeItem] public class Mouthwash : CharacterBody.ItemBehavior {
        public static ItemDef item;
        public static List<HurtBox> hurtBoxBuffer = new List<HurtBox>();
        public static SphereSearch searchMe = new SphereSearch();
        public static float baseDistance = 10f;
        public static GameObject rangeDisplayPrefab;

        public static void HDeItem_Init() {
            item = ItemManager.HDeItemsBundle.LoadAsset<ItemDef>("MouthwashDef");
            if (item == null) {
                Log.Error("Failed to load " + nameof(Mouthwash));
                return;
            }
            item.tier = ItemTier.Tier1;
            
            CustomItem customItem = new CustomItem( item, new [] {new ItemDisplayRule()});
            ItemAPI.Add(customItem);
            CharacterBody.onBodyInventoryChangedGlobal += OnInventoryChangedGlobal;
            CharacterBody.onBodyAwakeGlobal += MouthwashHelper.OnBodyAwakeGlobal;
            
            GameObject temp = ItemManager.HDeItemsBundle.LoadAsset<GameObject>("MouthwashRangeIndicator");
            temp.AddComponent<MouthwashRangeDisplay>();
            rangeDisplayPrefab = temp.InstantiateClone("HDeItems_MouthwashRangeIndicator");
            Destroy(temp);
            
            Log.Debug("Successfully loaded " + nameof(Mouthwash));
        }

        public static void OnInventoryChangedGlobal(CharacterBody body) {
            body.AddItemBehavior<Mouthwash>(body.inventory.GetItemCount(item));
        }
        
        public float totalDistance;
        public MouthwashRangeDisplay igniteSphere;
        public MouthwashHelper helper;
        
        public void Recalc() => totalDistance = baseDistance + ((float)Math.Tanh((double)(stack - 1) / 20) * 40);

        private void Awake() {
            helper = GetComponent<MouthwashHelper>();
        }

        private void OnEnable() {
            Recalc();
            helper.DamageServerEvent += OnTakeDamageServer;
            if (!NetworkServer.active) return;
            GameObject temp = Instantiate(rangeDisplayPrefab);
            NetworkServer.Spawn(temp);
            igniteSphere = temp.GetComponent<MouthwashRangeDisplay>();
        }

        private void OnDisable() {
            helper.DamageServerEvent -= OnTakeDamageServer;
            if (!NetworkServer.active) return;
            NetworkServer.Destroy(igniteSphere.gameObject);
        }

        public void OnTakeDamageServer(DamageReport damageReport) {
            Recalc();
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
                InflictDotInfo igniteEm = new InflictDotInfo {
                    victimObject = hc.body.gameObject,
                    attackerObject = body.gameObject,
                    totalDamage = 0.50f * body.damage,
                    dotIndex = DotController.DotIndex.Burn,
                    damageMultiplier = 1f,
                    maxStacksFromAttacker = 3 + (uint)(stack * 2)
                };
                StrengthenBurnUtils.CheckDotForUpgrade(body.inventory, ref igniteEm);
                DotController.InflictDot(ref igniteEm);
            }
        }

        private void FixedUpdate() {
            if (body.statsDirty) Recalc();
            if (!NetworkServer.active) return;
            igniteSphere.RpcSetPosition(transform.position);
            igniteSphere.RpcSetSize(Vector3.one * (totalDistance * 2));
        }
    }

    public class MouthwashHelper : NetworkBehaviour, IOnTakeDamageServerReceiver {
        public event Action<DamageReport> DamageServerEvent; 
        public void OnTakeDamageServer(DamageReport damageReport) {
            DamageServerEvent?.Invoke(damageReport);
        }
        
        public static void OnBodyAwakeGlobal(CharacterBody body) {
            body.gameObject.AddComponent<MouthwashHelper>();
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
}