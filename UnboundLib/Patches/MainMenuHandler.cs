using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnboundLib.Utils.UI;
using UnboundLib.Utils;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnboundLib.Patches
{
    [HarmonyPatch(typeof(MainMenuHandler))]
    public class MainMenuHandlerPatch
    {
        public static bool firstTime = true;
        private static TextMeshProUGUI text;


        [HarmonyPatch("Awake")]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPrefix]
        internal static void MainMenuHandlerAwake()
        {
            // reapply cards and levels
            Unbound.Instance.ExecuteAfterFrames(5, () =>
            {
                MapManager.instance.levels = LevelManager.activeLevels.ToArray();
            });

            // create unbound text
            Unbound.Instance.StartCoroutine(AddTextWhenReady(firstTime ? 2f : 0.1f));

            Debug.Log("1");
            ModOptions.instance.CreateModOptions(firstTime?firstTime:!firstTime);
            Credits.Instance.CreateCreditsMenu(firstTime);
            MainMenuLinks.AddLinks(firstTime);

            Unbound.Instance.ExecuteAfterSeconds(firstTime ? 0.4f : 0, () =>
            {
                var resumeButton = UIHandler.instance.transform.Find("Canvas/EscapeMenu/Main/Group/Resume").gameObject;
                // Create options button in escapeMenu
                var optionsMenu = Object.Instantiate(MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Options").gameObject, UIHandler.instance.transform.Find("Canvas/EscapeMenu/Main"));
                var menuBut = optionsMenu.transform.Find("Group/Back").GetComponent<Button>();
                menuBut.onClick = new Button.ButtonClickedEvent();
                menuBut.onClick.AddListener(() =>
                {
                    optionsMenu.transform.Find("Group").gameObject.SetActive(false);
                    UIHandler.instance.transform.Find("Canvas/EscapeMenu/Main/Group").gameObject.SetActive(true);
                });

                var optionsButton = Object.Instantiate(resumeButton, UIHandler.instance.transform.Find("Canvas/EscapeMenu/Main/Group"));
                optionsButton.transform.SetSiblingIndex(2);
                optionsButton.GetComponentInChildren<TextMeshProUGUI>().text = "OPTIONS";
                optionsButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                optionsButton.GetComponent<Button>().onClick.AddListener((() =>
                {
                    optionsMenu.transform.Find("Group").gameObject.SetActive(true);
                    UIHandler.instance.transform.Find("Canvas/EscapeMenu/Main/Group").gameObject.SetActive(false);
                }));
            });

            firstTime = false;
        }

        [HarmonyPatch("Close")]
        [HarmonyPrefix]
        internal static void MainMenuHandlerClose()
        {
            if (text != null) 
                Object.Destroy(text.gameObject);
        }



        private static IEnumerator AddTextWhenReady(float delay = 0f, float maxTimeToWait = 10f)
        {
            if (delay > 0f) { yield return new WaitForSecondsRealtime(delay); }

            float time = maxTimeToWait;
            while (time > 0f && MainMenuHandler.instance?.transform?.Find("Canvas/ListSelector/Main/Group") == null)
            {
                time -= Time.deltaTime;
                yield return null;
            }
            if (MainMenuHandler.instance?.transform?.Find("Canvas/ListSelector/Main/Group") == null)
            {
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
