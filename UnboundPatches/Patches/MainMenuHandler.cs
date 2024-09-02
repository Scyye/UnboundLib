using HarmonyLib;
using System.Collections;
using System.Linq;
using TMPro;
using Unbound.Core.Utils;
using Unbound.Core.Utils.UI;
using UnboundLib.Networking.Lobbies;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Unbound.Core.Patches {
    [HarmonyPatch(typeof(MainMenuHandler))]
    public class MainMenuHandlerPatch {
        public static bool firstTime = true;
        private static TextMeshProUGUI text;


        [HarmonyPatch("Awake")]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPrefix]
        internal static void MainMenuHandlerAwake() {
            // reapply cards and levels
            UnboundCore.Instance.ExecuteAfterFrames(5, () => {
                MapManager.instance.levels = LevelManager.activeLevels.ToArray();
            });

            // create unbound text
            UnboundCore.Instance.StartCoroutine(AddTextWhenReady(firstTime ? 2f : 0.1f));

            Debug.Log("1");
            ModOptions.instance.CreateModOptions(firstTime ? firstTime : !firstTime);
            Credits.Instance.CreateCreditsMenu(firstTime);
            MainMenuLinks.AddLinks(firstTime);

            var hostButton = GameObject.Find("/Game/UI/UI_MainMenu/Canvas/ListSelector/Online/Group/Invite friend/").GetComponent<Button>();
            hostButton.onClick.RemoveAllListeners();
            hostButton.onClick.AddListener(delegate {
                Unbound_​Lobby.Host();
               // MainMenuHandler.instance.Close();
               // LoadingScreen.instance?.StartLoading();
            });

            firstTime = false;
        }

        [HarmonyPatch("Close")]
        [HarmonyPrefix]
        internal static void MainMenuHandlerClose() {
            if(text != null)
                Object.Destroy(text.gameObject);
        }



        private static IEnumerator AddTextWhenReady(float delay = 0f, float maxTimeToWait = 10f) {
            if(delay > 0f) { yield return new WaitForSecondsRealtime(delay); }

            float time = maxTimeToWait;
            while(time > 0f && MainMenuHandler.instance?.transform?.Find("Canvas/ListSelector/Main/Group") == null) {
                time -= Time.deltaTime;
                yield return null;
            }
            if(MainMenuHandler.instance?.transform?.Find("Canvas/ListSelector/Main/Group") == null) {
                yield break;
            }
            text = MenuHandler.CreateTextAt("UNBOUND", Vector2.zero);
            text.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            text.fontSize = 30;
            text.color = (Color.yellow + Color.red) / 2;
            text.transform.SetParent(MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Main/Group"), true);
            text.transform.SetAsFirstSibling();
            text.rectTransform.localScale = Vector3.one;
            text.rectTransform.localPosition = new Vector3(0, 350, text.rectTransform.localPosition.z);
        }
    }
}
