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
            CharacterBody.onBodyAwakeGlobal += HDeItemHelper.OnBodyAwakeGlobal;
            Expansion.Init();
        }
    }
    
    public class HDeItemHelper : NetworkBehaviour, IOnTakeDamageServerReceiver {
        public event Action<DamageReport> DamageServerEvent;
        [SyncVar]public bool damagedThisTick;
        public void OnTakeDamageServer(DamageReport damageReport) {
            DamageServerEvent?.Invoke(damageReport);
        }
        
        public static void OnBodyAwakeGlobal(CharacterBody body) {
            body.gameObject.AddComponent<HDeItemHelper>();
        }

        private void FixedUpdate() {
            damagedThisTick = false;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class  HDeItem : Attribute {
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