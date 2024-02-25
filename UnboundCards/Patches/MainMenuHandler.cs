using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unbound.Cards.Utils;
using Unbound.Core;
using Unbound.Core.Patches;
using Unbound.Core.Utils.UI;
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

            Unbound.Core.UnboundCore.Instance.ExecuteAfterFrames(5, () =>
            {
                CardManager.RestoreCardToggles();
                ToggleCardsMenuHandler.RestoreCardToggleVisuals();
            });

            Unbound.Core.UnboundCore.Instance.ExecuteAfterSeconds(0.4f, () => 
                CardManager.FirstTimeStart());
            
        }
    }
}
