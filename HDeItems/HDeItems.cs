using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ShaderSwapper;
using UnityEngine;
using BepInEx.Logging;
using R2API.ContentManagement;
using R2API.ScriptableObjects;
using RoR2;
using UnityEngine.Networking;
using R2API;

namespace HDeMods.HDeItems {
    public static class ItemManager {
        // ReSharper disable once InconsistentNaming
        internal static AssetBundle HDeItemsBundle;
        internal static R2APISerializableContentPack pack;
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
            
            On.RoR2.WwiseIntegrationManager.Init += HDeItem.InitItems;
            CharacterBody.onBodyAwakeGlobal += BodyData.OnBodyAwakeGlobal;
            Expansion.Init();
            AggroManager.Init();

            if (OrbAPI.AddOrb<Tier2.InfusionAttackSpeedOrb>()) Tier2.InfusionAttackSpeed.orbAdded = true;
            
            if (!Options.RoO.Enabled) return;
            Options.RoO.SetSprite(HDeItemsBundle.LoadAsset<Sprite>("texHDeExpansionIcon"));
            Options.RoO.SetDescriptionToken("HDE_EXPANSION_DESCRIPTION");
            Options.RoO.AddButton("Reset to Default", "Items - Tier 1", Options.RoO.ResetToDefault);
            Options.RoO.AddButton("Reset to Default", "Items - Tier 2", Options.RoO.ResetToDefault);
            Options.RoO.AddButton("Reset to Default", "Items - Tier 3", Options.RoO.ResetToDefault);
        }
    }
    
    public class BodyData : NetworkBehaviour, IOnTakeDamageServerReceiver, IOnDamageDealtServerReceiver {
        public event Action<DamageReport> DamageReceivedServerEvent;
        public event Action<DamageReport> DamageDealtServerEvent;
        public CharacterBody body; 
        [SyncVar]public bool damagedThisTick;
        [SyncVar]public uint infusionBonus;
        [SyncVar]public int aggroUp;
        [SyncVar]public int aggroDown;
        [SyncVar]public int ouroborosBonus;
        public int Aggro => aggroUp - aggroDown;

        public void OnTakeDamageServer(DamageReport damageReport) {
            DamageReceivedServerEvent?.Invoke(damageReport);
        }

        public void OnDamageDealtServer(DamageReport damageReport) {
            DamageDealtServerEvent?.Invoke(damageReport);
        }
        
        public static void OnBodyAwakeGlobal(CharacterBody body) {
            BodyData temp = body.gameObject.AddComponent<BodyData>();
            temp.body = body;
        }

        private void FixedUpdate() {
            damagedThisTick = false;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class HDeItem : Attribute {
        internal static void InitItems(On.RoR2.WwiseIntegrationManager.orig_Init origInit) {
            origInit();
            
            Log.Debug("Loading HDeItems.");
            foreach (Type item in GetTypesWithHDeItemAttribute(Assembly.GetExecutingAssembly())) {
                AccessTools.Method(item, "HDeItem_Init").Invoke(null, null);
            }
        }

        private static IEnumerable<Type> GetTypesWithHDeItemAttribute(Assembly assembly) {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(HDeItem), false).Length > 0);
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