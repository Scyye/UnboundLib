using HarmonyLib;
using Photon.Pun;
using System.Linq;
using Unbound.Cards.Utils;
using Unbound.Core;
using UnityEngine;

namespace Unbound.Cards.Patches {
    [HarmonyPatch(typeof(CardChoice))]
    internal class CardChoicePatch {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CardChoice.GetSourceCard))]
        private static bool CheckHiddenCards(CardChoice __instance, CardInfo info, ref CardInfo __result) {
            for(int i = 0; i < __instance.cards.Length; i++) {
                if((__instance.cards[i].gameObject.name + "(Clone)") == info.gameObject.name) {
                    __result = __instance.cards[i];
                    return false;
                }
            }
            __result = null;

            return false;
        }
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void CardChoiceStartPost(CardChoice __instance) {
            for(int i = 0; i < __instance.cards.Length; i++) {
                if(!((DefaultPool)PhotonNetwork.PrefabPool).ResourceCache.ContainsKey(__instance.cards[i].gameObject.name))
                    PhotonNetwork.PrefabPool.RegisterPrefab(__instance.cards[i].gameObject.name, __instance.cards[i].gameObject);
            }
            var children = new Transform[__instance.transform.childCount];
            for(int j = 0; j < children.Length; j++) {
                children[j] = __instance.transform.GetChild(j);
            }
            __instance.SetFieldValue("children", children);
            __instance.cards = CardManager.activeCards.ToArray();
        }
    }
}
