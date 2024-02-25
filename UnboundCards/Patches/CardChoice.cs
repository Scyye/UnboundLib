using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.Cards.Utils;
using UnboundLib;
using UnboundLib.Utils;
using UnityEngine;

namespace UnboundLib.Cards.Patches
{
    [HarmonyPatch(typeof(CardChoice))]
    internal class CardChoicePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CardChoice.GetSourceCard))]
        private static bool CheckHiddenCards(CardChoice __instance, CardInfo info, ref CardInfo __result)
        {
            for (int i = 0; i < __instance.cards.Length; i++)
            {
                if ((__instance.cards[i].gameObject.name + "(Clone)") == info.gameObject.name)
                {
                    __result = __instance.cards[i];
                    return false;
                }
            }
            __result = null;

            return false;
        }
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void CardChoiceStartPost(CardChoice __instance)
        {
            for (int i = 0; i < __instance.cards.Length; i++)
            {
                if (!((DefaultPool) PhotonNetwork.PrefabPool).ResourceCache.ContainsKey(__instance.cards[i].gameObject.name))
                    PhotonNetwork.PrefabPool.RegisterPrefab(__instance.cards[i].gameObject.name, __instance.cards[i].gameObject);
            }
            var children = new Transform[__instance.transform.childCount];
            for (int j = 0; j < children.Length; j++)
            {
                children[j] = __instance.transform.GetChild(j);
            }
            __instance.SetFieldValue("children", children);
            __instance.cards = CardManager.activeCards.ToArray();
        }
    }
}
