﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Color = UnityEngine.Color;

namespace Unbound.Core.Utils.UI {

    public class ModOptions {
        internal struct subMenu {
            public string text;
            public UnityAction onClickAction;
            public int fontSize;
            public bool forceUpper;
            public Color? color;
            public TMP_FontAsset font;
            public Material fontMaterial;
            public TextAlignmentOptions? alignmentOptions;
        }
        internal static List<ModMenu> modMenus = new List<ModMenu>();
        internal static Dictionary<string, GUIListener> GUIListeners = new Dictionary<string, GUIListener>();

        internal GameObject modOptionsMenu;

        internal static bool showModUi;
        internal static bool showingModOptions;
        public static bool inPauseMenu { get; internal set; }
        internal static bool noDeprecatedMods;

        public static ModOptions instance = new ModOptions();
        internal static List<subMenu> prioritySubMenus = new List<subMenu>();
        internal static List<subMenu> subMenus = new List<subMenu>();

        private ModOptions() {
            // singleton first time setup

            instance = this;
        }

        public static bool RegesterPrioritySubMenu(string text, UnityAction onClickAction = null, int fontSize = 60, bool forceUpper = true, Color? color = null, TMP_FontAsset font = null, Material fontMaterial = null, TextAlignmentOptions? alignmentOptions = null) {
            if(prioritySubMenus.Any(menu => menu.text == text))
                return false;
            prioritySubMenus.Add(new subMenu {
                text = text,
                onClickAction = onClickAction,
                fontSize = fontSize,
                forceUpper = forceUpper,
                color = color,
                font = font,
                fontMaterial = fontMaterial,
                alignmentOptions = alignmentOptions
            });
            return true;
        }
        public static bool RegesterSubMenu(string text, UnityAction onClickAction = null, int fontSize = 60, bool forceUpper = true, Color? color = null, TMP_FontAsset font = null, Material fontMaterial = null, TextAlignmentOptions? alignmentOptions = null) {
            if(subMenus.Any(menu => menu.text == text))
                return false;
            subMenus.Add(new subMenu {
                text = text,
                onClickAction = onClickAction,
                fontSize = fontSize,
                forceUpper = forceUpper,
                color = color,
                font = font,
                fontMaterial = fontMaterial,
                alignmentOptions = alignmentOptions
            });
            return true;
        }

        public static void RegisterGUI(string modName, Action guiAction) {
            GUIListeners.Add(modName, new GUIListener(modName, guiAction));
        }

        internal void RegisterMenu(string name, UnityAction buttonAction, Action<GameObject> guiAction,
            GameObject parent = null, bool showInPauseMenu = false) {
            if(parent == null) {
                parent = instance.modOptionsMenu;
            }

            modMenus.Add(new ModMenu(name, buttonAction, guiAction, parent, showInPauseMenu));
        }

        public void CreateModOptions(bool firstTime) {
            // create mod options
            UnboundCore.Instance.ExecuteAfterSeconds(firstTime ? 0.1f : 0, () => {
                CreateModOptionsMenu(MainMenuHandler.instance.transform.Find("Canvas/ListSelector/Main").gameObject, null, false);
            });
        }

        private void CreateModOptionsMenu(GameObject parent, GameObject parentForMenu, bool pauseMenu) {
            // Create mod options menu
            modOptionsMenu = MenuHandler.CreateMenu("MODS", () => {
                showingModOptions = true;
                inPauseMenu = pauseMenu;
            }, parent
                , 60, true, false, parentForMenu,
                true, pauseMenu ? 2 : 4);

            // Create back actions 
            if(!pauseMenu) {
                modOptionsMenu.GetComponentInChildren<GoBack>(true).goBackEvent.AddListener(() => { showingModOptions = false; });
            } else {
                GameObject.Destroy(modOptionsMenu.GetComponentInChildren<GoBack>(true));
            }
            modOptionsMenu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(() => { showingModOptions = false; });

            if(!pauseMenu) {
                // Fix main menu layout
                void FixMainMenuLayout() {
                    var mainMenu = MainMenuHandler.instance.transform.Find("Canvas/ListSelector");
                    var logo = mainMenu.Find("Main/Group/Rounds_Logo2_White").gameObject.AddComponent<LayoutElement>();
                    logo.GetComponent<RectTransform>().sizeDelta =
                        new Vector2(logo.GetComponent<RectTransform>().sizeDelta.x, 80);
                    mainMenu.Find("Main").transform.position =
                        new Vector3(0, 1.7f, mainMenu.Find("Main").transform.position.z);
                    mainMenu.Find("Main/Group").GetComponent<VerticalLayoutGroup>().spacing = 10;
                }

                var visibleObj = new GameObject("visible");
                var visible = visibleObj.AddComponent<ActionOnBecameVisible>();
                visibleObj.AddComponent<SpriteRenderer>();
                visible.visibleAction += FixMainMenuLayout;
                visibleObj.transform.parent = parent.transform;
            }

            // Create sub menus

            // Create toggle levels button

            prioritySubMenus.Sort((menu1, menu2) => menu1.text.CompareTo(menu2.text));
            subMenus.Sort((menu1, menu2) => menu1.text.CompareTo(menu2.text));
            Debug.Log("Creating submenus");
            foreach(var subMenu in prioritySubMenus) {
                Debug.Log("Creating submenu: " + subMenu.text);
                MenuHandler.CreateButton(subMenu.text, modOptionsMenu, subMenu.onClickAction,
                    subMenu.fontSize, subMenu.forceUpper, subMenu.color, subMenu.font, subMenu.fontMaterial, subMenu.alignmentOptions);
            }
            if(prioritySubMenus.Any() && subMenus.Any()) MenuHandler.CreateText("---------------", modOptionsMenu, out _);
            foreach(var subMenu in subMenus) {
                Debug.Log("Creating submenu: " + subMenu.text);
                MenuHandler.CreateButton(subMenu.text, modOptionsMenu, subMenu.onClickAction,
                    subMenu.fontSize, subMenu.forceUpper, subMenu.color, subMenu.font, subMenu.fontMaterial, subMenu.alignmentOptions);
            }
            Debug.Log("Submenus created");




            // Create menu's for mods with new UI
            foreach(var menu in modMenus) {
                if(pauseMenu && !menu.showInPauseMenu) continue;
                var mmenu = MenuHandler.CreateMenu(menu.menuName,
                    menu.buttonAction,
                    modOptionsMenu,
                    60,
                    true,
                    false,
                    parentForMenu);

                void disableOldMenu() {
                    if(GUIListeners.ContainsKey(menu.menuName)) {
                        GUIListeners[menu.menuName].guiEnabled = false;
                        showModUi = false;
                    }
                }

                mmenu.GetComponentInChildren<GoBack>(true).goBackEvent.AddListener(disableOldMenu);
                mmenu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick
                    .AddListener(disableOldMenu);

                try { menu.guiAction.Invoke(mmenu); } catch(Exception e) {
                    UnityEngine.Debug.LogError($"Exception thrown when attempting to build menu '{menu.menuName}', see log below for details.");
                    UnityEngine.Debug.LogException(e);
                }
            }

            // Create menu's for mods that do not use the new UI
            if(GUIListeners.Count != 0) {
                MenuHandler.CreateText(" ", modOptionsMenu, out _);
                if(pauseMenu) MenuHandler.CreateText("SOME OF THESE MOD SETTINGS BELOW MIGHT BREAK YOUR GAME\n<color=red>YOU HAVE BEEN WARNED</color>", modOptionsMenu, out _, 35, false);
            }

            foreach(var modMenu in GUIListeners.Keys) {
                var menu = MenuHandler.CreateMenu(modMenu, () => {
                    foreach(var list in GUIListeners.Values.Where(list => list.guiEnabled)) {
                        list.guiEnabled = false;
                    }

                    GUIListeners[modMenu].guiEnabled = true;
                    showModUi = true;
                }, modOptionsMenu,
                    75, true, true, parentForMenu);

                void disableOldMenu() {
                    if(GUIListeners.ContainsKey(menu.name)) {
                        GUIListeners[menu.name].guiEnabled = false;
                        showModUi = false;
                    }
                }

                menu.GetComponentInChildren<GoBack>(true).goBackEvent.AddListener(disableOldMenu);
                menu.transform.Find("Group/Back").gameObject.GetComponent<Button>().onClick.AddListener(disableOldMenu);
                MenuHandler.CreateText(
                    "This mod has not yet been updated to the new UI system.\nPlease use the old UI system in the top left.",
                    menu, out _, 60, false);
            }

            // check if there are no deprecated ui's and disable the f1 menu
            if(GUIListeners.Count == 0) noDeprecatedMods = true;
        }

        internal class ModMenu {
            public string menuName;
            public UnityAction buttonAction;
            public Action<GameObject> guiAction;
            public GameObject parent;
            public bool showInPauseMenu;

            public ModMenu(string menuName, UnityAction buttonAction, Action<GameObject> guiAction, GameObject parent, bool showInPauseMenu = false) {
                this.menuName = menuName;
                this.buttonAction = buttonAction;
                this.guiAction = guiAction;
                this.parent = parent;
                this.showInPauseMenu = showInPauseMenu;
            }
        }

        internal class GUIListener {
            public bool guiEnabled;
            public string modName;
            public Action guiAction;

            public GUIListener(string modName, Action guiAction) {
                this.modName = modName;
                this.guiAction = guiAction;
            }
        }
    }
}