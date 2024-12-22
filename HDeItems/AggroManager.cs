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
            if (self.currentEnemy == null) return;
            BodyData enemyBodyData = damagereport.damageInfo.attacker.GetComponent<BodyData>();
            BodyData currentTargetData = self.currentEnemy.gameObject.GetComponent<BodyData>();
            if (!enemyBodyData || !currentTargetData) {
                orig(self, damagereport);
                return;
            }
            if (enemyBodyData.Aggro >= currentTargetData.Aggro) orig(self, damagereport);
        }

        // Thanks to KingEnderBrine, Chinchi, Violetchaolan, and Harb for this... creature...
        private static IEnumerable<HurtBox> BullseyeSearch_GetResults(On.RoR2.BullseyeSearch.orig_GetResults orig, 
            BullseyeSearch self) => orig(self)
            .OrderByDescending((i) => 
                i.healthComponent && i.healthComponent
                    .TryGetComponent<BodyData>(out BodyData d) ? d.Aggro : 0);
    }
}