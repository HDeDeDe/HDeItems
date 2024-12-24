using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.CharacterAI;

namespace HDeMods.HDeItems {
    public static class AggroManager {
        internal static void Init() {
            On.RoR2.BullseyeSearch.GetResults += BullseyeSearch_GetResults;
            On.RoR2.CharacterAI.BaseAI.OnBodyDamaged += BaseAI_OnBodyDamaged;
        }

        private static void BaseAI_OnBodyDamaged(On.RoR2.CharacterAI.BaseAI.orig_OnBodyDamaged orig, 
            BaseAI self, DamageReport damagereport) {
            if (!self.currentEnemy.characterBody || !damagereport.damageInfo.attacker) {
                orig(self, damagereport);
                return;
            }
            BodyDataHelper enemyBodyData = damagereport.damageInfo.attacker.GetComponent<BodyDataHelper>();
            if (!enemyBodyData) {
#if DEBUG
                Log.Error("AggroManager.BaseAI_OnBodyDamaged: " + damagereport.damageInfo.attacker.name + " does not have a body data helper.");
#endif
                orig(self, damagereport);
                return;
            }
            BodyDataHelper currentTargetData = self.currentEnemy.characterBody.GetComponent<BodyDataHelper>();
            if (!currentTargetData) {
#if DEBUG
                Log.Error("AggroManager.BaseAI_OnBodyDamaged: " +  self.currentEnemy.characterBody.name + " does not have a body data helper.");
#endif
                orig(self, damagereport);
                return;
            }
            
            if (enemyBodyData.invoker.Aggro >= currentTargetData.invoker.Aggro) orig(self, damagereport);
        }

        // Thanks to KingEnderBrine, Chinchi, Violetchaolan, and Harb for this... creature...
        private static IEnumerable<HurtBox> BullseyeSearch_GetResults(On.RoR2.BullseyeSearch.orig_GetResults orig, 
            BullseyeSearch self) => orig(self)
            .OrderByDescending((i) => 
                i.healthComponent && i.healthComponent.body.masterObject
                    .TryGetComponent<BodyData>(out BodyData d) ? d.Aggro : 0);
    }
}