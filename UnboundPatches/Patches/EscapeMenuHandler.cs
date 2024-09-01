using HarmonyLib;
using Unbound.Cards.Utils;
using Unbound.Gamemodes.Utils;
using UnityEngine;
using UnityEngine.UI;
using InControl;
using UnityEngine;
using Photon.Pun;

namespace Unbound.Core.Patches {
    [HarmonyPatch(typeof(EscapeMenuHandler))]
    public class EscapeMenuHandlerPath {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void Start(EscapeMenuHandler __instance) {
            __instance.transform.Find("Main/Group/Menu").GetComponent<Button>().onClick.RemoveAllListeners();
            __instance.transform.Find("Main/Group/Menu").GetComponent<Button>().onClick.AddListener(delegate { GameManager.instance.GoToMenu(); }) ;
        }
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static bool Update(EscapeMenuHandler __instance) {
            if(LoadingScreen.instance.IsLoading) {
                if(Input.GetKeyDown(KeyCode.Escape)) {
                    GameManager.instance.GoToMenu();
                }

                for(int i = 0; i < InputManager.ActiveDevices.Count; i++) {
                    if(InputManager.ActiveDevices[i].Action2.WasPressed) {
                        PhotonNetwork.Disconnect();
                        GameManager.instance.GoToMenu();
                    }
                }
                return false;
            }
            return true;
        }
    }
}