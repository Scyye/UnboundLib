using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.Cards;

namespace UnboundLib.Cards.Patches
{
    [HarmonyPatch(typeof(Player), "FullReset")]
    class PlayerFullResetPatch
    {
        static void Postfix(Player __instance)
        {
            foreach (CardInfo currentCard in __instance.data.currentCards)
            {
                if (currentCard.GetComponent<CustomCard>() is CustomCard customCard)
                {
                    try
                    {
                        Gun gun = __instance.GetComponent<Holding>().holdable.GetComponent<Gun>();
                        CharacterData characterData = __instance.GetComponent<CharacterData>();
                        HealthHandler healthHandler = __instance.GetComponent<HealthHandler>();
                        Gravity gravity = __instance.GetComponent<Gravity>();
                        Block block = __instance.GetComponent<Block>();
                        GunAmmo gunAmmo = gun.GetComponentInChildren<GunAmmo>();
                        CharacterStatModifiers characterStatModifiers = __instance.GetComponent<CharacterStatModifiers>();
                        customCard.OnRemoveCard(__instance, gun, gunAmmo, characterData, healthHandler, gravity, block, characterStatModifiers);
                    }
                    catch (NotImplementedException)
                    { }
                    catch (Exception exception)
                    {
                        UnityEngine.Debug.LogError($"{exception.GetType()}\nThrown by: {customCard.GetModName()} - {currentCard.CardName} - OnRemoveCard()");
                        UnityEngine.Debug.LogException(exception);
                    }
                }
            }
            __instance.data.currentCards.Clear();
        }
    }
}
