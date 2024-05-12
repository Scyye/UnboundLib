using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unbound.Gamemodes.Patches
{
    [HarmonyPatch(typeof(Player), "Awake")]
    class PlayerPatch
    {
        static void Postfix(Player __instance)
        {
            if (__instance.data.view.IsMine)
                GameModeManager.AddOnceHook(GameModeHooks.HookGameStart, gm => OnGameStart(gm, __instance));
        }

        static IEnumerator OnGameStart(IGameModeHandler gm, Player player)
        {
            if (gm.Name != "Sandbox")
            {
                player.GetFaceOffline();
            }
            yield break;
        }
    }
}
