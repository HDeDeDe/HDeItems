using System.Collections.Generic;
using System.Linq;
using RoR2;

namespace HDeMods.HDeItems {
    public static class AggroManager {
        internal static void Init() {
            On.RoR2.BullseyeSearch.GetResults += BullseyeSearch_GetResults;
        }

        // Thanks to KingEnderBrine, Chinchi, Violetchaolan, and Harb for this... creature...
        private static IEnumerable<HurtBox> BullseyeSearch_GetResults(On.RoR2.BullseyeSearch.orig_GetResults orig, 
            BullseyeSearch self) => orig(self)
            .OrderByDescending((i) => 
                i.healthComponent && i.healthComponent
                    .TryGetComponent<BodyData>(out BodyData d) ? d.Aggro : 0);
    }
}