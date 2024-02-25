using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnboundLib.Cards.Patches
{
    [HarmonyPatch(typeof(CardBar), nameof(CardBar.OnHover))]
    class CardBar_Patch
    {
        [HarmonyPatch(new Type[] { typeof(CardBarButton) })]
        static void Postfix(CardBar __instance, CardBarButton cardButton)
        {
            var currentCard = (GameObject) AccessTools.Field(typeof(CardBar), "m_currentCard").GetValue(__instance);
            currentCard.SetActive(true);
        }
    }
}
