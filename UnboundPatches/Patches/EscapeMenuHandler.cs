using HarmonyLib;
using Unbound.Cards.Utils;
using Unbound.Gamemodes.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Unbound.Core.Patches {
    [HarmonyPatch(typeof(EscapeMenuHandler))]
    public class EscapeMenuHandlerPath {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void Start(EscapeMenuHandler __instance) {
            __instance.transform.Find("Main/Group/Menu").GetComponent<Button>().onClick.RemoveAllListeners();
            __instance.transform.Find("Main/Group/Menu").GetComponent<Button>().onClick.AddListener(delegate { GameManager.instance.GoToMenu(); }) ;
        }
    }
}