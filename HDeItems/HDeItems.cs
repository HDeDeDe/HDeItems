using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using ShaderSwapper;
using UnityEngine;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.ContentManagement;
using R2API.ScriptableObjects;
using RoR2;
using UnityEngine.Networking;
using R2API;
using SimpleJSON;

namespace HDeMods.HDeItems {
    public static class ItemManager {
        // ReSharper disable once InconsistentNaming
        internal static AssetBundle HDeItemsBundle;
        internal static R2APISerializableContentPack pack;
        internal static bool successfulHook = true;
        internal static JSONNode displayRules;
        internal static bool displayRulesLoaded;
        
        internal static void Startup() {
            if (!File.Exists(Assembly.GetExecutingAssembly().Location
                    .Replace("HDeItems.dll", "hdeitems"))) {
                Log.Fatal("Could not find asset bundle, aborting!");
                return;
            }

            HDeItemsBundle = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly()
                .Location.Replace("HDeItems.dll", "hdeitems"));
            Plugin.instance.StartCoroutine(HDeItemsBundle.UpgradeStubbedShadersAsync());
            pack = R2APIContentManager.ReserveSerializableContentPack();

            displayRulesLoaded = LoadItemDisplays();
            
            On.RoR2.WwiseIntegrationManager.Init += HDeItem.InitItems;
            IL.RoR2.MasterCatalog.SetEntries += BodyData.MasterCatalog_SetEntries;
            IL.RoR2.BodyCatalog.SetBodyPrefabs += BodyDataHelper.BodyCatalog_SetBodyPrefabs;
            Expansion.Init();
            AggroManager.Init();

            if (OrbAPI.AddOrb<Tier2.InfusionAttackSpeedOrb>()) Tier2.InfusionAttackSpeed.orbAdded = true;
            
            if (!Options.RoO.Enabled) return;
            Options.RoO.SetSprite(HDeItemsBundle.LoadAsset<Sprite>("texHDeExpansionIcon"));
            Options.RoO.SetDescriptionToken("HDE_EXPANSION_DESCRIPTION");
            Options.RoO.AddButton("Reset to Default", "Items - Tier 1", Options.RoO.ResetToDefault);
            Options.RoO.AddButton("Reset to Default", "Items - Tier 2", Options.RoO.ResetToDefault);
            Options.RoO.AddButton("Reset to Default", "Items - Tier 3", Options.RoO.ResetToDefault);
            Options.RoO.AddButton("Reset to Default", "Items - Equipment", Options.RoO.ResetToDefault);
        }

        internal static bool LoadItemDisplays() {
            if (!File.Exists(Assembly.GetExecutingAssembly().Location
                    .Replace("HDeItems.dll", "HDeItems.itemdisplayrules"))) {
                Log.Error("Could not find display rules, items will not appear on bodies!");
                return false;
            }
            
            using Stream stream = File.Open(Assembly.GetExecutingAssembly().Location
                .Replace("HDeItems.dll", "HDeItems.itemdisplayrules"), FileMode.Open, FileAccess.Read);
            using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
            JSONNode jsonNode = JSON.Parse(streamReader.ReadToEnd());
            if (jsonNode == null) {
                Log.Error("Display rules appear to be missing, items will not appear on bodies!");
                return false;
            }
            displayRules = jsonNode["DisplayRules"];
            if (displayRules != null) return true;
            Log.Error("Display rules appear to be corrupted, items will not appear on bodies!");
            return false;
        }
        
        /*
        {bodyName}: {{
             "modelName": {modelName},
             "childName": {childName},
             "localPos": "{localPos.x:5},{localPos.y:5},{localPos.z:5}",
             "localAngles": "{localAngles.x:5},{localAngles.y:5},{localAngles.z:5}",
             "localScale": "{localScale.x:5},{localScale.y:5},{localScale.z:5}"
         }}
         */

        internal static ItemDisplayRuleDict GetDisplayRules(string type, ItemDef item, 
            ItemDisplayRuleType ruleType) => GetDisplayRules(type, item.pickupModelPrefab, ruleType);

        internal static ItemDisplayRuleDict GetDisplayRules(string type, EquipmentDef equipment,
            ItemDisplayRuleType ruleType) => GetDisplayRules(type, equipment.pickupModelPrefab, ruleType);

        internal static ItemDisplayRuleDict GetDisplayRules(string type, GameObject prefab,
            ItemDisplayRuleType ruleType) {
            ItemDisplayRuleDict dict = new ItemDisplayRuleDict();
            dict.Add("", new [] {
                new ItemDisplayRule {
                    ruleType = ruleType,
                    followerPrefab = prefab,
                    childName = "Pelvis", 
                    localPos = Vector3.zero,
                    localAngles = Vector3.zero,
                    localScale = Vector3.one
                } });
            if (!displayRulesLoaded) return dict;
            if (displayRules[type] == null) {
                Log.Error("No item display rules exist for " + type + ".");
                return dict;
            }
            foreach (JSONNode node in displayRules[type].Children) {
                dict.Add(node["modelName"].Value, new [] {
                    new ItemDisplayRule {
                        ruleType = ruleType,
                        followerPrefab = prefab,
                        childName = node["childName"].Value, 
                        localPos = ConvertToVector3(node["localPos"].Value.Split(',', 3)),
                        localAngles = ConvertToVector3(node["localAngles"].Value.Split(',', 3)),
                        localScale = ConvertToVector3(node["localScale"].Value.Split(',', 3))
                    } });
            }
            return dict;
        }
        
        private static Vector3 ConvertToVector3(String[] strings) => new Vector3(float.Parse(strings[0].Trim('F')), 
                float.Parse(strings[1].Trim('F')), float.Parse(strings[2].Trim('F')));
    }
    
    public class BodyData : NetworkBehaviour {
        public event Action<DamageReport> DamageReceivedServerEvent;
        public CharacterBody body;
        public CharacterMaster master;
        [SyncVar]public bool damagedThisTick;
        [SyncVar]public uint infusionBonus;
        [SyncVar]public int aggroUp;
        [SyncVar]public int aggroDown;
        [SyncVar]public int ouroborosBonus;
        public int Aggro => aggroUp - aggroDown;

        public void Awake() {
            master = GetComponent<CharacterMaster>();
            master.onBodyStart += GetBody;
        }

        public static void MasterCatalog_SetEntries(ILContext il) {
            ILCursor c = new ILCursor(il);
            int thing = 0;
            if (!c.TryGotoNext(moveType: MoveType.After,
                    x => x.MatchCallvirt<GameObject>("GetComponent"),
                    x => x.MatchStloc(out thing)
                )) {
                Log.Fatal("Failed to generate hook for MasterCatalog.SetEntries!");
                ItemManager.successfulHook = false;
                return;
            }
            c.Emit(OpCodes.Ldloc, thing);
            c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Action<CharacterMaster>>(characterMaster => {
                characterMaster.gameObject.AddComponent<BodyData>();
            });
        }

        public void GetBody(CharacterBody characterBody) {
            body = characterBody;
            body.GetComponent<BodyDataHelper>().invoker = this;
        }

        public void InvokeDamageReceivedServerEvent(DamageReport damageReport) {
            DamageReceivedServerEvent?.Invoke(damageReport);
        }

        private void FixedUpdate() {
            damagedThisTick = false;
        }
    }

    public class BodyDataHelper : MonoBehaviour, IOnTakeDamageServerReceiver {
        public BodyData invoker;
        public void OnTakeDamageServer(DamageReport damageReport) => 
            invoker?.InvokeDamageReceivedServerEvent(damageReport);

        public static void BodyCatalog_SetBodyPrefabs(ILContext il) {
            ILCursor c = new ILCursor(il);
            int thing = 0;
            if (!c.TryGotoNext(moveType: MoveType.After,
                    x => x.MatchCallvirt<GameObject>("GetComponent"),
                    x => x.MatchDup(),
                    x => x.MatchStloc(out thing)
                )) {
                Log.Fatal("Failed to generate hook for BodyCatalog.SetBodyPrefabs!");
                ItemManager.successfulHook = false;
                return;
            }
            c.Emit(OpCodes.Ldloc, thing);
            c.EmitDelegate<RuntimeILReferenceBag.FastDelegateInvokers.Action<CharacterBody>>(body => {
                body.gameObject.AddComponent<BodyDataHelper>();
            });
        }
    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class HDeItem : Attribute {
        internal static void InitItems(On.RoR2.WwiseIntegrationManager.orig_Init origInit) {
            origInit();
            
            Log.Debug("Loading HDeItems.");
            if (!ItemManager.successfulHook) {
                Log.Fatal("One or more important hooks failed, aborting!");
                return;
            }
            foreach (Type item in GetTypesWithHDeItemAttribute(Assembly.GetExecutingAssembly())) {
                if (!TryCatchMe.Method(out MethodInfo initMethod, item, "HDeItem_Init")) {
                    Log.Error(item + " does not implement HDeItem_Init!");
                    continue;
                }
                initMethod.Invoke(null, null);
            }
            HDeEquipment.InitEquipment();
        }

        private static IEnumerable<Type> GetTypesWithHDeItemAttribute(Assembly assembly) {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(HDeItem), false).Length > 0);
        }
    }
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class HDeEquipment : Attribute {
        private static readonly Dictionary<EquipmentDef, MethodInfo> equipmentActivatorByDef = 
            new Dictionary<EquipmentDef, MethodInfo>();
        
        internal static void InitEquipment() {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            
            foreach (Type equipment in GetTypesWithHDeEquipmentAttribute(Assembly.GetExecutingAssembly())) {
                if (!TryCatchMe.Method(out MethodInfo initMethod, equipment, "HDeEquipment_Init")) {
                    Log.Error(equipment + " does not implement HDeEquipment_Init!");
                    continue;
                }
                initMethod.Invoke(null, null);
                if (!TryCatchMe.PropertyEquipmentDef(out EquipmentDef currentEquipment, equipment, "Equipment")){
                    Log.Error(equipment + " does not have a valid Equipment property!");
                    continue;
                }
                if (!TryCatchMe.Method(out MethodInfo activateMethod, equipment, "HDeEquipment_Activate")){
                    Log.Error(equipment + " does not implement HDeEquipment_Activate!");
                    continue;
                }
                equipmentActivatorByDef.Add(currentEquipment, activateMethod);
            }
        }

        private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, 
            EquipmentSlot self, EquipmentDef equipmentDef) {
            if (equipmentActivatorByDef.TryGetValue(equipmentDef, out MethodInfo activate)) {
                return (bool)activate.Invoke(null, new object[] {self});
            }
            return orig(self, equipmentDef);
        }

        private static IEnumerable<Type> GetTypesWithHDeEquipmentAttribute(Assembly assembly) {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(HDeEquipment), false).Length > 0);
        }
    }

    internal static class TryCatchMe {
        public static bool Method(out MethodInfo methodInfoToWriteTo, Type type, string methodName) {
            methodInfoToWriteTo = AccessTools.Method(type, methodName);
            if (methodInfoToWriteTo == null) return false;
            return true;
        }
        public static bool PropertyEquipmentDef(out EquipmentDef equipmentToWriteTo, Type type, string propertyName) {
            equipmentToWriteTo = (EquipmentDef)AccessTools.Property(type, propertyName).GetValue(null, null);
            if (equipmentToWriteTo == null) return false;
            return true;
        }
    }
    
    internal static class Log {
        private static ManualLogSource logMe;

        internal static void Init(ManualLogSource logSource) {
            logMe = logSource;
        }

        internal static void Debug(object data) => logMe!.LogDebug(data);
        internal static void Error(object data) => logMe!.LogError(data);
        internal static void Fatal(object data) => logMe!.LogFatal(data);
        internal static void Info(object data) => logMe!.LogInfo(data);
        internal static void Message(object data) => logMe!.LogMessage(data);
        internal static void Warning(object data) => logMe!.LogWarning(data);
    }
}