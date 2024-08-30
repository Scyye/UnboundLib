using HarmonyLib;
using Unbound.Cards.Utils;
using Unbound.Core;
using UnityEngine;

namespace Unbound.Cards.Patches {
    [HarmonyPatch(typeof(MainMenuHandler))]
    internal class MainMenuHandlerPatch {
        private static bool firstTime = true;
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        internal static void MainMenuHandlerAwake() {
            Debug.Log("0");

            Unbound.Core.UnboundCore.Instance.ExecuteAfterFrames(5, () => {
                CardManager.RestoreCardToggles();
                ToggleCardsMenuHandler.RestoreCardToggleVisuals();
            });
            if(firstTime) {
                Unbound.Core.UnboundCore.Instance.ExecuteAfterSeconds(0.4f, () =>
                    CardManager.FirstTimeStart());
                firstTime = false;
            }

        }
    }
}
