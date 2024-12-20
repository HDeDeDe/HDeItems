using R2API;
using RoR2;
using RoR2.EntitlementManagement;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HDeMods.HDeItems {
    public static class Expansion {
        public static ExpansionDef def;
        public static ExpansionDef sotvDef;
        public static ExpansionDef sotsDef;
        
        private static RuleChoiceDef sotvDefOff;
        private static RuleChoiceDef sotsDefOff;
        private static ExpansionDef sotvRef;
        private static ExpansionDef sotsRef;
        
        internal static void Init() {
            def = ItemManager.HDeItemsBundle.LoadAsset<ExpansionDef>("HDeItemsExpansion");
            def.disabledIconSprite = Addressables.LoadAssetAsync<Sprite>(
                    "RoR2/Base/Common/MiscIcons/texUnlockIcon.png").WaitForCompletion();

            sotvDef = ItemManager.HDeItemsBundle.LoadAsset<ExpansionDef>("HDeItemsExpansionSotv");
            sotvDef.requiredEntitlement =
                Addressables.LoadAssetAsync<EntitlementDef>("RoR2/DLC1/Common/entitlementDLC1.asset")
                    .WaitForCompletion();
            sotvDef.disabledIconSprite = def.disabledIconSprite;
            
            sotsDef = ItemManager.HDeItemsBundle.LoadAsset<ExpansionDef>("HDeItemsExpansionSots");
            sotsDef.requiredEntitlement =
                Addressables.LoadAssetAsync<EntitlementDef>("RoR2/DLC2/Common/entitlementDLC2.asset")
                    .WaitForCompletion();
            sotsDef.disabledIconSprite = def.disabledIconSprite;

            sotvRef = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();
            sotsRef = Addressables.LoadAssetAsync<ExpansionDef>("RoR2/DLC2/Common/DLC2.asset").WaitForCompletion();
            
            ContentAddition.AddExpansionDef(def);
            ContentAddition.AddExpansionDef(sotvDef);
            ContentAddition.AddExpansionDef(sotsDef);
            
            RoR2Application.onLoad += OnLoad;
            On.RoR2.Run.OnRuleBookUpdated += Run_OnRuleBookUpdated;

            Log.Debug("Created HDeItems expansion.");
        }
        
        private static void Run_OnRuleBookUpdated(On.RoR2.Run.orig_OnRuleBookUpdated orig, 
            Run self, NetworkRuleBook networkRuleBookComponent) {
            RuleBook ruleBook = networkRuleBookComponent.ruleBook;
            
            if (!ruleBook.IsChoiceActive(def.enabledChoice)) {
                ruleBook.ApplyChoice(sotvDefOff);
                ruleBook.ApplyChoice(sotsDefOff);
                orig(self, networkRuleBookComponent);
                return;
            }
            
            if (ruleBook.IsChoiceActive(sotvRef.enabledChoice)) ruleBook.ApplyChoice(sotvDef.enabledChoice);
            else ruleBook.ApplyChoice(sotvDefOff);
            
            if (ruleBook.IsChoiceActive(sotsRef.enabledChoice)) ruleBook.ApplyChoice(sotsDef.enabledChoice);
            else ruleBook.ApplyChoice(sotsDefOff);
            
            orig(self, networkRuleBookComponent);
        }

        private static void OnLoad() {
            sotvDefOff = RuleCatalog.ruleChoiceDefsByGlobalName["Expansions.HDeItemsExpansionSotv.Off"];
            sotsDefOff = RuleCatalog.ruleChoiceDefsByGlobalName["Expansions.HDeItemsExpansionSots.Off"];
            
            sotvDef.enabledChoice.availableInSinglePlayer = false;
            sotvDefOff.availableInSinglePlayer = false;
            sotvDef.enabledChoice.availableInMultiPlayer = false;
            sotvDefOff.availableInMultiPlayer = false;
            
            sotsDef.enabledChoice.availableInSinglePlayer = false;
            sotsDefOff.availableInSinglePlayer = false;
            sotsDef.enabledChoice.availableInMultiPlayer = false;
            sotsDefOff.availableInMultiPlayer = false;
        }
    }
}