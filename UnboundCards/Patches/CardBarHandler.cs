﻿using HarmonyLib;
using UnboundLib.Cards;

namespace UnboundLib.Cards.Patches
{

    [HarmonyPatch(typeof(CardBarHandler), "AddCard")]
    class CardBarHandler_Patch
    {
        static void Prefix(int teamId, CardInfo card)
        {
            CardData.AddCard(teamId, card.CardName);
        } 
    }
}
