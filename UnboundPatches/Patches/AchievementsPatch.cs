using FriendlyFoe.Platform;
using HarmonyLib;

namespace Unbound.Patches {
    [HarmonyPatch(typeof(PlatformManager))]
    public class NoAchievementsPatch {
        [HarmonyPatch("UnlockAchievement")]
        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix() {
            return false;
        }
    }
}
