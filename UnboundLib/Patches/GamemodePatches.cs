using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unbound.Core.GameModes;

namespace Unbound.Core.Patches
{
    internal class GamemodePatches
    {
        [HarmonyPatch(typeof(GM_ArmsRace), "Start")]
        [HarmonyPrefix]
        static void ArmsRaceStartPre(GM_ArmsRace __instance)
        {
            UnboundCore.Instance.StartCoroutine(GameModeManager.TriggerHook(GameModeHooks.HookInitStart));
        }


        [HarmonyPatch(typeof(GM_ArmsRace), "Start")]
        [HarmonyPostfix]
        static void ArmsRaceStartPost(GM_ArmsRace __instance)
        {
            UnboundCore.Instance.StartCoroutine(GameModeManager.TriggerHook(GameModeHooks.HookInitEnd));
        }

        [HarmonyPatch(typeof(GM_Test), "Start")]
        [HarmonyPrefix]
        static void SandboxStartPre(GM_Test __instance)
        {
            UnboundCore.Instance.StartCoroutine(GameModeManager.TriggerHook(GameModeHooks.HookInitStart));
            UnboundCore.Instance.StartCoroutine(GameModeManager.TriggerHook(GameModeHooks.HookInitEnd));
            UnboundCore.Instance.StartCoroutine(GameModeManager.TriggerHook(GameModeHooks.HookGameStart));
        }

        [HarmonyPatch(typeof(GM_Test), "Start")]
        [HarmonyPostfix]
        static void SandboxStartPost(GM_Test __instance)
        {
            UnboundCore.Instance.StartCoroutine(GameModeManager.TriggerHook(GameModeHooks.HookRoundStart));
            UnboundCore.Instance.StartCoroutine(GameModeManager.TriggerHook(GameModeHooks.HookBattleStart));
        }
    }
}
