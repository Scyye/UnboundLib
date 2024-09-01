using HarmonyLib;
using Unbound.Core.Utils;
using UnboundLib.Networking.Lobbies;
using UnityEngine;

namespace Unbound.Patches {
    [HarmonyPatch(typeof(DevConsole))]
    internal class DevConsolePatch {
        [HarmonyPrefix]
        [HarmonyPatch("Send")]
        private static bool Send_Postfix(string message) {
            if(MainMenuHandler.instance.isOpen) {
                MainMenuHandler.instance.Close();
                LoadingScreen.instance?.StartLoading();
                Unbound_Lobby.Join(message);
                return false;
            }
            if(Application.isEditor || (GM_Test.instance && GM_Test.instance.gameObject.activeSelf)) {
                LevelManager.SpawnMap(message);
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SpawnCard")]
        private static bool SpawnCard_Prefix(string message) {
            return !message.Contains("/");
        }
    }
}