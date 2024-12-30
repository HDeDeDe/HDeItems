using System;
using System.IO;
using System.Text;
using R2API;
using RoR2;
using SimpleJSON;
using UnityEngine;

namespace HDeMods.HDeItems {
    public static class ItemDisplayHelper {
        private static JSONNode displayRules;
        private static bool displayRulesLoaded;
        
        internal static void LoadItemDisplays(string filePath) {
            if (!File.Exists(filePath)) {
                Log.Error("Could not find display rules, items will not appear on bodies!");
                return;
            }
            
            using Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
            JSONNode jsonNode = JSON.Parse(streamReader.ReadToEnd());
            if (jsonNode == null) {
                Log.Error("Display rules appear to be missing, items will not appear on bodies!");
                return;
            }
            displayRules = jsonNode["DisplayRules"];
            if (displayRules == null) {
                Log.Error("Display rules appear to be corrupted, items will not appear on bodies!");
                return;
            }
            displayRulesLoaded = true;
        }

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
}