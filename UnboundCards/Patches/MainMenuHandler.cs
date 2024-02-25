using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unbound.Cards.Utils;
using UnboundLib;
using UnboundLib.Patches;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace Unbound.Cards.Patches
{
    [HarmonyPatch(typeof(MainMenuHandler))]
    internal class MainMenuHandlerPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        internal static void MainMenuHandlerAwake()
        {
            Debug.Log("0");

            UnboundLib.Unbound.Instance.ExecuteAfterFrames(5, () =>
            {
                CardManager.RestoreCardToggles();
                ToggleCardsMenuHandler.RestoreCardToggleVisuals();
            });

            UnboundLib.Unbound.Instance.ExecuteAfterSeconds(0.4f, () => 
                CardManager.FirstTimeStart());
            
        }
    }
}
