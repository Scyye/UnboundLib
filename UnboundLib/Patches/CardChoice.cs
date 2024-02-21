using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.Utils;
using UnityEngine;

namespace UnboundLib.Patches
{
    internal class CardChoicePatch
    {
        [HarmonyPatch(typeof(CardChoice), "Start")]
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
