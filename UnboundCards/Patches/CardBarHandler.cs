using HarmonyLib;
using Unbound.Cards;

namespace Unbound.Cards.Patches
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
