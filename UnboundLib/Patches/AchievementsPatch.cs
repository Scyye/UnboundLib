using FriendlyFoe.Platform;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unbound.Core.Patches
{
    [HarmonyPatch(typeof(PlatformManager))]
    public class NoAchievementsPatch
    {
        [HarmonyPatch("UnlockAchievement")]
        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix()
        {
            return false;
        }
    }
}
